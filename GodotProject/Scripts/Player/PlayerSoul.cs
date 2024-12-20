using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using Godot.NativeInterop;


namespace Fcc{

	public interface ITransferrable{
		public Shape2D RequiredSpace{get;}
		public Vector2 RenderSize{get;}
		public uint RequiredLayer{get;}
		public void Store();
		public void Unstore();
	}
	public partial class PlayerSoul : CharacterBody2D{
		public static float maxHSpeed => 500.0f;
		public static float maxHRoboSpeed => 200.0f;
		public static float hAccel => 2000.0f;
		public static float hGroundDrag => 1500.0f;
		public static float hAirDrag => 0.02f;
		public static float airHMultiplier => 0.7f;
		public static float jumpHVel => 50.0f;
		public static float jumpVVel => 500.0f;
		public static float gravity => 3200.0f;
		public static float maxFallSpeed => 600.0f;
		public static float jumpGravityMultiplier => 0.15f;
		public static ulong jumpFrames => 12;
		public static ulong wolfFrames => 6;
		public static ulong preFrames => 10;
		public static ulong jumpTurnFrames => 4;
		CharacterBody2D bodyb;

		ITransferrable storedObject = null;

		[Export]
		Area2D possessArea;
		CharacterBody2D cb;
		GeneralLevel level;

		[Export]
		AudioStream jumpAudio;
		[Export]
		AudioStream dieAudio;

		public bool isSoulForm = false;

		public bool canOperate = true;
		ulong frameCounter = 0;
		ulong wolfTill = 0;
		ulong preTill = 0;
		ulong jumpTill = 0;
		ulong jumpTurnTill = 0;
		[Export]
		float storeRadius = 144.0f;

		AudioStreamPlayer PlayAudio(AudioStream astr){
			var asp = new AudioStreamPlayer();
			AddChild(asp);
			asp.Stream = astr;
			asp.Play();
			asp.Finished += () => asp.CallDeferred(MethodName.Free);
			
			return asp;

		}
		public async void Kill(CharacterBody2D victim){
			if(!canOperate)return;
			if (Input.IsActionJustPressed("reset")) level.loader.Reset();
			PlayAudio(dieAudio);
			GetChild<AnimatedSprite2D>(2).Play("die");
			bodyb.GetChild<AnimatedSprite2D>(2).Play("die");
			canOperate = false;
			cb.Velocity = Vector2.Zero;
			GD.Print("soul got killed");
			for(int i = 0; i < 30; ++i)await level.physicsUpdate.Wait();
			await level.loader.PlayTransOut(victim.GlobalPosition);
			for(int i = 0; i < 10; ++i)await level.physicsUpdate.Wait();
			level.loader.Reset();
		}
		public void Project(){
			var anim = GetChild<AnimatedSprite2D>(2);
			var canim = cb.GetChild<AnimatedSprite2D>(2);
			anim.Play("hover");
			anim.FlipH = canim.FlipH;
			GlobalPosition = cb.GlobalPosition;
			Visible = true;
			Velocity = cb.Velocity;
			(cb as IPossessable).Unpossess();
			cb = this;
			isSoulForm = true;
			GD.Print("Project");
			level.loader.BGM1asp.VolumeDb = Mathf.LinearToDb(0);
			level.loader.BGM2asp.VolumeDb = level.loader.volume;
		}
		void PlayerMove(float dt){
			AnimatedSprite2D renderer = cb.GetChild<AnimatedSprite2D>(2);
			Vector2 vel = cb.Velocity;
			
			float hAxis = Input.GetAxis("ui_left", "ui_right");
			{
				int sgn = Mathf.Sign(vel.X);
				if(sgn == 0)	sgn = Mathf.Sign(hAxis);
				vel.X *= 1.0f - hAirDrag;
				renderer.FlipH = (hAxis == -1f) ? true : (hAxis == 1f) ? false : renderer.FlipH;
				float abVelX = sgn * vel.X;
				float dv = 0.0f;
				float ms = cb is Robot ? maxHRoboSpeed : maxHSpeed;
				if(sgn * hAxis < 0.0f || abVelX < ms)	dv += sgn * hAxis * hAccel;
				if(cb.IsOnFloor())	dv += (sgn * hAxis - 1.0f) * hGroundDrag;
				else dv *= airHMultiplier;
				abVelX += dv * dt;
				if(abVelX < 0.0f)
					abVelX = 0.0f;
				vel.X = abVelX * sgn;
			}
			if(cb.IsOnFloor()){
				if(hAxis == 0){
					if(!renderer.IsPlaying() || renderer.Animation == "run")renderer.Play("idle");
				} else {
					if(!renderer.IsPlaying() || renderer.Animation == "idle")renderer.Play("run");
				}
				wolfTill = frameCounter + wolfFrames;
			}
			if(Input.IsActionJustPressed("ui_accept"))	preTill = frameCounter + preFrames;
			if(frameCounter < wolfTill && frameCounter < preTill && !(cb is Robot)){
				renderer.Play("jump");GD.Print("jump");
				var a = PlayAudio(jumpAudio);
				a.PitchScale = 1.0f + 0.3f * 2.0f * (GD.Randf() - 0.5f);
				wolfTill = preTill = 0;
				jumpTill = frameCounter + jumpFrames;
				jumpTurnTill = frameCounter + jumpTurnFrames;
				vel.Y = -jumpVVel;
				vel.X += hAxis * jumpHVel;
			}
			if(frameCounter < jumpTurnTill && vel.X * hAxis < 0.0f){
				jumpTurnTill = 0;
				vel.X = 2 * hAxis * jumpHVel;
			}
			if(!Input.IsActionPressed("ui_accept"))jumpTurnTill = jumpTill = 0;
			vel.Y += dt * gravity * (frameCounter < jumpTill ? jumpGravityMultiplier : 1.0f);
			vel.Y = Mathf.Clamp(vel.Y, -Mathf.Inf, maxFallSpeed);
			if (!(cb is Robot) && renderer.Animation != "hover" && (
					!renderer.IsPlaying() && renderer.GetAnimation() == "jump"
					|| !cb.IsOnFloor()
				)){
				renderer.SetAnimation("hover");GD.Print("hover");
			}
			//GD.Print(wasOnFloor);
			if(renderer.Animation == "hover" && cb.IsOnFloor()){
				renderer.Play("land");GD.Print("land");
			}
			cb.Velocity = vel;
			cb.MoveAndSlide();
		}
		void PlayerIdle(float dt){
			var anim = cb.GetChild<AnimatedSprite2D>(2);
			if(anim.Animation != "idle")anim.Play("idle");
			Vector2 vel = cb.Velocity;
			vel.X *= 1.0f - hAirDrag;
			float sgn = Mathf.Sign(vel.X);
			float abVelX = sgn * vel.X;
			abVelX -= dt * hGroundDrag * (cb.IsOnFloor() ? 1.0f : airHMultiplier);
			if(abVelX < 0.0f)abVelX = 0.0f;
			vel.X = abVelX * sgn;
			vel.Y += dt * gravity;
			cb.Velocity = vel;
			cb.MoveAndSlide();
		}
		public async void Init(PlayerBody body, GeneralLevel lv){
			level = lv;
			bodyb = body;
			cb = bodyb;
			Visible = false;
			level.loader.BGM1asp.VolumeDb = level.loader.volume;
			level.loader.BGM2asp.VolumeDb = Mathf.LinearToDb(0);
			lv.CallDeferred(Node.MethodName.AddChild, this);
			level.loader?.PlayTransIn(body.Position);
			IPossessable ps = null;
			for(;;){
				float dt = await level.physicsUpdate.Wait();
				++frameCounter;
				if(!isSoulForm)GlobalPosition = cb.GlobalPosition;
				if(!canOperate)continue;
				if(Input.IsActionJustPressed("reset")){
					canOperate = false;
					await level.loader.PlayTransOut(GlobalPosition);
					level.loader.Reset();
					return;
				}
				if(isSoulForm && GetChild<Area2D>(3).HasOverlappingBodies())Kill(this);
				{
					var v = GetViewport();
					if(v != null){
						var c = v.GetCamera2D();
						if(c != null)c.Position = GlobalPosition;
					}
				}
				if(isSoulForm){
					if(ps != null)(ps as Node2D).Modulate = new Color("#ffffff", 1.0f);
					ps = null;
					foreach(var n in possessArea.GetOverlappingBodies()){
						if(n is IPossessable && !(n as IPossessable).discarded){
							ps = n as IPossessable;
							break;
						}
					}
					if(ps != null)(ps as Node2D).Modulate = new Color("#df55ff", 1.0f);
				}
				if(Input.IsActionJustPressed("soul_projection")){
					if(!isSoulForm){
						Project();
					}else{
						if(ps != null){
							//level.CallDeferred(Node.MethodName.RemoveChild, this);
							Transform = Transform2D.Identity;
							cb = ps as CharacterBody2D;
							cb.Modulate = new Color("#ffffff", 1.0f);
							Visible = false;
							//cb.CallDeferred(Node.MethodName.AddChild, this);
							ps.Possess(this);
							isSoulForm = false;
							wolfTill = jumpTill = 0;
							level.loader.BGM1asp.VolumeDb = level.loader.volume;
							level.loader.BGM2asp.VolumeDb = Mathf.LinearToDb(0);
						}
					}
				}
				if(Input.IsActionJustPressed("transfer")){
					if(storedObject == null){
						Area2D mark = GD.Load<PackedScene>("res://Scenes/Objects/TransferMark.tscn").Instantiate<Area2D>();
						mark.Position = GlobalPosition;
						level.CallDeferred(Node.MethodName.AddChild, mark);
						for(;;){
							var bs = mark.GetOverlappingBodies();
							if(bs.Count != 0)
								foreach(var b in bs){
									if(b is ITransferrable){
										if(storedObject == b)break;
										if(storedObject != null)(storedObject as CanvasItem).Modulate = new Color("#ffffff", 1.0f);
										storedObject = b as ITransferrable;
										(storedObject as CanvasItem).Modulate = new Color("#007fff", 1.0f);
										break;
									}
								}
							else{
								if(storedObject != null)(storedObject as CanvasItem).Modulate = new Color("#ffffff", 1.0f);
								storedObject = null;
							}
							if(Input.IsActionJustReleased("transfer")){
								mark.CallDeferred(Node.MethodName.Free);
								if(storedObject == null)break;
								if(cb == storedObject)Project();
								level.UI.TransferSlot.Size = (Vector2I)storedObject.RenderSize;
								level.UI.TransferRender.Size = storedObject.RenderSize;
								(storedObject as CanvasItem).Modulate = new Color("#ffffff", 1.0f);
								(storedObject as Node).GetParent().CallDeferred(Node.MethodName.RemoveChild, storedObject as Node);
								level.UI.TransferSlot.CallDeferred(Node.MethodName.AddChild, storedObject as Node);
								(storedObject as Node2D).Position = Vector2.Zero;
								
								storedObject.Store();
								break;
							}
							PlayerIdle(dt);
							dt = await level.physicsUpdate.Wait();
							++frameCounter;
							Vector2 v = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
							mark.Translate(v * dt * 400.0f);
							Vector2 dis = mark.Position - GlobalPosition;
							if(dis.LengthSquared() > storeRadius*storeRadius){
								mark.Position = GlobalPosition + dis.Normalized() * storeRadius;
							}
						}
					}else{
						Area2D detectArea = new Area2D();
						var sm = new ShaderMaterial();{
							ColorRect cr = new ColorRect();
							cr.SetSize(storedObject.RenderSize);
							var lm = level.UI.TransferRender.Material as ShaderMaterial;
							
							sm.Shader = GD.Load<Shader>("res://Shaders/TransferIndicater.gdshader");
							sm.SetShaderParameter("subv", lm.GetShaderParameter("subv"));
							sm.SetShaderParameter("modu", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
							cr.Material = sm;
							cr.Position = -cr.Size / 2.0f;
							detectArea.AddChild(cr);
							var shape = new CollisionShape2D();
							shape.Shape = storedObject.RequiredSpace;
							shape.Scale = (storedObject as Node2D).GlobalScale;
							detectArea.AddChild(shape);
							detectArea.Position = GlobalPosition;
							detectArea.CollisionMask = storedObject.RequiredLayer;
						}
						level.CallDeferred(Node.MethodName.AddChild, detectArea);
						
						for(;;){
							sm.SetShaderParameter("modu", detectArea.HasOverlappingBodies() ? 
							new Vector4(1.0f, 0.3f, 0.3f, 1.0f) : new Vector4(0.3f, 1.0f, 0.3f, 1.0f));
							if(Input.IsActionJustReleased("transfer")){
								if(!detectArea.HasOverlappingBodies()){
									level.UI.TransferSlot.CallDeferred(Node.MethodName.RemoveChild, storedObject as Node);
									(storedObject as Node2D).Position = detectArea.Position;
									level.CallDeferred(Node.MethodName.AddChild, storedObject as Node);
									storedObject.Unstore();
									storedObject = null;
								}
								detectArea.CallDeferred(Node.MethodName.Free);
								break;
							}
							PlayerIdle(dt);
							dt = await level.physicsUpdate.Wait();
							++frameCounter;
							Vector2 v = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
							detectArea.Position = detectArea.Position + v * dt * 400.0f;
							Vector2 dis = detectArea.Position - GlobalPosition;
							if(dis.LengthSquared() > storeRadius * storeRadius){
								detectArea.Position = GlobalPosition + dis.Normalized() * storeRadius;
							}
						}
					}
				}
				PlayerMove(dt);
			}
		}

	}
}

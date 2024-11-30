using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Godot;


namespace Fcc{

	public partial class Robot : CharacterBody2D, ILevelObject, IPossessable, ITransferrable{
		PlayerSoul soul = null;
		bool discarded = false;
		bool stored = false;
		public uint RequiredLayer => 4;

		public Shape2D RequiredSpace => GetChild<CollisionShape2D>(0).Shape;

		public Vector2 RenderSize => new Vector2(200, 124);

		public void Possess(PlayerSoul ps){
			soul = ps;
		}
		public void Unpossess(){
			soul = null;
			GetChild<AnimatedSprite2D>(2).Play("idle");
		}
		public void Kill(){
			if(discarded)return;
			discarded = true;
			Velocity = Vector2.Zero;
			soul?.Project();
			GD.Print("robot got killed");
			CallDeferred(Node.MethodName.Free);
			
		}
		public async void FeedLevelInstance(GeneralLevel lev){
			GetChild<Area2D>(1).BodyEntered += b => b.CallDeferred(MethodName.Free);
			GetChild<Area2D>(3).BodyEntered += _ => Kill();
			for(;;){
				float dt = await lev.physicsUpdate.Wait();
				if(discarded)return;
				if(stored)continue;
				if(soul == null){
					Vector2 vel = Velocity;
					int sgn = Mathf.Sign(vel.X);
					float abVelX = sgn * vel.X;
					float dv = -dt * PlayerSoul.hGroundDrag * (IsOnFloor() ? 1.0f : PlayerSoul.airHMultiplier);
					abVelX = Mathf.Clamp(abVelX + dv, 0, PlayerSoul.maxHSpeed);
					vel.X = abVelX * sgn;
					vel.Y += dt * PlayerSoul.gravity;
					Velocity = vel;
					MoveAndSlide();
				}
			}
		}

		public void Store(){
			stored = true;
		}
		public void Unstore(){
			stored = false;
		}

	}
}

using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Godot;


namespace Fcc{
	public interface IPossessable{
		public void Possess(PlayerSoul soul);
		public void Unpossess();
		public void Kill();
	}
	public partial class PlayerBody : CharacterBody2D, ILevelObject, IPossessable{
		public bool possessed = true;
		public void Possess(PlayerSoul soul){
			possessed = true;
		}
		public void Unpossess(){
			possessed = false;
		}
		PlayerSoul soul = GD.Load<PackedScene>("res://Scenes/Objects/PlayerSoul.tscn").Instantiate() as PlayerSoul;
		public void Kill(){
			if(!soul.canOperate)return;
			Velocity = Vector2.Zero;
			GD.Print("body got killed");
			soul.Project();
			soul.Kill();
		}
		public async void FeedLevelInstance(GeneralLevel level){
			possessed = true;
			soul.Init(this, level);
			Area2D endDetect = GetChild<Area2D>(1);
			for(;;){
				float dt = await level.physicsUpdate.Wait();
				if(!possessed){
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
				if(possessed && soul.canOperate ){
					var arr = endDetect.GetOverlappingAreas();
					if(arr.Count != 0){
						soul.canOperate = false;
						GD.Print("Congratulations");
						var exitNode = arr[0];
						exitNode.GetChild<AnimatedSprite2D>(1).Play("open");
					}
				}
			}
		}

	}
}

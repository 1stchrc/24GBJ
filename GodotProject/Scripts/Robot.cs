using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Godot;


namespace Fcc{

    public partial class Robot : CharacterBody2D, ILevelObject, IPossessable{
		PlayerSoul soul = null;
		bool discarded = false;
		public void Possess(PlayerSoul ps){
			soul = ps;
		}
		public void Unpossess(){
			soul = null;
		}
        public void Kill(){
			if(discarded)return;
			discarded = true;
            Velocity = Vector2.Zero;
			soul.Project();
            GD.Print("robot got killed");
			CallDeferred(Node.MethodName.Free);
			
        }
        public async void FeedLevelInstance(TestLevel lev){
            for(;;){
                float dt = await lev.physicsUpdate.Wait();
				if(discarded)return;
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

    }
}
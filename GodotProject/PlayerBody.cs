using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Godot;


namespace Fcc{
    public partial class PlayerBody : CharacterBody2D, ILevelObject{
        static float maxHSpeed => 500.0f;
        static float hAccel => 2000.0f;
        static float hDrag => 1500.0f;
        static float airHMultiplier => 0.7f;
        static float jumpHVel => 50.0f;
        static float jumpVVel => 500.0f;
        static float gravity => 3200.0f;
        static float maxFallSpeed => 500.0f;
        static float jumpGravityMultiplier => 0.15f;
        static ulong jumpFrames => 12;
        static ulong wolfFrames => 6;
        static ulong preFrames => 6;

        public async void FeedLevelInstance(TestLevel level){
			
			CharacterBody2D bodyb = this;
			CharacterBody2D soulb = GD.Load<PackedScene>("res://PlayerSoul.tscn").Instantiate() as CharacterBody2D;

            CharacterBody2D cb = bodyb;
            CharacterBody2D bb = null;

            bool isSoulForm = false;

            ulong frameCounter = 0;
            ulong wolfTill = 0;
            ulong preTill = 0;
            ulong jumpTill = 0;
            for(;;){
                float dt = await level.physicsUpdate.Wait();
                if(Input.IsActionJustPressed("soul_projection")){
                    if(!isSoulForm){
                        bodyb.GetParent().AddChild(soulb);
                        soulb.Transform = new Transform2D(bodyb.Transform.X, bodyb.Transform.Y, bodyb.Transform.Origin);
                        soulb.Velocity = bodyb.Velocity;
                        cb = soulb;
                        bb = bodyb;
                        bodyb.CollisionLayer = 1;
                        bodyb.CollisionMask = 4;
                        isSoulForm = true;
                    }else{
                        var dis = soulb.Transform.Origin - bodyb.Transform.Origin;
                        float sqrmgn = dis.X * dis.X + dis.Y + dis.Y;
                        if(sqrmgn <= 64.0f * 64.0f){
                            soulb.GetParent().RemoveChild(soulb);
                            cb = bodyb;
                            bb = null;
                            bodyb.CollisionLayer = 3;
                            bodyb.CollisionMask = 12;
                            isSoulForm = false;
                            wolfTill = jumpTill = 0;
                        }
                    }
                }
                {
                    Vector2 vel = cb.Velocity;
                    float hAxis = Input.GetAxis("ui_left", "ui_right");
                    {
                        int sgn = Mathf.Sign(vel.X);
                        if(sgn == 0)sgn = Mathf.Sign(hAxis);
                        float abVelX = sgn * vel.X;
                        float dv = dt * (sgn * hAxis * hAccel + (sgn * hAxis - 1.0f) * hDrag) * (cb.IsOnFloor() ? 1.0f : airHMultiplier);
                        abVelX = Mathf.Clamp(abVelX + dv, 0, maxHSpeed);
                        vel.X = abVelX * sgn;
                    }
                    ++frameCounter;
                    if(cb.IsOnFloor())wolfTill = frameCounter + wolfFrames;
                    if(Input.IsActionJustPressed("ui_accept"))preTill = frameCounter + preFrames;
                    if(frameCounter < wolfTill && frameCounter < preTill){
                        wolfTill = preTill = 0;
                        jumpTill = frameCounter + jumpFrames;
                        vel.Y = -jumpVVel;
                        vel.X += hAxis *jumpHVel;
                    }
                    if(!Input.IsActionPressed("ui_accept"))jumpTill = 0;
                    vel.Y += dt * gravity * (frameCounter < jumpTill ? jumpGravityMultiplier : 1.0f);
                    vel.Y = Mathf.Clamp(vel.Y, -Mathf.Inf, maxFallSpeed);
                    cb.Velocity = vel;
                    cb.MoveAndSlide();
                }
                if(bb != null){
                    Vector2 vel = bb.Velocity;
                    int sgn = Mathf.Sign(vel.X);
                    float abVelX = sgn * vel.X;
                    float dv = -dt * hDrag * (cb.IsOnFloor() ? 1.0f : airHMultiplier);
                    abVelX = Mathf.Clamp(abVelX + dv, 0, maxHSpeed);
                    vel.X = abVelX * sgn;
                    vel.Y += dt * gravity;
                    bb.Velocity = vel;
                    bb.MoveAndSlide();
                }
            }
        }

    }
}
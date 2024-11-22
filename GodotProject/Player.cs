using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Godot;


namespace Fcc{
    public static class PlayerSpirit{
        static float maxHSpeed => 400.0f;
        static float hAccel => 3000.0f;
        static float hDrag => 1000.0f;
        static float airHMultiplier => 0.7f;
        static float jumpHVel => 50.0f;
        static float jumpVVel => 400.0f;
        static float gravity => 2500.0f;
        static float maxFallSpeed => 500.0f;
        static float jumpGravityMultiplier => 0.15f;
        static int jumpFrames => 13;
        static int wolfFrames => 6;
        static int preFrames => 6;
        

        public static async void Possess(
            CharacterBody2D bodyb, 
            CharacterBody2D soulb, 
            EventSrc<float> phu){
            
            var counterEv = new EventSrc<int>();

            var wolfCounter = new EventCounter<int>(counterEv);
            var preCounter = new EventCounter<int>(counterEv);
            var jmpCounter = new EventCounter<int>(counterEv);

            CharacterBody2D cb = bodyb;
            CharacterBody2D bb = null;
            soulb.GetParent().RemoveChild(soulb);
            bool isSoulForm = false;
            for(;;){
                float dt = await phu.Wait();
                if(Input.IsActionJustPressed("soul_projection")){
                    if(!isSoulForm){
                        bodyb.GetParent().AddChild(soulb);
                        soulb.Transform = new Transform2D(bodyb.Transform.X, bodyb.Transform.Y, bodyb.Transform.Origin);
                        soulb.Velocity = bodyb.Velocity;
                        cb = soulb;
                        bb = bodyb;
                        isSoulForm = true;
                    }else{
                        var dis = soulb.Transform.Origin - bodyb.Transform.Origin;
                        float sqrmgn = dis.X * dis.X + dis.Y + dis.Y;
                        if(sqrmgn <= 64.0f * 64.0f){
                            soulb.GetParent().RemoveChild(soulb);
                            cb = bodyb;
                            bb = null;
                            isSoulForm = false;
                            wolfCounter.Rewind(0);
                            jmpCounter.Rewind(0);
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
                        float dv = dt * (sgn * hAccel * hAxis - hDrag) * (cb.IsOnFloor() ? 1.0f : airHMultiplier);
                        abVelX = Mathf.Clamp(abVelX + dv, 0, maxHSpeed);
                        vel.X = abVelX * sgn;
                    }
                    counterEv.Emit(0);
                    if(cb.IsOnFloor())wolfCounter.Rewind(wolfFrames);
                    if(Input.IsActionJustPressed("ui_accept"))preCounter.Rewind(preFrames);
                    if(!wolfCounter.elapsed && !preCounter.elapsed){
                        wolfCounter.Rewind(0);
                        preCounter.Rewind(0);
                        jmpCounter.Rewind(jumpFrames);
                        vel.Y = -jumpVVel;
                        vel.X += hAxis *jumpHVel;
                    }
                    if(!Input.IsActionPressed("ui_accept"))jmpCounter.Rewind(0);
                    vel.Y += dt * gravity * (jmpCounter.elapsed ? 1.0f : jumpGravityMultiplier);
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
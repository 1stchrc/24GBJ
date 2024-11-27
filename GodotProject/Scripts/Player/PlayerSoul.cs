using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Godot;


namespace Fcc{
    public partial class PlayerSoul : CharacterBody2D{
        public static float maxHSpeed => 500.0f;
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

        [Export]
        Area2D possessArea;
        CharacterBody2D cb;
        TestLevel level;

        bool isSoulForm = false;

        public bool canOperate = true;
        ulong frameCounter = 0;
        ulong wolfTill = 0;
        ulong preTill = 0;
        ulong jumpTill = 0;
        ulong jumpTurnTill = 0;
        public void Kill(){
            if(!canOperate)return;
            canOperate = false;
            cb.Velocity = Vector2.Zero;
			GD.Print("soul got killed");
        }
        
        public void Project(){
            //cb.RemoveChild(this);
            GetParent().CallDeferred(Node.MethodName.RemoveChild, this);
            level.CallDeferred(Node.MethodName.AddChild, this);
            Transform = new Transform2D(cb.Transform.X, cb.Transform.Y, cb.Transform.Origin);
            Visible = true;
            Velocity = cb.Velocity;
            (cb as IPossessable).Unpossess();
            cb = this;
            isSoulForm = true;
        }
        public async void Init(PlayerBody body, TestLevel lv){
            level = lv;
			bodyb = body;
			cb = bodyb;
            Visible = false;
            cb.CallDeferred(Node.MethodName.AddChild, this);
            for(;;){
                float dt = await level.physicsUpdate.Wait();
                if(!canOperate)continue;
                if(Input.IsActionJustPressed("soul_projection")){
                    if(!isSoulForm){
                        Project();
                    }else{
                        IPossessable ps = null;
                        foreach(var n in possessArea.GetOverlappingBodies()){
                            if(n is IPossessable){
                                ps = n as IPossessable;
                                break;
                            }
                        }
                        if(ps != null){
                            level.CallDeferred(Node.MethodName.RemoveChild, this);
                            Transform = Transform2D.Identity;
                            cb = ps as CharacterBody2D;
                            Visible = false;
                            cb.CallDeferred(Node.MethodName.AddChild, this);
                            ps.Possess(this);
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
                        vel.X *= 1.0f - hAirDrag;
                        float abVelX = sgn * vel.X;
                        float dv = 0.0f;
                        if(sgn * hAxis < 0.0f || abVelX < maxHSpeed)dv += sgn * hAxis * hAccel;
                        if(cb.IsOnFloor())dv += (sgn * hAxis - 1.0f) * hGroundDrag;
                        else dv *= airHMultiplier;
                        abVelX += dv * dt;
                        if(abVelX < 0.0f)abVelX = 0.0f;
                        vel.X = abVelX * sgn;
                    }
                    ++frameCounter;
                    if(cb.IsOnFloor())wolfTill = frameCounter + wolfFrames;
                    if(Input.IsActionJustPressed("ui_accept"))preTill = frameCounter + preFrames;
                    if(frameCounter < wolfTill && frameCounter < preTill){
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
                    cb.Velocity = vel;
                    cb.MoveAndSlide();
                }
            }
        }

    }
}
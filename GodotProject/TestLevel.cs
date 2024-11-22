using Godot;
using System;
using Fcc;

public partial class TestLevel : Node2D
{
	[Export]
	public CharacterBody2D playerBody;
	[Export]
	public CharacterBody2D playerSoul;
	EventSrc<float> phu = new EventSrc<float>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		PlayerSpirit.Possess(playerBody, playerSoul, phu);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		phu.Emit((float)delta);
		
	}
}

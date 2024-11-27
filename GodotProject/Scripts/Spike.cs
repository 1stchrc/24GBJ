using Fcc;
using Godot;
using System;

public partial class Spike : Area2D{
	public override void _Ready(){
		BodyEntered += b => {
			if(b is IPossessable){
				CharacterBody2D pb = b as CharacterBody2D;
				if(pb.Velocity.Dot(Vector2.Up.Rotated(Rotation)) <= 0)(b as IPossessable).Kill();
			}
		};
	}
}

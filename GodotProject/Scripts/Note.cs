using Godot;
using System;

public partial class Note : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Visible=false;
	}
	void _on_visible_on_screen_notifier_2d_screen_entered()
	{
		Visible=true;
	}
	void _on_visible_on_screen_notifier_2d_2_screen_exited()
	{
		Visible=false;
	}
	
}

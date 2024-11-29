using Godot;
using System;
using System.Collections.Generic;
public partial class PressurePlate : Node2D, ILevelObject{
	[Export]
	public string enterEvent;
	[Export]
	public string exitEvent;
	HashSet<Node2D> bs;
	public void FeedLevelInstance(GeneralLevel level){
		Area2D enterArea = GetChild<Area2D>(0);
		Area2D exitArea = GetChild<Area2D>(1);
		enterArea.GlobalScale = new Vector2(enterArea.GlobalScale.X, 1.0f);
		exitArea.GlobalScale = new Vector2(exitArea.GlobalScale.X, 1.0f);
		ColorRect renderer = GetChild<ColorRect>(2);
		bs = new HashSet<Node2D>(enterArea.GetOverlappingBodies());
		if(bs.Count != 0)level.EmitEvent(enterEvent);
		enterArea.BodyEntered += b => {
			if(bs.Count == 0){
				renderer.Color = new Color("#acda73", 1.0f);
				level.EmitEvent(enterEvent);
			}
			bs.Add(b);
		};
		exitArea.BodyExited += b => {
			bs.Remove(b);
			if(bs.Count == 0){
				renderer.Color = new Color("#a6004b", 1.0f);
				level.EmitEvent(exitEvent);
			}
		};
	}

}

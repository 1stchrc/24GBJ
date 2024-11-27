using Godot;
using System;

public partial class PressurePlate : Node2D, ILevelObject{
	[Export]
	public string enterEvent;
	[Export]
	public string exitEvent;
	int bCount;
	public void FeedLevelInstance(TestLevel level){
		Area2D detectArea = GetChild<Area2D>(0);
		detectArea.GlobalScale = new Vector2(detectArea.GlobalScale.X, 1.0f);
		ColorRect renderer = GetChild<ColorRect>(2);
		bCount = detectArea.GetOverlappingBodies().Count;
		detectArea.BodyEntered += b => {
			if(bCount == 0){
				renderer.Color = new Color("#acda73", 1.0f);
				level.EmitEvent(enterEvent);
			}
			++bCount;
		};
		detectArea.BodyExited += b => {
			--bCount;
			if(bCount == 0){
				renderer.Color = new Color("#a6004b", 1.0f);
				level.EmitEvent(exitEvent);
			}
		};
	}

}

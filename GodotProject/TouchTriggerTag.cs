using Godot;
using System;

public partial class TouchTriggerTag : Area2D, ILevelObject{
	[Export]
	public string[] eventNames;
	public void FeedLevelInstance(GeneralLevel level){
		foreach(var s in eventNames)BodyEntered += b => level.EmitEvent(s);
	}
}

using Godot;
using System;

public partial class ToggleTile : StaticBody2D, ILevelObject{
	[Export]
	public ColorRect cr;
	[Export]
	public string eventName;
	[Export]
	public Color onColor;
	[Export]
	public Color offColor;
	[Export]
	public uint onCollisionLayer;
	[Export]
	public uint offCollisionLayer;
	[Export]
	public bool activated = true;
    public void FeedLevelInstance(TestLevel level){
		if(activated){
			CollisionLayer = onCollisionLayer;
			cr.Color = onColor;
		} else {
			CollisionLayer = offCollisionLayer;
			cr.Color = offColor;
		}
		level.ConnectEvent(eventName, () => {
			activated = !activated;
			if(activated){
				CollisionLayer = onCollisionLayer;
				cr.Color = onColor;
			} else {
				CollisionLayer = offCollisionLayer;
				cr.Color = offColor;
			}
		});
    }

}

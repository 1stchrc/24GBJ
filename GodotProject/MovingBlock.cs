using Fcc;
using Godot;
using System;

public partial class MovingBlock : AnimatableBody2D, ILevelObject{
	[Export]
	float transitionTime;

	[Export]
	Node2D from;
	[Export]
	Node2D to;

	[Export]
	string[] toEvents;
	[Export]
	string[] backEvents;
	
	float lerp = 0.0f;
	bool isTo = false;
	ulong animIteration = 0;

    public void FeedLevelInstance(TestLevel level){
		Transform = new Transform2D(Transform.X, Transform.Y, (isTo ? to : from).Transform.Origin);
        foreach(var e in toEvents){
			level.ConnectEvent(e, () => {
				if(isTo)return;
				GD.Print("TO");
				isTo = true;
				new Action<ulong>(async ait => {
					for(;;){
						lerp += (await level.physicsUpdate.Wait()) / transitionTime;
						if(animIteration != ait)return;
						if(lerp >= 1.0f){
							lerp = 1.0f;
							Transform = new Transform2D(Transform.X, Transform.Y, to.Transform.Origin);
							return;
						}
						Vector2 O = (1.0f - lerp) * from.Transform.Origin + lerp * to.Transform.Origin;
						Transform = new Transform2D(Transform.X, Transform.Y, O);
					}
				})(++animIteration);
			});
		}
		foreach(var e in backEvents){
			level.ConnectEvent(e, () => {
				if(!isTo)return;
				GD.Print("BACK");
				isTo = false;
				new Action<ulong>(async ait => {
					for(;;){
						lerp -= (await level.physicsUpdate.Wait()) / transitionTime;
						if(animIteration != ait)return;
						if(lerp <= 0.0f){
							lerp = 0.0f;
							Transform = new Transform2D(Transform.X, Transform.Y, from.Transform.Origin);
							return;
						}
						Vector2 O = (1.0f - lerp) * from.Transform.Origin + lerp * to.Transform.Origin;
						Transform = new Transform2D(Transform.X, Transform.Y, O);
					}
				})(++animIteration);
			});
		}
    }
}

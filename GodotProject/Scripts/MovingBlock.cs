using Fcc;
using Godot;
using System;
using System.Collections.Generic;
public partial class MovingBlock : AnimatableBody2D, ILevelObject{
	[Export]
	float transitionTime;

	[Export]
	Node2D to;

	[Export]
	string[] toEvents;
	[Export]
	string[] backEvents;
	
	float lerp = 0.0f;
	bool isTo = false;
	ulong animIteration = 0;

	public async void FeedLevelInstance(GeneralLevel level){
		Vector2 fromOrigin = Transform.Origin;
		Vector2 toOrigin = to.GlobalPosition - (GetParent() as Node2D).GlobalPosition;
		HashSet<string> toSet = new HashSet<string>(toEvents);
		HashSet<string> backSet = new HashSet<string>(backEvents);
		for(;;){
			string s = await level.eventEvent.Wait();
			if(isTo){
				if(!backSet.Contains(s))continue;
				isTo = false;
				new Action<ulong>(async ait => {
					for(;;){
						lerp -= (await level.physicsUpdate.Wait()) / transitionTime;
						if(animIteration != ait)return;
						if(lerp <= 0.0f){
							lerp = 0.0f;
							Transform = new Transform2D(Transform.X, Transform.Y, fromOrigin);
							return;
						}
						Vector2 O = (1.0f - lerp) * fromOrigin + lerp * toOrigin;
						Transform = new Transform2D(Transform.X, Transform.Y, O);
					}
				})(++animIteration);
			}else{
				if(!toSet.Contains(s))continue;
				isTo = true;
				new Action<ulong>(async ait => {
					for(;;){
						lerp += (await level.physicsUpdate.Wait()) / transitionTime;
						if(animIteration != ait)return;
						if(lerp >= 1.0f){
							lerp = 1.0f;
							Transform = new Transform2D(Transform.X, Transform.Y, toOrigin);
							return;
						}
						Vector2 O = (1.0f - lerp) * fromOrigin + lerp * toOrigin;
						Transform = new Transform2D(Transform.X, Transform.Y, O);
					}
				})(++animIteration);
			}
		}
	}
}

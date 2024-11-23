using Godot;
using System;
using Fcc;
using System.Collections;
using System.Collections.Generic;

public interface ILevelObject{
	public void FeedLevelInstance(TestLevel level);
}

public partial class TestLevel : Node2D{
	[Export]
	public CharacterBody2D playerBody;
	[Export]
	public CharacterBody2D playerSoul;
	[Export]
	public TileMapLayer mapSource;
	[Export]
	public Node2D gridBaseNode;
	EventSrc<float> phu = new EventSrc<float>();
	Dictionary<string, Action> events = new Dictionary<string, Action>();
	public void EmitEvent(string eventName){
		Action a;
		if(!events.TryGetValue(eventName, out a))return;
		a();
	}
	public void ConnectEvent(string eventName, Action action){
		if(events.TryAdd(eventName, action))return;
		events[eventName] += action;
	}

	public override void _Ready(){
		mapSource.Enabled = false;
		foreach(var pos in mapSource.GetUsedCells()){
			string path = mapSource.GetCellTileData(pos).GetCustomDataByLayerId(0).As<string>();
			var n = GD.Load<PackedScene>($"res://{path}").Instantiate();
			if(n is ILevelObject){
				(n as ILevelObject).FeedLevelInstance(this);
			}
			if(n is Node2D)(n as Node2D).Translate(new Vector2(pos.X * 32.0f, pos.Y * 32.0f));
			gridBaseNode.CallDeferred(Node.MethodName.AddChild, n);	
		}
		foreach(var ch in GetChildren()){
			if(ch is ILevelObject)(ch as ILevelObject).FeedLevelInstance(this);
		}
		PlayerSpirit.Possess(playerBody, playerSoul, phu);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		phu.Emit((float)delta);
		
	}
}

using Godot;
using System;
using Fcc;
using System.Collections;
using System.Collections.Generic;

public interface ILevelObject{
	public void FeedLevelInstance(TestLevel level);
}

public partial class TestLevel : Node2D{
	public EventSrc<float> physicsUpdate = new EventSrc<float>();
	public EventSrc<string> eventEvent = new EventSrc<string>();
	Dictionary<string, Action> events = new Dictionary<string, Action>();
	public void EmitEvent(string eventName){
		eventEvent.Emit(eventName);
		Action a;
		if(!events.TryGetValue(eventName, out a))return;
		a();
	}
	public void ConnectEvent(string eventName, Action action){
		if(events.TryAdd(eventName, action))return;
		events[eventName] += action;
	}
	void dfsChildren(Node node){
		foreach(var ch in node.GetChildren()){
			if(ch is TileMapLayer){
				TileMapLayer mapSource = ch as TileMapLayer;
				mapSource.Enabled = false;
				foreach(var pos in mapSource.GetUsedCells()){
					string path = mapSource.GetCellTileData(pos).GetCustomDataByLayerId(0).As<string>();
					var n = GD.Load<PackedScene>($"res://{path}").Instantiate();
					if(n is ILevelObject){
						(n as ILevelObject).FeedLevelInstance(this);
					}
					if(n is Node2D)(n as Node2D).Translate(new Vector2(pos.X * 32.0f, pos.Y * 32.0f));
					ch.CallDeferred(Node.MethodName.AddChild, n);	
				}
			}
			if(ch is ILevelObject)(ch as ILevelObject).FeedLevelInstance(this);
			dfsChildren(ch);
		}
	}
	public override void _Ready(){
		dfsChildren(this);
	}
	public override void _PhysicsProcess(double delta){
		physicsUpdate.Emit((float)delta);
	}
}

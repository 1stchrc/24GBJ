using Godot;
using System;
using Fcc;
using System.Collections;
using System.Collections.Generic;

public interface ILevelObject{
	public void FeedLevelInstance(GeneralLevel level);
}

public partial class GeneralLevel : Node2D{
	public EventSrc<float> physicsUpdate = new EventSrc<float>();
	public EventSrc<string> eventEvent = new EventSrc<string>();
	Dictionary<string, Action> events = new Dictionary<string, Action>();
	public InLevelUI UI;
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
			if(ch is ILevelObject)(ch as ILevelObject).FeedLevelInstance(this);
			dfsChildren(ch);
		}
	}
	public override void _Ready(){
		UI = GD.Load<PackedScene>("res://Scenes/UI/InLevelUI.tscn").Instantiate<InLevelUI>();
		CallDeferred(MethodName.AddChild, UI);
		dfsChildren(this);
	}
	public override void _PhysicsProcess(double delta){
		physicsUpdate.Emit((float)delta);
	}
}

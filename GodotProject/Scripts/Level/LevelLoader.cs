using Godot;
using System;

public partial class LevelLoader : Node{
	GeneralLevel levelLoaded;
	PackedScene curLevelScene;
	[Export]
	string[] levelList;
	[Export]
	int curLevelIdx;
	void LoadCurLevel(){
		levelLoaded = curLevelScene.Instantiate<GeneralLevel>();
		levelLoaded.loader = this;
		CallDeferred(MethodName.AddChild, levelLoaded);
	}
	public override void _Ready(){
		curLevelScene = GD.Load<PackedScene>(levelList[curLevelIdx]);
		LoadCurLevel();
	}
	public void ResetLevel(){
		levelLoaded.CallDeferred(MethodName.Free);
		LoadCurLevel();
	}
}

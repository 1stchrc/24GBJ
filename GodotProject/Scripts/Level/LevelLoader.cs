using Fcc;
using Godot;
using System;
using System.Threading.Tasks;

public partial class LevelLoader : Node{
	GeneralLevel levelLoaded;
	PackedScene curLevelScene;
	[Export]
	string[] levelList;
	[Export]
	int curLevelIdx;
	Node WhereLevelInitializedAt;
	[Export]
	Node2D TransMaskHole;
	[Export]
	AudioStream BGMIntro;
	[Export]
	AudioStream BGMLoop;
	EventSrc<float> update = new EventSrc<float>();
	public override void _PhysicsProcess(double delta){
		levelLoaded.physicsUpdate.Emit((float)delta);
	}
	public override void _Process(double delta){
		update.Emit((float)delta);
	}
	public async Task PlayTransIn(Vector2 center){
		float t = 0.0f;
		TransMaskHole.Position = center;
		// {
		// 	var cam = GetViewport().GetCamera2D();
		// 	if(cam != null)
		// 		TransMaskHole.Position += GetViewport().GetWindow().Size / 2 - cam.GlobalPosition;
		// }
		
		do{
			TransMaskHole.Scale = Vector2.One * t * t * 50.0f;
			TransMaskHole.Rotation = t * Mathf.Pi;
			t += await update.Wait();
		}while(t < 1.0f);
		TransMaskHole.Scale = Vector2.One * t * 50.0f;
	}
	public async Task PlayTransOut(Vector2 center){
		float t = 0.0f;
		TransMaskHole.Position = GetViewport().CanvasTransform * center;
		do{
			TransMaskHole.Scale = Vector2.One * (1.0f - t) * (1.0f - t) * 50.0f;
			TransMaskHole.Rotation = t * Mathf.Pi;
			t += await update.Wait();
		}while(t < 1.0f);
		TransMaskHole.Scale = Vector2.Zero;
	}
	void LoadCurLevel(){
		levelLoaded = curLevelScene.Instantiate<GeneralLevel>();
		levelLoaded.loader = this;
		WhereLevelInitializedAt.CallDeferred(MethodName.AddChild, levelLoaded);
	}
	AudioStreamPlayer asp;
	public override void _Ready(){
		asp = new AudioStreamPlayer();
		AddChild(asp);
		asp.Stream = BGMIntro;
		asp.Play();
		asp.Finished += () => {
			asp.Stream = BGMLoop;
			asp.Play();
		};
		(TransMaskHole.GetViewport() as SubViewport).Size = GetViewport().GetWindow().Size;
		curLevelScene = GD.Load<PackedScene>(levelList[curLevelIdx]);
		WhereLevelInitializedAt = GetChild(0);
		LoadCurLevel();
	}
	public void Reset(){
		levelLoaded.CallDeferred(MethodName.Free);
		LoadCurLevel();
	}
	public void MoveToNext(){
		levelLoaded.CallDeferred(MethodName.Free);
		curLevelScene = GD.Load<PackedScene>(levelList[++curLevelIdx]);
		LoadCurLevel();
	}
}

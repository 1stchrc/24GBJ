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
	AudioStream BGMIntro1;
	[Export]
	AudioStream BGMLoop1;
	[Export]
	AudioStream BGMIntro2;
	[Export]
	AudioStream BGMLoop2;
	EventSrc<float> update = new EventSrc<float>();
	public override void _PhysicsProcess(double delta){
		levelLoaded.physicsUpdate.Emit((float)delta);
	}
	public override void _Process(double delta){
		update.Emit((float)delta);
		
	}
	public async Task PlayTransIn(Vector2 center){
		float t = 0.0f;
		TransMaskHole.Position = GetViewport().CanvasTransform * center;
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
	private Timer _timer;
	void LoadCurLevel(){
<<<<<<< HEAD
		//GetChild<AnimatedSprite2D>(1).Play("cutscene")；
=======
>>>>>>> 781c7ce9f24de3eb8686259bf902f1e79604f9f8
		levelLoaded = curLevelScene.Instantiate<GeneralLevel>();
		levelLoaded.loader = this;
		WhereLevelInitializedAt.CallDeferred(MethodName.AddChild, levelLoaded);
		
		
	}
	
	public AudioStreamPlayer BGM1asp;
	public AudioStreamPlayer BGM2asp;
	[Export]
	public float volume;
	public override void _Ready(){
		BGM1asp = new AudioStreamPlayer();
		BGM2asp = new AudioStreamPlayer();
		BGM1asp.VolumeDb = volume;
		BGM2asp.VolumeDb = Mathf.LinearToDb(0);
		AddChild(BGM1asp);
		AddChild(BGM2asp);
		BGM1asp.Stream = BGMIntro1;
		BGM2asp.Stream = BGMIntro2;
		BGM1asp.Play();
		BGM2asp.Play();
		BGM1asp.Finished += () => {
			BGM1asp.Stream = BGMLoop1;
			BGM1asp.Play();
		};
		BGM2asp.Finished += () => {
			BGM2asp.Stream = BGMLoop2;
			BGM2asp.Play();
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

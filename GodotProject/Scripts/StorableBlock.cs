using Fcc;
using Godot;
using System;

public partial class StorableBlock : StaticBody2D, ITransferrable{
	public uint RequiredLayer => 1 | 2 | 4 | 8;
	public Shape2D RequiredSpace => GetChild<CollisionShape2D>(0).Shape;
    public Vector2 RenderSize => new Vector2(349, 88);
	public void Store(){

	}

	public void Unstore(){

	}

}

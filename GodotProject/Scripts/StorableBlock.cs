using Fcc;
using Godot;
using System;

public partial class StorableBlock : StaticBody2D, ITransferrable
{
	public uint RequiredLayer => 1 | 2 | 4 | 8;

	public Shape2D GetRequiredSpace(){
		var ret = new RectangleShape2D();
		ret.Size = Vector2.One * 32.0f;
		return ret;
	}

	public void Store(){

	}

	public void Unstore(){

	}

}

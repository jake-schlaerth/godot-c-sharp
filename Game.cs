using Godot;
using System;



public partial class Game : Node2D
{

	public override void _Ready()
	{
		GD.Print("hello cruel world");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// _sprite.Position = new Vector2(100, 0) * (float)delta;
	}
}

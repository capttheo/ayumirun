using Godot;
using System;

public class LevelChanger : Node2D
{
	GameState game;

	public override void _Ready()
	{
		game = GetNode("/root/GameState") as GameState;
		FindNode("Area2D").Connect("body_entered", this, "OnBodyEntered");
	}

	public void OnBodyEntered(Node body)
	{
		if(body.Name.Contains("Player"))
		{
			game.currentLevel = Name;
			GetTree().ChangeScene("res://scenes/" + Name + ".tscn");
		}
	}

}

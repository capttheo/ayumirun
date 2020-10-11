using Godot;
using System;

public class Game : Node2D
{
	PackedScene playerTscn;
	GameState game;
		
	//Node2D playerRoot;

	public override void _Ready()
	{
		game = GetNode("/root/GameState") as GameState;
		game.Save();
		GD.Print("Save OnGameReady");
		playerTscn = ResourceLoader.Load("res://scenes/Player.tscn") as PackedScene;

		if(Name == "LevelIntro")
		{
			GD.Print("cutscene");
			(FindNode("AnimationPlayer") as AnimationPlayer).Play("cutscene");
		}
		else
		{
			if(Name != "LevelMain2")
			{
				SpawnPlayer();
			}
		}

		var tutor = FindNode("Tutorial");
		if(tutor != null)
		{
			var chapter = tutor.FindNode(Name) as Node2D;
			if (chapter != null)
			{
				chapter.Show();
			}
		}

	}

	void SpawnPlayer(bool delet = true)
	{
		if(delet)
		{
			foreach(Node child in GetChildren())
			{
				if(child.Name.Contains("Player"))
				{
					RemoveChild(child);
					child.QueueFree();
				}
			}
		}
		var player = playerTscn.Instance() as KinematicBody2D;
		var spawn = FindNode("Spawn");
		player.Position = (spawn as Node2D).Position;
		player.Name = "Player";
		AddChild(player);
	}

	public override void _Input(InputEvent @event)
	{

		base._Input(@event);
		if(Input.IsActionJustReleased("f11"))
		{
			game.ToggleFullScreen();
		}
		if(Input.IsActionJustReleased("reset"))
		{
			SpawnPlayer();	
		}
		if(Input.IsActionJustReleased("clone"))
		{
			SpawnPlayer(false);
		}



	}

};

using Godot;
using System;

public class TitleScreen : Node2D
{
	GameState game;

	public override void _Ready()
	{
		game = GetNode("/root/GameState") as GameState;
		FindNode("btnContinue").Connect("pressed", this, "OnBtnContinue");
		FindNode("btnNewGame").Connect("pressed", this, "OnBtnNewGame");
		FindNode("btnQuit").Connect("pressed", this, "OnBtnQuit");
		FindNode("btnFullscreen").Connect("pressed", this, "OnBtnFullscreen");
	}

	public void OnBtnContinue()
	{
		game.Load();
		GD.Print("Load  OnContinue");
		SwitchSceneGame();
	}

	public void OnBtnNewGame()
	{
		game.Reset();
		SwitchSceneGame();
	}

	void SwitchSceneGame()
	{
		GetTree().ChangeScene("res://scenes/" + game.currentLevel + ".tscn");
	}

	public void OnBtnQuit()
	{
		GetTree().Quit();
	}

	public void OnBtnFullscreen()
	{
		game.ToggleFullScreen();
	}

	public override void _Input(InputEvent @event)
	{
		if(Input.IsActionJustReleased("f11"))
		{
			game.ToggleFullScreen();
		}
	}
}


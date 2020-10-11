using Godot;
using System;

public class GameState : Node2D
{
	byte[] secret = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 100, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31  };
	
	public string currentLevel;
	public int points;

	const string START_LEVEL = "LevelIntro";

	public void Reset()
	{
		points = 0;
		currentLevel = START_LEVEL;
	}

	const string SAVE_FILE_NAME = "user://savegame.bin";
	const string SECTION_GAME = "game";
	const string GAME_LEVEL = "level";
	const string GAME_POINTS = "points";

	public void Load()
	{
		// var config = new ConfigFile();
		// var err = config.LoadEncrypted(SAVE_FILE_NAME, secret);
		// if(err == Error.Ok)
		// {
		// 	currentLevel = (string)config.GetValue(SECTION_GAME, GAME_LEVEL, START_LEVEL);
		// 	points = (int)config.GetValue(SECTION_GAME, GAME_POINTS, 0);
		// 	GD.Print("loaded " + currentLevel);
		// 	return;
		// }
		// else
		// {
		// 	Reset();
		// }
	}

	public void Save()
	{
		// GD.Print("saving " + currentLevel);
		// var config = new ConfigFile();
		// config.SetValue(SECTION_GAME, GAME_LEVEL, currentLevel);
		// config.SetValue(SECTION_GAME, GAME_POINTS, points);
		// config.SaveEncrypted(SAVE_FILE_NAME, secret);
	}

	public override void _Ready()
	{
		Reset();
		
	}

	Vector2 winSize;
	Vector2 winPos;

	public void ToggleFullScreen()
	{
		if(OS.GetName() == "HTML5")
		{
			OS.WindowFullscreen = !OS.WindowFullscreen;
		}
		else 
		{
			if(OS.WindowBorderless)
			{
				OS.WindowBorderless = false;
				OS.WindowSize = winSize;
				OS.WindowPosition = winPos;
			}
			else
			{
				winSize = OS.WindowSize;
				winPos = OS.WindowPosition;

				OS.WindowPosition = Vector2.Zero;
				OS.WindowSize = OS.GetScreenSize(-1);
				OS.WindowBorderless = true;
			}
		}
	}
}

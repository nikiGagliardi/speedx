using Godot;
using System;

public partial class MainMenu : Control
{
	private Label _bestScoreLabel;
	private Button _startButton;
	private Button _settingsButton;

	public override void _Ready()
	{

		_bestScoreLabel = GetNode<Label>("VBoxContainer/BestScoreMainMenu");
		_startButton = GetNode<Button>("VBoxContainer/Start");
		_settingsButton = GetNode<Button>("VBoxContainer/Settings");

		_startButton.Pressed += OnStartPressed;
		_settingsButton.Pressed += OnSettingsPressed;

		_bestScoreLabel.Text = $"Best: {LoadBestScore()}";

	}

	private void OnStartPressed()
	{

		var error = GetTree().ChangeSceneToFile("res://scenes/Main.tscn");

	}

private void OnSettingsPressed()
{
	GetTree().ChangeSceneToFile("res://scenes/Settings.tscn");
}

	private int LoadBestScore()
	{
		const string savePath = "user://best_score.dat";
		if (!FileAccess.FileExists(savePath)) return 0;

		using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
		return file != null ? (int)file.Get32() : 0;
	}
}

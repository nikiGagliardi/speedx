using Godot;
using System;

public partial class UI : CanvasLayer
{
	private Label _scoreLabel;
	private Label _speedLabel;
	private Label _bestScoreLabel;
	private Control _gameOverScreen;
	private Label _gameOverBestLabel;
	private Button _restartButton;
	private Button _pauseButton;
	private Control _pauseScreen;
	private Button _resumeButton;
	private Button _restartFromPauseButton;
	private Button _menuButton;

	private ColorRect[] _lifeIndicators;
	private bool _isPaused = false;

	public override void _Ready()
	{
		_scoreLabel       = GetNode<Label>("ScoreLabel");
		_speedLabel       = GetNode<Label>("SpeedLabel");
		_bestScoreLabel   = GetNode<Label>("BestScoreLabel");
		_gameOverScreen   = GetNode<Control>("GameOverScreen");
		_gameOverBestLabel = GetNode<Label>("GameOverScreen/GameOverBestLabel");
		_restartButton    = GetNode<Button>("GameOverScreen/Button");
		_pauseButton      = GetNode<Button>("PauseButton");
		_pauseScreen      = GetNode<Control>("PauseScreen");
		_resumeButton     = GetNode<Button>("PauseScreen/VBoxContainer/Resume");
		_restartFromPauseButton = GetNode<Button>("PauseScreen/VBoxContainer/Restart");
		_menuButton       = GetNode<Button>("PauseScreen/VBoxContainer/Menu");

		_restartButton.Pressed          += OnRestartPressed;
		_pauseButton.Pressed            += OnPausePressed;
		_resumeButton.Pressed           += OnResumePressed;
		_restartFromPauseButton.Pressed += OnRestartPressed;
		_menuButton.Pressed             += OnMenuPressed;

GD.Print("UI Ready end");
		_gameOverScreen.Visible = false;
		_pauseScreen.Visible    = false;
		
		_lifeIndicators = new ColorRect[]
{
	GetNode<ColorRect>("LivesContainer/Life1"),
	GetNode<ColorRect>("LivesContainer/Life2"),
	GetNode<ColorRect>("LivesContainer/Life3"),
};

UpdateLives(0);
	}

	public override void _Process(double delta)
	{
		if (GameManager.Instance == null) return;
		_scoreLabel.Text     = $"Score: {(int)GameManager.Instance.Score}";
		_speedLabel.Text     = $"Speed: {(int)GameManager.Instance.CurrentSpeed}";
		_bestScoreLabel.Text = $"Best: {GameManager.Instance.BestScore}";
	}
	
	public void UpdateLives(int lives)
{
	for (int i = 0; i < _lifeIndicators.Length; i++)
	{
		_lifeIndicators[i].Color = i < lives
			? new Color(0.0f, 1.0f, 0.4f)
			: new Color(0.2f, 0.2f, 0.2f);
	}
}

	public void ShowGameOver()
	{
		int currentScore = (int)GameManager.Instance.Score;
		int bestScore    = GameManager.Instance.BestScore;

		_gameOverBestLabel.Text = currentScore == bestScore
			? $"Nuovo record: {bestScore}"
			: $"Best: {bestScore}";

		_gameOverScreen.Visible = true;
	}

	private void OnPausePressed()
	{
		_isPaused = true;
		_pauseScreen.Visible        = true;
		_pauseButton.Visible        = false;
		GetTree().Paused            = true;
	}

	private void OnResumePressed()
	{
		_isPaused = false;
		_pauseScreen.Visible        = false;
		_pauseButton.Visible        = true;
		GetTree().Paused            = false;
	}

	private void OnRestartPressed()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnMenuPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}
}

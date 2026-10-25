using Godot;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Export] public float StartSpeed = 10f;
	[Export] public float MaxSpeed = 50f;
	[Export] public float SpeedIncreaseRate = 1f;

	public float CurrentSpeed { get; private set; }
	public bool IsPlaying { get; private set; }
	public float Score { get; private set; }
	public int BestScore { get; private set; }
	
	public static int LaneCount { get; set; } = 5;
	
	public int Lives { get; private set; } = 0;
	public const int MaxLives = 3;

	private float _scoreForNextLifeCheck = 500f;
	private bool _lifeCheckDone = false;

	private const string SavePath = "user://best_score.dat";

public override void _Ready()
{
	Instance = this;
	SettingsManager.Load();
	LoadBestScore();
	LaneCount = SettingsManager.Lanes;
	StartGame();

	var music = GetNode<AudioStreamPlayer>("/root/MusicPlayer") as MusicPlayer;
	music?.SetVolume(SettingsManager.Volume);
}

	public override void _Process(double delta)
	{
		if (!IsPlaying) return;

		CurrentSpeed = Mathf.Min(
			CurrentSpeed + SpeedIncreaseRate * (float)delta,
			MaxSpeed
		);

		Score += CurrentSpeed * (float)delta;
	}

	public void StartGame()
	{
	CurrentSpeed = StartSpeed;
	Score = 0;
	Lives = 0;
	IsPlaying = true;
	_lifeCheckDone = false;
	_scoreForNextLifeCheck = (float)GD.RandRange(500, 600);
	}


public bool TryLoseLife()
{
	if (Lives > 0)
	{
		Lives--;
		var ui = GetTree().GetFirstNodeInGroup("ui") as UI;
		ui?.UpdateLives(Lives);
		return true;
	}
	return false; // 0 vite → game over
}

public void AddLife()
{
	if (Lives < MaxLives)
	{
		Lives++;
		var ui = GetTree().GetFirstNodeInGroup("ui") as UI;
		ui?.UpdateLives(Lives);
	}
}

public bool ShouldSpawnLife()
{
	
	if (Score >= _scoreForNextLifeCheck && !_lifeCheckDone)
	{
		GD.Print("Check vita!");
		_lifeCheckDone = true;
		
		// Per test: sempre true
		_scoreForNextLifeCheck += (float)GD.RandRange(50, 100);
		_lifeCheckDone = false;
		return true;
	}
	return false;
}

public void TriggerGameOver()
{
	if (!IsPlaying) return;
	IsPlaying = false;

	if ((int)Score > BestScore)
	{
		BestScore = (int)Score;
		SaveBestScore();
	}


	var ui = GetTree().GetFirstNodeInGroup("ui") as UI;
	ui?.ShowGameOver();
}

	private void SaveBestScore()
	{
		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		if (file != null)
			file.Store32((uint)BestScore);
	}

	private void LoadBestScore()
	{
		if (!FileAccess.FileExists(SavePath)) return;

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		if (file != null)
			BestScore = (int)file.Get32();
	}
}

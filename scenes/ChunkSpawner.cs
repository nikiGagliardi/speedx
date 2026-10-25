using Godot;
using System.Collections.Generic;

public partial class ChunkSpawner : Node3D
{
	[Export] public PackedScene ChunkScene;
	[Export] public PackedScene ObstacleScene;
	[Export] public PackedScene LifePowerUpScene;
	[Export] public int ChunksAhead = 10;
	[Export] public float ChunkLength = 40f;

	private const float LaneWidth = 2.5f;
	private readonly Queue<Node3D> _activeChunks = new();
	private float _nextSpawnZ = 0f;
	private Node3D _player;
	private HashSet<int> _lastChunkLanes = new();
	private bool _initialized = false;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Node3D;
		// Non spawna più qui — aspetta il _Process
	}

	public override void _Process(double delta)
	{
		// Aspetta che GameManager sia pronto prima di spawnare
		if (!_initialized)
		{
			if (GameManager.Instance == null) return;

			for (int i = 0; i < ChunksAhead; i++)
				SpawnChunk();

			_initialized = true;
			return;
		}

		if (_player == null)
			_player = GetTree().GetFirstNodeInGroup("player") as Node3D;

		if (_player == null) return;

		while (_nextSpawnZ > _player.Position.Z - ChunksAhead * ChunkLength)
			SpawnChunk();

		while (_activeChunks.Count > ChunksAhead + 2)
		{
			var old = _activeChunks.Dequeue();
			old.QueueFree();
		}
	}

	private void SpawnChunk()
	{
		var chunk = ChunkScene.Instantiate<Node3D>();
		chunk.Position = new Vector3(0, -0.5f, _nextSpawnZ);
		AddChild(chunk);
		_activeChunks.Enqueue(chunk);
		SpawnObstaclesInChunk(chunk);
		SpawnLifePowerUp(chunk);
		_nextSpawnZ -= ChunkLength;
	}
	
private void SpawnLifePowerUp(Node3D chunk)
{
	if (LifePowerUpScene == null) return;
	if (GameManager.Instance == null) return;
	if (!GameManager.Instance.ShouldSpawnLife()) return;
	if (GameManager.Instance.Lives >= GameManager.MaxLives) return;

	int halfLanes = GameManager.LaneCount / 2;
	int lane = (int)GD.RandRange(-halfLanes, halfLanes);
	float x = lane * 2.5f;
	
	// Spawna nella prima metà del chunk — più vicino al player
	float z = -(float)GD.RandRange(2f, ChunkLength * 0.4f);

	var powerUp = LifePowerUpScene.Instantiate<Node3D>();
	powerUp.Position = new Vector3(x, 0.5f, z);
	chunk.AddChild(powerUp);
	
	GD.Print($"PowerUp spawnato a pos globale:{powerUp.GlobalPosition}");
}

	private void SpawnObstaclesInChunk(Node3D chunk)
	{
		if (ObstacleScene == null) return;
		if (_activeChunks.Count <= 3) return;

		float difficulty = 0f;
		if (GameManager.Instance != null)
			difficulty = (GameManager.Instance.CurrentSpeed - GameManager.Instance.StartSpeed)
						 / (GameManager.Instance.MaxSpeed - GameManager.Instance.StartSpeed);

		int maxObstacles = Mathf.Min(4, 1 + Mathf.FloorToInt(difficulty * 3));
		int minObstacles = 1;
		int count = (int)GD.RandRange(minObstacles, maxObstacles);

		var usedLanes = new HashSet<int>(_lastChunkLanes);
		var currentChunkLanes = new HashSet<int>();

		for (int i = 0; i < count; i++)
		{
			int lane = GetFreeLane(usedLanes);
			if (lane == -99) break;

			float z = -(float)GD.RandRange(ChunkLength * 0.5f, ChunkLength - 4f);
			float x = LaneToX(lane);

			var obstacle = ObstacleScene.Instantiate<Node3D>();
			obstacle.Position = new Vector3(x, 0.5f, z);
			chunk.AddChild(obstacle);
			usedLanes.Add(lane);
			currentChunkLanes.Add(lane);
		}

		_lastChunkLanes = currentChunkLanes;
	}

	private int GetFreeLane(HashSet<int> used)
	{
		int halfLanes = GameManager.LaneCount / 2;
		var available = new Godot.Collections.Array<int>();

		for (int i = -halfLanes; i <= halfLanes; i++)
			if (!used.Contains(i))
				available.Add(i);

		if (available.Count == 0) return -99;
		return available[(int)GD.RandRange(0, available.Count - 1)];
	}

	private float LaneToX(int lane) => lane * LaneWidth;
}

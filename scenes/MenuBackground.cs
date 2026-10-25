using Godot;
using System;
using System.Collections.Generic;
public partial class MenuBackground : Node3D
{
	[Export] public PackedScene ChunkScene;
	[Export] public float Speed = 15f;
	[Export] public int ChunksAhead = 8;
	[Export] public float ChunkLength = 40f;

	private readonly Queue<Node3D> _chunks = new();
	private float _nextSpawnZ = 0f;
	private float _cameraZ = 0f;

	public override void _Ready()
	{
		for (int i = 0; i < ChunksAhead; i++)
			SpawnChunk();

			
	}

	public override void _Process(double delta)
	{
		// Muovi la camera in avanti
		_cameraZ -= Speed * (float)delta;
		var cam = GetNode<Camera3D>("Camera3D");
		cam.Position = new Vector3(0, 1.5f, _cameraZ + 2f);

		// Spawna nuovi chunk davanti
		while (_nextSpawnZ > _cameraZ - ChunksAhead * ChunkLength)
			SpawnChunk();

		// Rimuovi chunk troppo indietro
		while (_chunks.Count > ChunksAhead + 2)
		{
			var old = _chunks.Dequeue();
			old.QueueFree();
		}
		
		
	}

	private void SpawnChunk()
	{
		var chunk = ChunkScene.Instantiate<Node3D>();
		chunk.Position = new Vector3(0, -0.5f, _nextSpawnZ);
		AddChild(chunk);
		_chunks.Enqueue(chunk);
		_nextSpawnZ -= ChunkLength;
	}
}

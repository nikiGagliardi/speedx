using Godot;
using System.Collections.Generic;

public partial class StarField : Node3D
{
	[Export] public int StarCount = 2000;
	[Export] public float FieldRadius = 80f;
	[Export] public float FieldLength = 200f;
	[Export] public float StarSpeed = 1.2f;

	private Node3D _player;
	private MeshInstance3D _starMesh;
	private ImmediateMesh _immediateMesh;
	private List<Vector3> _starPositions = new();

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Node3D;
		GenerateStars();
	}

	private void GenerateStars()
	{
		_starMesh      = new MeshInstance3D();
		_immediateMesh = new ImmediateMesh();
		_starMesh.Mesh = _immediateMesh;

		var material = new StandardMaterial3D();
		material.ShadingMode            = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.AlbedoColor            = new Color(1, 1, 1, 1);
		material.VertexColorUseAsAlbedo = true;
		material.UsePointSize           = true;
		material.PointSize              = 6.0f;

		_starMesh.SetSurfaceOverrideMaterial(0, material);
		AddChild(_starMesh);

		InitStarPositions();
		RedrawStars(0f);
	}

	private void InitStarPositions()
	{
		_starPositions.Clear();

		var rng = new RandomNumberGenerator();
		rng.Randomize();

		for (int i = 0; i < StarCount; i++)
		{
			float angle  = rng.RandfRange(0, Mathf.Tau);
			float radius = rng.RandfRange(15f, FieldRadius);
			float z      = rng.RandfRange(0, -FieldLength);

			_starPositions.Add(new Vector3(
				Mathf.Cos(angle) * radius,
				Mathf.Sin(angle) * radius,
				z
			));
		}
	}

	private void RedrawStars(float speedRatio)
	{
		_immediateMesh.ClearSurfaces();
		_immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Points);

		foreach (var pos in _starPositions)
		{
			float dist       = Mathf.Sqrt(pos.X * pos.X + pos.Y * pos.Y);
			float brightness = Mathf.Lerp(1.0f, 0.4f, dist / FieldRadius);

			Color color = new Color(
				brightness * Mathf.Lerp(1f, 0.6f, speedRatio),
				brightness * Mathf.Lerp(1f, 0.8f, speedRatio),
				brightness
			);

			_immediateMesh.SurfaceSetColor(color);
			_immediateMesh.SurfaceAddVertex(pos);
		}

		_immediateMesh.SurfaceEnd();
	}

	public override void _Process(double delta)
	{
		if (_player == null)
		{
			_player = GetTree().GetFirstNodeInGroup("player") as Node3D;
			return;
		}

		Position = new Vector3(0, 0, _player.Position.Z);

		float speed      = GameManager.Instance?.CurrentSpeed ?? 10f;
		float maxSpeed   = GameManager.Instance?.MaxSpeed ?? 40f;
		float speedRatio = speed / maxSpeed;

		var material = _starMesh.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
		float dpi      = DisplayServer.ScreenGetDpi();
		float dpiScale = Mathf.Max(1.0f, dpi / 96.0f);
		if (material != null)
			material.PointSize = Mathf.Lerp(4f, 8f, speedRatio) * dpiScale;

		float currentStarSpeed = Mathf.Lerp(StarSpeed, StarSpeed * 3f, speedRatio);

		for (int i = 0; i < _starPositions.Count; i++)
		{
			var star = _starPositions[i];
			star.Z += speed * currentStarSpeed * (float)delta;

			if (star.Z > 10f)
				star.Z -= FieldLength;

			_starPositions[i] = star;
		}

		RedrawStars(speedRatio);
	}
}

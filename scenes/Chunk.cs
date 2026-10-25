using Godot;

public partial class Chunk : Node3D
{
	[Export] public float ChunkLength = 40f;
	[Export] public float LaneWidth = 2.5f;

	public override void _Ready()
	{
		GenerateLanes();
	}

	private void GenerateLanes()
	{
		int laneCount = GameManager.LaneCount;
		float totalWidth = laneCount * LaneWidth;
		float halfWidth = totalWidth / 2f;

		var floor = GetNode<MeshInstance3D>("MeshInstance3D");
		if (floor == null) return;

		var newBox = new BoxMesh();
		newBox.Size = new Vector3(totalWidth, 0.2f, ChunkLength);

		var originalMaterial = floor.GetActiveMaterial(0);
		if (originalMaterial != null)
			newBox.SurfaceSetMaterial(0, originalMaterial.Duplicate() as Material);

		floor.Mesh = newBox;

		for (int i = 0; i <= laneCount; i++)
		{
			float x = -halfWidth + i * LaneWidth;
			AddLaneLine(x);
		}

		AddTransversalLine(-ChunkLength / 3f, totalWidth);
		AddTransversalLine(-ChunkLength * 2f / 3f, totalWidth);
	}

	private void AddLaneLine(float x)
	{
		var line = new MeshInstance3D();
		var box = new BoxMesh();
		box.Size = new Vector3(0.05f, 0.01f, ChunkLength);
		line.Mesh = box;
		line.Position = new Vector3(x, 0.11f, -ChunkLength / 2f);

		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(0.2f, 0.2f, 0.2f);
		material.EmissionEnabled = true;
		material.Emission = new Color(0.2f, 0.2f, 0.2f);
		material.EmissionEnergyMultiplier = 0.5f;
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		line.SetSurfaceOverrideMaterial(0, material);
		AddChild(line);
	}

	private void AddTransversalLine(float z, float totalWidth)
	{
		var line = new MeshInstance3D();
		var box = new BoxMesh();
		box.Size = new Vector3(totalWidth, 0.02f, 0.15f);
		line.Mesh = box;
		line.Position = new Vector3(0, 0.11f, z);

		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(0.2f, 0.2f, 0.2f);
		material.EmissionEnabled = true;
		material.Emission = new Color(0.2f, 0.2f, 0.2f);
		material.EmissionEnergyMultiplier = 0.5f;
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		line.SetSurfaceOverrideMaterial(0, material);
		AddChild(line);
	}
}

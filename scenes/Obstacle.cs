using Godot;
using System;

public partial class Obstacle : Area3D
{
	private static readonly Color[] MetallicColors = new[]
	{
	new Color(0.6f, 0.0f, 0.9f), // viola
	new Color(0.0f, 0.8f, 0.9f), // ciano
	new Color(0.9f, 0.5f, 0.0f), // arancione
	};

	private Color _obstacleColor;

	public override void _Ready()
	{
	ApplyRandomColor();
	AddOutline();
	AddTrail();
	}
	
	
private void AddOutline()
{
	var outlineMesh = new MeshInstance3D();
	var box = new BoxMesh();
	box.Size = new Vector3(2.6f, 1.1f, 2.6f); // leggermente più grande dell'ostacolo (2.5, 1, 2.5)
	outlineMesh.Mesh = box;

	var material = new StandardMaterial3D();
	material.AlbedoColor = new Color(0, 0, 0, 1);
	material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
	material.CullMode = BaseMaterial3D.CullModeEnum.Front; // renderizza solo le facce posteriori
	material.GrowAmount = 0.02f;

	outlineMesh.SetSurfaceOverrideMaterial(0, material);
	AddChild(outlineMesh);
}

	private void ApplyRandomColor()
	{
		var mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		var material = mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
		if (material == null) return;

		_obstacleColor = MetallicColors[(int)GD.RandRange(0, MetallicColors.Length - 1)];

		material.AlbedoColor = _obstacleColor;
		material.EmissionEnabled = true;
		material.Emission = _obstacleColor * 0.4f;
		material.Metallic = 0.7f;
		material.Roughness = 0.3f;

		mesh.SetSurfaceOverrideMaterial(0, material);
	}

private void AddTrail()
{
	var trailMesh = new MeshInstance3D();
	var box = new BoxMesh();
	box.Size = new Vector3(2.5f, 0.04f, 72f); // 3x più lunga
	trailMesh.Mesh = box;
	trailMesh.Position = new Vector3(0, -0.35f, 37f); // circa metà di 72

	var shader = ResourceLoader.Load<Shader>("res://shaders/trail.gdshader");
	if (shader == null)
	{
		GD.Print("Shader non trovato!");
		return;
	}

	var material = new ShaderMaterial();
	material.Shader = shader;

	
material.SetShaderParameter("trail_color", new Color(
	_obstacleColor.R,
	_obstacleColor.G,
	_obstacleColor.B,
	0.7f  // alpha massimo per il test
));

	trailMesh.SetSurfaceOverrideMaterial(0, material);
	AddChild(trailMesh);
}
}

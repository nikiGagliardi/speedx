using Godot;
using System.Collections.Generic;

public partial class LifePowerUp : Area3D
{
	private float _time = 0f;
	private List<MeshInstance3D> _shieldParts = new();

	private static readonly Color ShieldColor = new Color(0.2f, 0.6f, 1.0f);
	private static readonly Color ShieldGlow  = new Color(0.4f, 0.8f, 1.0f);

	public override void _Ready()
	{
		BuildShield();
		AreaEntered += OnAreaEntered;
	}

	private void BuildShield()
	{
		// Sfera centrale
		var core = new MeshInstance3D();
		var coreMesh = new SphereMesh();
		coreMesh.Radius = 0.3f;
		coreMesh.Height = 0.6f;
		var coreMat = new StandardMaterial3D();
		coreMat.EmissionEnabled          = true;
		coreMat.Emission                 = ShieldGlow;
		coreMat.EmissionEnergyMultiplier = 8.0f;
		coreMat.ShadingMode              = BaseMaterial3D.ShadingModeEnum.Unshaded;
		coreMat.AlbedoColor              = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		coreMesh.SurfaceSetMaterial(0, coreMat);
		core.Mesh     = coreMesh;
		core.Position = new Vector3(0, 0.8f, 0);
		AddChild(core);
		_shieldParts.Add(core);

		// Scudo esterno semitrasparente
		var shield = new MeshInstance3D();
		var shieldMesh = new SphereMesh();
		shieldMesh.Radius = 0.9f;
		shieldMesh.Height = 1.8f;
		var shieldMat = new StandardMaterial3D();
		shieldMat.EmissionEnabled          = true;
		shieldMat.Emission                 = ShieldColor;
		shieldMat.EmissionEnergyMultiplier = 3.0f;
		shieldMat.ShadingMode              = BaseMaterial3D.ShadingModeEnum.Unshaded;
		shieldMat.Transparency             = BaseMaterial3D.TransparencyEnum.Alpha;
		shieldMat.AlbedoColor              = new Color(ShieldColor.R, ShieldColor.G, ShieldColor.B, 0.25f);
		shieldMat.CullMode                 = BaseMaterial3D.CullModeEnum.Disabled;
		shieldMesh.SurfaceSetMaterial(0, shieldMat);
		shield.Mesh     = shieldMesh;
		shield.Position = new Vector3(0, 0.8f, 0);
		AddChild(shield);
		_shieldParts.Add(shield);

		// Anelli
		AddRing(0.8f,  0.05f, 0.8f, ShieldColor, 4.0f, 0.9f);
		var ring2 = AddRing(0.75f, 0.04f, 0.8f, ShieldGlow, 4.5f, 0.8f);
		ring2.Rotation = new Vector3(Mathf.Pi / 3f, 0, 0);
		var ring3 = AddRing(0.75f, 0.04f, 0.8f, ShieldGlow, 4.5f, 0.8f);
		ring3.Rotation = new Vector3(-Mathf.Pi / 3f, 0, 0);

		// Base disco
		var base3D  = new MeshInstance3D();
		var disc    = new CylinderMesh();
		disc.TopRadius    = 0.6f;
		disc.BottomRadius = 0.6f;
		disc.Height       = 0.03f;
		var baseMat = new StandardMaterial3D();
		baseMat.EmissionEnabled          = true;
		baseMat.Emission                 = ShieldColor;
		baseMat.EmissionEnergyMultiplier = 4.0f;
		baseMat.ShadingMode              = BaseMaterial3D.ShadingModeEnum.Unshaded;
		baseMat.Transparency             = BaseMaterial3D.TransparencyEnum.Alpha;
		baseMat.AlbedoColor              = new Color(ShieldColor.R, ShieldColor.G, ShieldColor.B, 0.6f);
		disc.SurfaceSetMaterial(0, baseMat);
		base3D.Mesh     = disc;
		base3D.Position = new Vector3(0, 0.02f, 0);
		AddChild(base3D);
	}

	private MeshInstance3D AddRing(float radius, float height, float posY, Color color, float emission, float alpha)
	{
		var ring     = new MeshInstance3D();
		var ringMesh = new TorusMesh();
		ringMesh.InnerRadius = radius - 0.05f;
		ringMesh.OuterRadius = radius;
		var ringMat = new StandardMaterial3D();
		ringMat.EmissionEnabled          = true;
		ringMat.Emission                 = color;
		ringMat.EmissionEnergyMultiplier = emission;
		ringMat.ShadingMode              = BaseMaterial3D.ShadingModeEnum.Unshaded;
		ringMat.Transparency             = BaseMaterial3D.TransparencyEnum.Alpha;
		ringMat.AlbedoColor              = new Color(color.R, color.G, color.B, alpha);
		ringMesh.SurfaceSetMaterial(0, ringMat);
		ring.Mesh     = ringMesh;
		ring.Position = new Vector3(0, posY, 0);
		AddChild(ring);
		_shieldParts.Add(ring);
		return ring;
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;

		float pulse = 1.0f + Mathf.Sin(_time * 2.5f) * 0.08f;
		Scale = new Vector3(pulse, pulse, pulse);

		if (_shieldParts.Count > 2)
		{
			_shieldParts[2].RotateY((float)delta * 1.5f);
			_shieldParts[3].RotateY((float)delta * -2.0f);
			_shieldParts[4].RotateY((float)delta * 1.0f);
		}

		float emissionPulse = 3.0f + Mathf.Sin(_time * 4.0f) * 1.0f;
		if (_shieldParts.Count > 1)
		{
			var shieldMat = _shieldParts[1].GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
			if (shieldMat != null)
				shieldMat.EmissionEnergyMultiplier = emissionPulse;
		}
	}

	private void OnAreaEntered(Area3D area)
	{
		if (area.GetParent().IsInGroup("player"))
		{
			GameManager.Instance?.AddLife();
			QueueFree();
		}
	}
}

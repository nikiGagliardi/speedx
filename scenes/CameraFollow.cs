using Godot;

public partial class CameraFollow : Camera3D
{
	[Export] public NodePath TargetPath;
	[Export] public Vector3 Offset = new Vector3(0, 1.5f, 2f);
	[Export] public float SwayAmount = 0.03f;
	[Export] public float SwaySpeed = 2f;
	[Export] public float TiltAmount = 0.06f;
	[Export] public float TiltSpeed = 6f;
	[Export] public float BaseFov = 75f;
[Export] public float MaxFov = 100f;

	private Node3D _target;
	private float _swayTime = 0f;
	private float _previousX = 0f;
	private float _currentTilt = 0f;


	public override void _Ready()
	{
		if (TargetPath != null)
			_target = GetNode<Node3D>(TargetPath);
	}

public override void _PhysicsProcess(double delta)
{
	if (_target == null) return;

	Position = _target.Position + Offset;

	float speed = GameManager.Instance?.CurrentSpeed ?? 10f;
	float maxSpeed = GameManager.Instance?.MaxSpeed ?? 40f;
	float speedRatio = speed / maxSpeed;

	// FOV aumenta con la velocità
	Fov = Mathf.Lerp(BaseFov, MaxFov, speedRatio);

	// Oscillazione e tilt esistenti
	float deltaX = _target.Position.X - _previousX;
	_previousX = _target.Position.X;
	float targetTilt = -deltaX * TiltAmount * 50f;
	_currentTilt = Mathf.Lerp(_currentTilt, targetTilt, TiltSpeed * (float)delta);
	_swayTime += (float)delta * SwaySpeed;
	float sway = Mathf.Sin(_swayTime) * SwayAmount;

	Rotation = new Vector3(Rotation.X, Rotation.Y, sway + _currentTilt);
}
}

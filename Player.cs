using Godot;

public partial class Player : CharacterBody3D
{
	[Export] public float LaneWidth = 2.5f;
	[Export] public float LaneChangeSpeed = 12f;
	[Export] public float TiltThreshold = 0.6f;
	[Export] public float TiltSensitivity = 4.5f;
	[Export] public float MaxX = 6.25f;
	[Export] public float KeyboardSpeed = 30f;

	private float _tiltX = 0f;
	private int _maxLane = 0;
	private int _currentLane = 0;
	private float _targetX = 0f;
	private Vector2 _touchStart;
	private bool _isDead = false;
	private bool _isInvincible = false;
	private float _invincibilityTimer = 0f;
	private const float InvincibilityDuration = 1.5f;

public override void _Ready()
{
	AddToGroup("player");
	SettingsManager.Load();
	TiltSensitivity = SettingsManager.Sensitivity;
	TiltThreshold   = SettingsManager.Threshold;

	// Collega il segnale area_entered per rilevare ostacoli
	var area = GetNode<Area3D>("Area3D");
	area.AreaEntered += OnAreaEntered;
}

private void OnAreaEntered(Area3D area)
{
	if (area.IsInGroup("obstacle") && !_isDead && !_isInvincible)
	{
		area.QueueFree();

		bool survived = GameManager.Instance?.TryLoseLife() ?? false;
		if (!survived)
		{
			_isDead = true;
			GameManager.Instance?.TriggerGameOver();
		}
		else
		{
			_isInvincible = true;
			_invincibilityTimer = InvincibilityDuration;
		}
	}
}

	private void UpdateLanes()
	{
		int laneCount = GameManager.LaneCount;
		int halfLanes = laneCount / 2;
		MaxX = halfLanes * LaneWidth;
		_maxLane = halfLanes;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_maxLane == 0 && GameManager.Instance != null)
			UpdateLanes();

		// Aggiorna timer invincibilità
		if (_isInvincible)
		{
			_invincibilityTimer -= (float)delta;
			if (_invincibilityTimer <= 0f)
				_isInvincible = false;
		}

		if (_isDead) return;

		HandleInput();
		HandleAccelerometer();
		MoveForward((float)delta);
		SmoothLaneChange((float)delta);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventScreenTouch touch && touch.Pressed)
			_touchStart = touch.Position;

		if (@event is InputEventScreenDrag drag)
		{
			float deltaX = drag.Position.X - _touchStart.X;
			if (Mathf.Abs(deltaX) > 50f)
			{
				ChangeLane(deltaX > 0 ? 1 : -1);
				_touchStart = drag.Position;
			}
		}
	}

	private void HandleAccelerometer()
	{
		Vector3 accel = Input.GetAccelerometer();
		if (Mathf.Abs(accel.X) > TiltThreshold)
		{
			_targetX += accel.X * TiltSensitivity * (float)GetProcessDeltaTime();
			_targetX = Mathf.Clamp(_targetX, -MaxX, MaxX);
		}
	}

	private void HandleInput()
	{
		if (Input.IsActionPressed("ui_left"))
			_targetX = Mathf.Clamp(_targetX - KeyboardSpeed * (float)GetProcessDeltaTime(), -MaxX, MaxX);
		if (Input.IsActionPressed("ui_right"))
			_targetX = Mathf.Clamp(_targetX + KeyboardSpeed * (float)GetProcessDeltaTime(), -MaxX, MaxX);
	}

	private void ChangeLane(int dir)
	{
		_currentLane = Mathf.Clamp(_currentLane + dir, -_maxLane, _maxLane);
		_targetX = _currentLane * LaneWidth;
	}

	private void SmoothLaneChange(float delta)
	{
		var pos = Position;
		pos.X = Mathf.Lerp(pos.X, _targetX, LaneChangeSpeed * delta);
		Position = pos;
	}

private void MoveForward(float delta)
{
	var velocity = Velocity;
	velocity.Z = -(GameManager.Instance?.CurrentSpeed ?? 10f);
	velocity.X = 0f;
	Velocity = velocity;
	MoveAndSlide();
}
}

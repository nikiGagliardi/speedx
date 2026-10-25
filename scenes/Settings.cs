using Godot;
using System;

public partial class Settings : Control
{
	private HSlider _sensitivitySlider;
	private HSlider _thresholdSlider;
	private HSlider _volumeSlider;
	private Label _sensitivityValue;
	private Label _thresholdValue;
	private Label _volumeValue;
	private Button _backButton;
	private HSlider _lanesSlider;
	private Label _lanesValue;

	private const string SavePath = "user://settings.cfg";

	public override void _Ready()
	{
		_sensitivitySlider  = GetNode<HSlider>("VBoxContainer/SensitivitySlider");
		_thresholdSlider    = GetNode<HSlider>("VBoxContainer/ThresholdSlider");
		_volumeSlider       = GetNode<HSlider>("VBoxContainer/VolumeSlider");
		_sensitivityValue   = GetNode<Label>("VBoxContainer/SensitivityValue");
		_thresholdValue     = GetNode<Label>("VBoxContainer/ThresholdValue");
		_volumeValue        = GetNode<Label>("VBoxContainer/VolumeValue");
		_backButton         = GetNode<Button>("VBoxContainer/BackButton");
		_lanesSlider = GetNode<HSlider>("VBoxContainer/LanesSlider");
		_lanesValue  = GetNode<Label>("VBoxContainer/LanesValue");

		// Collega i segnali
		_sensitivitySlider.ValueChanged += OnSensitivityChanged;
		_thresholdSlider.ValueChanged   += OnThresholdChanged;
		_volumeSlider.ValueChanged      += OnVolumeChanged;
		_backButton.Pressed             += OnBackPressed;
		_lanesSlider.ValueChanged += OnLanesChanged;

		// Carica impostazioni salvate
		LoadSettings();
	}

	private void OnSensitivityChanged(double value)
	{
		_sensitivityValue.Text = $"{value:F1}";
		SaveSettings();
	}

	private void OnThresholdChanged(double value)
	{
		_thresholdValue.Text = $"{value:F1}";
		SaveSettings();
	}

	private void OnVolumeChanged(double value)
	{
		_volumeValue.Text = $"{(int)(value * 100)}%";

		// Applica il volume alla musica in tempo reale
		var music = GetNode<AudioStreamPlayer>("/root/MusicPlayer") as MusicPlayer;
		music?.SetVolume((float)value);
		SaveSettings();
	}

	private void OnBackPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}

	private void SaveSettings()
	{
		var config = new ConfigFile();
		config.SetValue("settings", "sensitivity", _sensitivitySlider.Value);
		config.SetValue("settings", "threshold",   _thresholdSlider.Value);
		config.SetValue("settings", "volume",      _volumeSlider.Value);
		config.SetValue("settings", "lanes", _lanesSlider.Value);
		config.Save(SavePath);
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();
		if (config.Load(SavePath) != Error.Ok)
		{
			// Prima volta — usa valori di default
			UpdateLabels();
			return;
		}

		_sensitivitySlider.Value = (double)config.GetValue("settings", "sensitivity", 4.5);
		_thresholdSlider.Value   = (double)config.GetValue("settings", "threshold",   2.0);
		_volumeSlider.Value      = (double)config.GetValue("settings", "volume",      1.0);
		_lanesSlider.Value = (double)config.GetValue("settings", "lanes", 5);

		UpdateLabels();
	}
	private void OnLanesChanged(double value)
	{
		_lanesValue.Text = $"{(int)value} corsie";
		SaveSettings();
	}

	private void UpdateLabels()
	{
		_sensitivityValue.Text = $"{_sensitivitySlider.Value:F1}";
		_thresholdValue.Text   = $"{_thresholdSlider.Value:F1}";
		_volumeValue.Text      = $"{(int)(_volumeSlider.Value * 100)}%";
		_lanesValue.Text = $"{(int)_lanesSlider.Value} corsie";
	}
}

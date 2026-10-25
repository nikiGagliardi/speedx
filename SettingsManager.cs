using Godot;
using System;

public static class SettingsManager
{
	private const string SavePath = "user://settings.cfg";
	public static int Lanes { get; private set; } = 5;

	public static float Sensitivity { get; private set; } = 4.5f;
	public static float Threshold   { get; private set; } = 2.0f;
	public static float Volume      { get; private set; } = 1.0f;

public static void Load()
{
	var config = new ConfigFile();
	if (config.Load(SavePath) != Error.Ok) return;

	Sensitivity = (float)(double)config.GetValue("settings", "sensitivity", 4.5);
	Threshold   = (float)(double)config.GetValue("settings", "threshold",   2.0);
	Volume      = (float)(double)config.GetValue("settings", "volume",      1.0);
	Lanes       = (int)(double)config.GetValue("settings", "lanes",         5.0);
}
}

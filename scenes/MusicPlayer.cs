using Godot;
using System;

public partial class MusicPlayer : AudioStreamPlayer
{
	public override void _Ready()
	{
		AddToGroup("music");

		var stream = Stream as AudioStreamOggVorbis;
		if (stream != null)
			stream.Loop = true;

		if (!Playing)
			Play();
	}

	public void SetVolume(float linearVolume)
	{
		VolumeDb = Mathf.LinearToDb(linearVolume);
	}
}

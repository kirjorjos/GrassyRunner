using Godot;
using System;

public partial class PlayButton : Button
{
	public void OnPressed() {
		GlobalEvents globalEvents = GetNode<GlobalEvents>("/root/GlobalEvents");
		globalEvents.HandleStateReset();
		GetTree().ChangeSceneToFile("res://Past.tscn");
	}
}

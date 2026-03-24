using Godot;
using System;

public partial class TutorialButton : Button {
	public void OnPressed() {
		GetTree().ChangeSceneToFile("res://HowToPlay.tscn");
	}
}

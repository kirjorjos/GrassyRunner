using Godot;
using System;

public partial class MainMenuButton : Button {
	public void OnMainMenuButtonPressed() {
		GetTree().ChangeSceneToFile("res://Title.tscn");
	}
}

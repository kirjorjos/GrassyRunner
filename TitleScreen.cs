using Godot;

public partial class TitleScreen : Control
{
	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("MainButton")) {
			GlobalEvents globalEvents = GetNode<GlobalEvents>("/root/GlobalEvents");
			globalEvents.HandleStateReset();
			GetTree().ChangeSceneToFile("res://Past.tscn");
		}
	}
}

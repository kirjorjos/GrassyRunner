using Godot;
using System;

public partial class GlobalEvents : Node
{
	private static int MinPlatformHeight = 2;
	private static int MaxPlatformHeight = 6;
	private static int MinPlatformWidth = 10;
	private static int MaxPlatformWidth = 25;
	private bool isActionHeld;
	private bool isDoubleTapping;
	private double lastTapTime;
	private SceneTreeTimer clickTimer;
	private Actions lastAction;
	public static Node2D currentWorld;
	public static double runSpeed = 1000.0;
	private double yVelocity = 0.0;
	private const double gravity = 1200.0;
	private bool isRolling = false;
	private static Random rng = new Random();

	public enum Actions {
		cycleTime,
		jump,
		roll,
		clear
	}

	public const double DoubleTapSecDelay = 0.25;

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("MainButton")) {
			double currentTime = Time.GetTicksMsec() / 1000.0;
			isActionHeld = true;

			if (currentTime - lastTapTime < DoubleTapSecDelay) {
				isDoubleTapping = true;
				lastTapTime = -1.0;
			} else {
				isDoubleTapping = false;
				lastTapTime = currentTime;
				
				clickTimer = GetTree().CreateTimer(DoubleTapSecDelay);
				clickTimer.Timeout += OnSingleTapTimeout;
			}
		} else if (@event.IsActionReleased("MainButton")) {
			isActionHeld = false;
		}
	}

	private void OnSingleTapTimeout() {
		if (!isDoubleTapping && !isActionHeld) TickLoop(Actions.cycleTime);
	}

	public override void _Process(double delta) {
		if (currentWorld == null) return;

		if (isActionHeld) {
			TickLoop(isDoubleTapping ? Actions.roll : Actions.jump);
		} else {
			TickLoop(Actions.clear);
		}

		CharacterBody2D player = currentWorld.GetNode<CharacterBody2D>("Player");
		AnimatedSprite2D animation = player.GetNode<AnimatedSprite2D>("PlayerSprite");
		
		bool isGrounded = player.TestMove(player.GlobalTransform, new Vector2(0, 1));
		if (!isGrounded) {
			yVelocity += gravity * delta;
		} else if (yVelocity > 0) {
			yVelocity = 0;
		}

		animation.Play("Run");

		Vector2 motion = new Vector2((float)runSpeed * (float)delta, (float)yVelocity * (float)delta);

		foreach (Node child in currentWorld.GetChildren()) {
			if (child is TileMapLayer layer) {
				layer.Position -= motion;
			}
		}
	}

	private void TickLoop(Actions currentAction) {
		CharacterBody2D player = currentWorld.GetNode<CharacterBody2D>("Player");
		CollisionShape2D collision = player.GetNode<CollisionShape2D>("PlayerCollision");

		bool isGrounded = player.TestMove(player.GlobalTransform, new Vector2(0, 1));

		switch(currentAction) {
			case Actions.jump:
				if (lastAction != Actions.jump && isGrounded) {
					yVelocity = -600.0;
				}
				break;
			case Actions.roll:
				if (!isRolling) {
					isRolling = true;
					collision.Scale = new Vector2(1, 0.5f);
				}
				break;
			case Actions.clear:
				if (isRolling) {
					isRolling = false;
					collision.Scale = new Vector2(1, 1.0f);
				}
				break;
		}
		lastAction = currentAction;
	}


	public static Vector2I MakeMetaTile(Vector2I topLeftPos, TimeWorld world) {
		TileMapLayer tileMap = world.tileMap;

		int width = rng.Next(MinPlatformWidth, MaxPlatformWidth);
		int height = rng.Next(MinPlatformHeight, MaxPlatformHeight);
		int sourceId = 0;

		var palette = world.Palette;
		GD.Print($"Generating platform at {topLeftPos} (Width: {width})");

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Vector2I cellPos = new(topLeftPos.X + x, topLeftPos.Y + y);
				string type;
				int alternative = 0;

				if (y == 0) {
					if (x == 0) type = "topLeft";
					else if (x == width - 1) type = "topRight";
					else type = "topMiddle";
				} else if (y == height - 1) {
					if (x == 0) type = "bottomLeft";
					else if (x == width - 1) type = "bottomRight";
					else type = "bottomMiddle";
				} else {
					if (x == 0) type = "leftWall";
					else if (x == width - 1) type = "rightWall";
					else type = "interior";
				}

				if (palette.ContainsKey(type)) {
					Vector2I[] options = palette[type];
					Vector2I atlasPos = options[rng.Next(options.Length)];
					if (atlasPos.Y < 0) atlasPos.Y = 0;
					tileMap.SetCell(cellPos, sourceId, atlasPos, alternative);
				}
			}
		}

		// Scenery
		var scenery = world.Scenery;
		if (scenery != null && scenery.Count > 0) {
			for (int x = 1; x < width - 1; x++) {
				if (rng.NextDouble() > 0.4) continue;

				SceneryItem item = scenery[rng.Next(scenery.Count)];
				if (x + item.Size.X < width) {
					int verticalOffset = (item.Size.Y > 1) ? 1 : 0;
					tileMap.SetCell(new Vector2I(topLeftPos.X + x, topLeftPos.Y - item.Size.Y + verticalOffset), sourceId, item.AtlasPos, 0);
					x += item.Size.X; 
				}
			}
		}

		return new Vector2I(width, height);
	}
	}


namespace ChessPlusPlus.UI
{
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using Godot;

	public partial class MainMenu : Control
	{
		[Export] public PackedScene GameScene { get; set; } = null!;

		private Button startButton = null!;
		private Button playAsWhiteButton = null!;
		private Button playAsBlackButton = null!;
		private Label titleLabel = null!;
		private VBoxContainer configContainer = null!;

		public override void _Ready()
		{
			SetupUI();
			ConnectSignals();
		}

		private void SetupUI()
		{
			// Set up main container
			var mainContainer = new VBoxContainer();
			mainContainer.Name = "MainContainer";
			mainContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			mainContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			mainContainer.AnchorLeft = 0.5f;
			mainContainer.AnchorTop = 0.5f;
			mainContainer.AnchorRight = 0.5f;
			mainContainer.AnchorBottom = 0.5f;
			mainContainer.Position = new Vector2(-200, -150);
			mainContainer.Size = new Vector2(400, 300);
			AddChild(mainContainer);

			// Title
			titleLabel = new Label();
			titleLabel.Name = "TitleLabel";
			titleLabel.Text = "Chess++";
			titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
			mainContainer.AddChild(titleLabel);

			// Spacer
			var spacer1 = new Control();
			spacer1.CustomMinimumSize = new Vector2(0, 30);
			mainContainer.AddChild(spacer1);

			// Configuration container
			configContainer = new VBoxContainer();
			configContainer.Name = "ConfigContainer";
			mainContainer.AddChild(configContainer);

			// Color selection label
			var colorLabel = new Label();
			colorLabel.Text = "Choose your side:";
			colorLabel.HorizontalAlignment = HorizontalAlignment.Center;
			configContainer.AddChild(colorLabel);

			// Color selection buttons container
			var colorButtonsContainer = new HBoxContainer();
			colorButtonsContainer.Alignment = BoxContainer.AlignmentMode.Center;
			configContainer.AddChild(colorButtonsContainer);

			// Play as White button
			playAsWhiteButton = new Button();
			playAsWhiteButton.Name = "PlayAsWhiteButton";
			playAsWhiteButton.Text = "Play as White";
			playAsWhiteButton.CustomMinimumSize = new Vector2(150, 40);
			colorButtonsContainer.AddChild(playAsWhiteButton);

			// Spacer between buttons
			var buttonSpacer = new Control();
			buttonSpacer.CustomMinimumSize = new Vector2(20, 0);
			colorButtonsContainer.AddChild(buttonSpacer);

			// Play as Black button
			playAsBlackButton = new Button();
			playAsBlackButton.Name = "PlayAsBlackButton";
			playAsBlackButton.Text = "Play as Black";
			playAsBlackButton.CustomMinimumSize = new Vector2(150, 40);
			colorButtonsContainer.AddChild(playAsBlackButton);

			// Spacer
			var spacer2 = new Control();
			spacer2.CustomMinimumSize = new Vector2(0, 30);
			mainContainer.AddChild(spacer2);

			// Start button
			startButton = new Button();
			startButton.Name = "StartButton";
			startButton.Text = "Start Game";
			startButton.CustomMinimumSize = new Vector2(200, 50);
			startButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
			startButton.Disabled = true; // Initially disabled until color is selected
			mainContainer.AddChild(startButton);
		}

		private void ConnectSignals()
		{
			playAsWhiteButton.Pressed += () => OnColorSelected(PieceColor.White);
			playAsBlackButton.Pressed += () => OnColorSelected(PieceColor.Black);
			startButton.Pressed += OnStartGame;
		}

		private void OnColorSelected(PieceColor color)
		{
			GameConfig.Instance.SetPlayerColor(color);

			// Update button appearances
			if (color == PieceColor.White)
			{
				playAsWhiteButton.ButtonPressed = true;
				playAsBlackButton.ButtonPressed = false;
			}
			else
			{
				playAsWhiteButton.ButtonPressed = false;
				playAsBlackButton.ButtonPressed = true;
			}

			// Enable start button
			startButton.Disabled = false;

			GD.Print($"Player selected: {color}");
		}

		private void OnStartGame()
		{
			// Load the game scene
			GameScene = GD.Load<PackedScene>("res://Scenes/game.tscn");
			if (GameScene != null)
			{
				GetTree().ChangeSceneToPacked(GameScene);
			}
			else
			{
				GD.PrintErr("Failed to load game scene!");
			}
		}
	}
}

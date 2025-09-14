namespace ChessPlusPlus.UI
{
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Network;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	public partial class MainMenu : Control
	{
		[Export]
		public PackedScene GameScene { get; set; } = null!;

		[Export]
		public PackedScene ArmyCustomizationScene { get; set; } = null!;

		private Button startButton = null!;
		private Button customizeArmyButton = null!;
		private Button playAsWhiteButton = null!;
		private Button playAsBlackButton = null!;
		private Label titleLabel = null!;
		private VBoxContainer configContainer = null!;

		// Game mode buttons
		private Button devModeButton = null!;
		private Button aiModeButton = null!;
		private Button networkModeButton = null!;

		// AI difficulty buttons
		private HBoxContainer aiDifficultyContainer = null!;
		private Button easyAIButton = null!;
		private Button mediumAIButton = null!;
		private Button hardAIButton = null!;

		// Network UI elements
		private VBoxContainer networkContainer = null!;
		private Button hostGameButton = null!;
		private Button joinGameButton = null!;
		private LineEdit ipAddressInput = null!;
		private Label connectionStatusLabel = null!;

		public override void _Ready()
		{
			SetupUI();
			ConnectSignals();
		}

		private void SetupUI()
		{
			// Create a centering container that fills the screen
			var centerContainer = new CenterContainer();
			centerContainer.Name = "CenterContainer";
			centerContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			AddChild(centerContainer);

			// Set up main container
			var mainContainer = new VBoxContainer();
			mainContainer.Name = "MainContainer";
			centerContainer.AddChild(mainContainer);

			// Title
			titleLabel = new Label();
			titleLabel.Name = "TitleLabel";
			titleLabel.Text = "Chess++";
			titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			titleLabel.AddThemeFontSizeOverride("font_size", 48);
			titleLabel.AddThemeColorOverride("font_color", Colors.White);
			mainContainer.AddChild(titleLabel);

			// Spacer
			var spacer1 = new Control();
			spacer1.CustomMinimumSize = new Vector2(0, 40);
			mainContainer.AddChild(spacer1);

			// Configuration container
			configContainer = new VBoxContainer();
			configContainer.Name = "ConfigContainer";
			mainContainer.AddChild(configContainer);

			// Game mode selection label
			var gameModeLabel = new Label();
			gameModeLabel.Text = "Select Game Mode:";
			gameModeLabel.HorizontalAlignment = HorizontalAlignment.Center;
			gameModeLabel.AddThemeFontSizeOverride("font_size", 20);
			configContainer.AddChild(gameModeLabel);

			// Game mode buttons container
			var gameModeContainer = new HBoxContainer();
			gameModeContainer.Alignment = BoxContainer.AlignmentMode.Center;
			configContainer.AddChild(gameModeContainer);

			// Dev Mode button (Player vs Player locally)
			devModeButton = new Button();
			devModeButton.Name = "DevModeButton";
			devModeButton.Text = "Dev Mode\n(vs Self)";
			devModeButton.CustomMinimumSize = new Vector2(120, 50);
			devModeButton.ToggleMode = true;
			devModeButton.ButtonPressed = true; // Default selection
			gameModeContainer.AddChild(devModeButton);

			// Spacer
			var modeSpacer1 = new Control();
			modeSpacer1.CustomMinimumSize = new Vector2(10, 0);
			gameModeContainer.AddChild(modeSpacer1);

			// AI Mode button
			aiModeButton = new Button();
			aiModeButton.Name = "AIModeButton";
			aiModeButton.Text = "vs AI";
			aiModeButton.CustomMinimumSize = new Vector2(120, 50);
			aiModeButton.ToggleMode = true;
			gameModeContainer.AddChild(aiModeButton);

			// Spacer
			var modeSpacer2 = new Control();
			modeSpacer2.CustomMinimumSize = new Vector2(10, 0);
			gameModeContainer.AddChild(modeSpacer2);

			// Network Mode button
			networkModeButton = new Button();
			networkModeButton.Name = "NetworkModeButton";
			networkModeButton.Text = "LAN Game";
			networkModeButton.CustomMinimumSize = new Vector2(120, 50);
			networkModeButton.ToggleMode = true;
			networkModeButton.Disabled = false; // Now implemented!
			gameModeContainer.AddChild(networkModeButton);

			// Spacer
			var spacerAfterMode = new Control();
			spacerAfterMode.CustomMinimumSize = new Vector2(0, 20);
			configContainer.AddChild(spacerAfterMode);

			// AI Difficulty selection (initially hidden)
			aiDifficultyContainer = new HBoxContainer();
			aiDifficultyContainer.Alignment = BoxContainer.AlignmentMode.Center;
			aiDifficultyContainer.Visible = false;
			configContainer.AddChild(aiDifficultyContainer);

			var difficultyLabel = new Label();
			difficultyLabel.Text = "AI Difficulty: ";
			aiDifficultyContainer.AddChild(difficultyLabel);

			easyAIButton = new Button();
			easyAIButton.Text = "Easy";
			easyAIButton.CustomMinimumSize = new Vector2(80, 35);
			easyAIButton.ToggleMode = true;
			aiDifficultyContainer.AddChild(easyAIButton);

			mediumAIButton = new Button();
			mediumAIButton.Text = "Medium";
			mediumAIButton.CustomMinimumSize = new Vector2(80, 35);
			mediumAIButton.ToggleMode = true;
			mediumAIButton.ButtonPressed = true; // Default difficulty
			aiDifficultyContainer.AddChild(mediumAIButton);

			hardAIButton = new Button();
			hardAIButton.Text = "Hard";
			hardAIButton.CustomMinimumSize = new Vector2(80, 35);
			hardAIButton.ToggleMode = true;
			aiDifficultyContainer.AddChild(hardAIButton);

			// Spacer after difficulty
			var spacerAfterDifficulty = new Control();
			spacerAfterDifficulty.CustomMinimumSize = new Vector2(0, 20);
			configContainer.AddChild(spacerAfterDifficulty);

			// Network game container (initially hidden)
			networkContainer = new VBoxContainer();
			networkContainer.Alignment = BoxContainer.AlignmentMode.Center;
			networkContainer.Visible = false;
			configContainer.AddChild(networkContainer);

			// Host/Join buttons
			var networkButtonsContainer = new HBoxContainer();
			networkButtonsContainer.Alignment = BoxContainer.AlignmentMode.Center;
			networkContainer.AddChild(networkButtonsContainer);

			hostGameButton = new Button();
			hostGameButton.Text = "Host Game";
			hostGameButton.CustomMinimumSize = new Vector2(120, 40);
			networkButtonsContainer.AddChild(hostGameButton);

			var networkButtonSpacer = new Control();
			networkButtonSpacer.CustomMinimumSize = new Vector2(20, 0);
			networkButtonsContainer.AddChild(networkButtonSpacer);

			joinGameButton = new Button();
			joinGameButton.Text = "Join Game";
			joinGameButton.CustomMinimumSize = new Vector2(120, 40);
			networkButtonsContainer.AddChild(joinGameButton);

			// IP address input
			var ipContainer = new HBoxContainer();
			ipContainer.Alignment = BoxContainer.AlignmentMode.Center;
			networkContainer.AddChild(ipContainer);

			var ipLabel = new Label();
			ipLabel.Text = "Host IP: ";
			ipContainer.AddChild(ipLabel);

			ipAddressInput = new LineEdit();
			ipAddressInput.Text = "127.0.0.1";
			ipAddressInput.PlaceholderText = "Enter host IP address";
			ipAddressInput.CustomMinimumSize = new Vector2(200, 30);
			ipContainer.AddChild(ipAddressInput);

			// Connection status
			connectionStatusLabel = new Label();
			connectionStatusLabel.Text = "";
			connectionStatusLabel.HorizontalAlignment = HorizontalAlignment.Center;
			connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Yellow);
			networkContainer.AddChild(connectionStatusLabel);

			// Spacer after network
			var spacerAfterNetwork = new Control();
			spacerAfterNetwork.CustomMinimumSize = new Vector2(0, 20);
			configContainer.AddChild(spacerAfterNetwork);

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
			playAsWhiteButton.CustomMinimumSize = new Vector2(160, 45);
			playAsWhiteButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			playAsWhiteButton.ToggleMode = true;
			colorButtonsContainer.AddChild(playAsWhiteButton);

			// Spacer between buttons
			var buttonSpacer = new Control();
			buttonSpacer.CustomMinimumSize = new Vector2(20, 0);
			colorButtonsContainer.AddChild(buttonSpacer);

			// Play as Black button
			playAsBlackButton = new Button();
			playAsBlackButton.Name = "PlayAsBlackButton";
			playAsBlackButton.Text = "Play as Black";
			playAsBlackButton.CustomMinimumSize = new Vector2(160, 45);
			playAsBlackButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			playAsBlackButton.ToggleMode = true;
			colorButtonsContainer.AddChild(playAsBlackButton);

			// Spacer
			var spacer2 = new Control();
			spacer2.CustomMinimumSize = new Vector2(0, 30);
			mainContainer.AddChild(spacer2);

			// Button container for game options
			var gameButtonsContainer = new VBoxContainer();
			gameButtonsContainer.Alignment = BoxContainer.AlignmentMode.Center;
			mainContainer.AddChild(gameButtonsContainer);

			// Start button
			startButton = new Button();
			startButton.Name = "StartButton";
			startButton.Text = "Start Dev Mode"; // Default text for dev mode
			startButton.CustomMinimumSize = new Vector2(240, 55);
			startButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
			startButton.AddThemeFontSizeOverride("font_size", 16);
			startButton.Disabled = true; // Initially disabled until color is selected
			gameButtonsContainer.AddChild(startButton);

			// Small spacer between game buttons
			var gameButtonSpacer = new Control();
			gameButtonSpacer.CustomMinimumSize = new Vector2(0, 15);
			gameButtonsContainer.AddChild(gameButtonSpacer);

			// Customize Army button
			customizeArmyButton = new Button();
			customizeArmyButton.Name = "CustomizeArmyButton";
			customizeArmyButton.Text = "Customize Army";
			customizeArmyButton.CustomMinimumSize = new Vector2(240, 55);
			customizeArmyButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
			customizeArmyButton.AddThemeFontSizeOverride("font_size", 16);
			customizeArmyButton.Disabled = true; // Initially disabled until color is selected
			gameButtonsContainer.AddChild(customizeArmyButton);
		}

		private void ConnectSignals()
		{
			// Game mode buttons
			devModeButton.Pressed += () => OnGameModeSelected(GameMode.PlayerVsPlayer);
			aiModeButton.Pressed += () => OnGameModeSelected(GameMode.PlayerVsAI);
			networkModeButton.Pressed += () => OnNetworkModeSelected();

			// Network buttons
			hostGameButton.Pressed += OnHostGame;
			joinGameButton.Pressed += OnJoinGame;

			// AI difficulty buttons
			easyAIButton.Pressed += () => OnAIDifficultySelected(AIDifficulty.Easy);
			mediumAIButton.Pressed += () => OnAIDifficultySelected(AIDifficulty.Medium);
			hardAIButton.Pressed += () => OnAIDifficultySelected(AIDifficulty.Hard);

			// Color selection buttons
			playAsWhiteButton.Pressed += () => OnColorSelected(PieceColor.White);
			playAsBlackButton.Pressed += () => OnColorSelected(PieceColor.Black);

			// Action buttons
			startButton.Pressed += OnStartGame;
			customizeArmyButton.Pressed += OnCustomizeArmy;
		}

		private void OnGameModeSelected(GameMode mode)
		{
			GameConfig.Instance.Mode = mode;

			// Update button states
			devModeButton.ButtonPressed = mode == GameMode.PlayerVsPlayer;
			aiModeButton.ButtonPressed = mode == GameMode.PlayerVsAI;
			networkModeButton.ButtonPressed = false;

			// Show/hide AI difficulty selection
			aiDifficultyContainer.Visible = mode == GameMode.PlayerVsAI;
			networkContainer.Visible = false;

			// Update start button text
			startButton.Text = mode switch
			{
				GameMode.PlayerVsPlayer => "Start Dev Mode",
				GameMode.PlayerVsAI => "Start vs AI",
				_ => "Start Game",
			};

			// Re-enable color selection for non-network modes
			playAsWhiteButton.Disabled = false;
			playAsBlackButton.Disabled = false;

			GD.Print($"Game mode selected: {mode}");
		}

		private void OnNetworkModeSelected()
		{
			// Update button states
			devModeButton.ButtonPressed = false;
			aiModeButton.ButtonPressed = false;
			networkModeButton.ButtonPressed = true;

			// Show network UI, hide AI difficulty
			aiDifficultyContainer.Visible = false;
			networkContainer.Visible = true;

			// Disable start button until connected
			startButton.Text = "Waiting for connection...";
			startButton.Disabled = true;

			// Color will be determined by host/join
			playAsWhiteButton.Disabled = true;
			playAsBlackButton.Disabled = true;

			GD.Print("Network mode selected");
		}

		private void OnAIDifficultySelected(AIDifficulty difficulty)
		{
			GameConfig.Instance.AIDifficulty = difficulty;

			// Update button states
			easyAIButton.ButtonPressed = difficulty == AIDifficulty.Easy;
			mediumAIButton.ButtonPressed = difficulty == AIDifficulty.Medium;
			hardAIButton.ButtonPressed = difficulty == AIDifficulty.Hard;

			GD.Print($"AI difficulty selected: {difficulty}");
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

			// Enable buttons
			startButton.Disabled = false;
			customizeArmyButton.Disabled = false;

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

		private void OnCustomizeArmy()
		{
			// Load the army customization scene
			ArmyCustomizationScene = GD.Load<PackedScene>("res://Scenes/army_customization.tscn");
			if (ArmyCustomizationScene != null)
			{
				var customizationScene = ArmyCustomizationScene.Instantiate<ChessPlusPlus.UI.ArmyCustomization>();
				customizationScene.Initialize(GameConfig.Instance.PlayerColor);

				// Connect signals to handle navigation
				customizationScene.BackToMenu += OnBackToMenuFromCustomization;
				// Note: StartCustomGame signal is handled directly by ArmyCustomization now

				GetTree().Root.AddChild(customizationScene);
				QueueFree(); // Remove the main menu
			}
			else
			{
				GD.PrintErr("Failed to load army customization scene!");
			}
		}

		private void OnBackToMenuFromCustomization()
		{
			// Return to main menu from army customization
			var mainMenuScene = GD.Load<PackedScene>("res://Scenes/main_menu.tscn");
			if (mainMenuScene != null)
			{
				GetTree().ChangeSceneToPacked(mainMenuScene);
			}
		}

		private void OnStartCustomGame()
		{
			// The custom army is already stored in GameConfig by ArmyCustomization
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

		private void OnHostGame()
		{
			// Add NetworkManager to the scene if not already present
			var networkManager = NetworkManager.Instance;
			if (networkManager.GetParent() == null)
			{
				GetTree().Root.AddChild(networkManager);
			}

			// Host the game
			var error = networkManager.HostGame();
			if (error == Error.Ok)
			{
				// Host is always white
				GameConfig.Instance.SetPlayerColor(PieceColor.White);
				GameConfig.Instance.Mode = GameMode.PlayerVsPlayer; // Network games use PlayerVsPlayer mode

				playAsWhiteButton.ButtonPressed = true;
				playAsBlackButton.ButtonPressed = false;

				connectionStatusLabel.Text =
					$"Hosting on {networkManager.GetLocalIPAddress()}:7000\nWaiting for player...";
				connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Green);

				// Connect to network events
				networkManager.PlayerConnected += OnPlayerConnected;
				networkManager.PlayerDisconnected += OnPlayerDisconnected;

				startButton.Text = "Waiting for player...";
				startButton.Disabled = true;
			}
			else
			{
				connectionStatusLabel.Text = "Failed to host game!";
				connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Red);
			}
		}

		private void OnJoinGame()
		{
			// Add NetworkManager to the scene if not already present
			var networkManager = NetworkManager.Instance;
			if (networkManager.GetParent() == null)
			{
				GetTree().Root.AddChild(networkManager);
			}

			// Join the game
			var hostAddress = ipAddressInput.Text;
			var error = networkManager.JoinGame(hostAddress);

			if (error == Error.Ok)
			{
				// Client is always black
				GameConfig.Instance.SetPlayerColor(PieceColor.Black);
				GameConfig.Instance.Mode = GameMode.PlayerVsPlayer; // Network games use PlayerVsPlayer mode

				playAsWhiteButton.ButtonPressed = false;
				playAsBlackButton.ButtonPressed = true;

				connectionStatusLabel.Text = $"Connecting to {hostAddress}...";
				connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Yellow);

				// Connect to network events
				networkManager.ConnectionSucceeded += OnConnectionSucceeded;
				networkManager.ConnectionFailed += OnConnectionFailed;
				networkManager.ServerDisconnected += OnServerDisconnected;

				startButton.Text = "Connecting...";
				startButton.Disabled = true;
			}
			else
			{
				connectionStatusLabel.Text = "Failed to join game!";
				connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Red);
			}
		}

		private void OnPlayerConnected(int id)
		{
			GD.Print($"Player connected with ID: {id}");
			connectionStatusLabel.Text = "Player connected!";
			connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Green);

			// Enable start button for host
			if (NetworkManager.Instance.IsHost)
			{
				startButton.Text = "Start Network Game";
				startButton.Disabled = false;
			}
		}

		private void OnPlayerDisconnected(int id)
		{
			GD.Print($"Player disconnected with ID: {id}");
			connectionStatusLabel.Text = "Player disconnected";
			connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Red);

			startButton.Text = "Waiting for player...";
			startButton.Disabled = true;
		}

		private void OnConnectionSucceeded()
		{
			GD.Print("Successfully connected to host");
			connectionStatusLabel.Text = "Connected!";
			connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Green);

			// Client waits for host to start
			startButton.Text = "Waiting for host...";
			startButton.Disabled = true;

			// Auto-start for client when connected
			CallDeferred(nameof(StartNetworkGame));
		}

		private void OnConnectionFailed()
		{
			GD.PrintErr("Failed to connect to host");
			connectionStatusLabel.Text = "Connection failed!";
			connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Red);

			startButton.Text = "Connection failed";
			startButton.Disabled = true;
		}

		private void OnServerDisconnected()
		{
			GD.Print("Disconnected from server");
			connectionStatusLabel.Text = "Disconnected from host";
			connectionStatusLabel.AddThemeColorOverride("font_color", Colors.Red);
		}

		private void StartNetworkGame()
		{
			// Set network mode flag (we'll use this in GameManager)
			GameConfig.Instance.Mode = GameMode.PlayerVsPlayer;

			// Load game scene
			OnStartGame();
		}
	}
}

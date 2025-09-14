using ChessPlusPlus.Core;
using ChessPlusPlus.Network;
using ChessPlusPlus.Pieces;
using ChessPlusPlus.Players;
using ChessPlusPlus.UI.Builders;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;
using StylePresets = ChessPlusPlus.UI.Styles.StylePresets;
using UIBuilder = ChessPlusPlus.UI.Builders.UI;

namespace ChessPlusPlus.UI.Screens
{
	public partial class MainMenuScreen : ScreenBase
	{
		// Game mode buttons
		private Button? devModeButton;
		private Button? aiModeButton;
		private Button? networkModeButton;

		// Color selection buttons
		private Button? whiteButton;
		private Button? blackButton;

		// AI difficulty buttons
		private Button? easyAIButton;
		private Button? mediumAIButton;
		private Button? hardAIButton;

		// Network buttons
		private Button? hostGameButton;
		private Button? joinGameButton;
		private LineEdit? ipAddressInput;

		// UI containers
		private Control? aiDifficultyContainer;
		private Control? networkContainer;
		private Control? colorSelectionContainer;
		private Control? customizeArmyContainer;
		private Label? connectionStatusLabel;

		// State
		private bool useCustomArmy = false;

		protected override Control BuildUI()
		{
			// Main background
			var background = UIBuilder.ColorRect(ColorPalette.BackgroundDark).FullRect().Build();

			// Main content container
			var mainContent = UIBuilder
				.VBox()
				.ExpandFill()
				.Spacing(StylePresets.Spacing.Large)
				.Children(
					BuildTitle(),
					UIBuilder.Spacer(0, StylePresets.Spacing.ExtraLarge),
					BuildGameModeSection(),
					BuildColorSelectionSection(),
					BuildAIDifficultySection(),
					BuildNetworkSection(),
					BuildArmyCustomizationSection(),
					UIBuilder.Spacer(0, StylePresets.Spacing.Large),
					BuildActionButtons()
				)
				.AddTo(background);

			// Center the content
			var centerContainer = UIBuilder.CenterContainer().ExpandFill().Child(mainContent).AddTo(background);

			return background;
		}

		private Control BuildTitle()
		{
			return UIBuilder
				.VBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Small)
				.Children(
					UIBuilder.Label("Chess++").Title().FontSize(StylePresets.FontSizes.Huge).Build(),
					UIBuilder.Label("Enhanced Chess Experience").Subtitle().Muted().Build()
				)
				.Build();
		}

		private Control BuildGameModeSection()
		{
			var buttonGroup = new ButtonGroup();

			devModeButton = UIBuilder
				.Button("Dev Mode")
				.Size(StylePresets.ButtonSizes.Medium)
				.Toggle()
				.Pressed(true)
				.ButtonGroup(buttonGroup)
				.OnPress(OnDevModeSelected)
				.Build();

			aiModeButton = UIBuilder
				.Button("vs AI")
				.Size(StylePresets.ButtonSizes.Medium)
				.Toggle()
				.ButtonGroup(buttonGroup)
				.OnPress(OnAIModeSelected)
				.Build();

			networkModeButton = UIBuilder
				.Button("LAN Game")
				.Size(StylePresets.ButtonSizes.Medium)
				.Toggle()
				.ButtonGroup(buttonGroup)
				.OnPress(OnNetworkModeSelected)
				.Build();

			return UIBuilder
				.VBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Medium)
				.Children(
					UIBuilder.Label("Game Mode").Heading().Centered().Build(),
					UIBuilder
						.HBox()
						.CenterAlign()
						.Spacing(StylePresets.Spacing.Medium)
						.Children(devModeButton, aiModeButton, networkModeButton)
						.Build()
				)
				.Build();
		}

		private Control BuildColorSelectionSection()
		{
			var buttonGroup = new ButtonGroup();

			whiteButton = UIBuilder
				.Button("Play as White")
				.Size(StylePresets.ButtonSizes.Medium)
				.Toggle()
				.Pressed(true)
				.ButtonGroup(buttonGroup)
				.Build();

			blackButton = UIBuilder
				.Button("Play as Black")
				.Size(StylePresets.ButtonSizes.Medium)
				.Toggle()
				.ButtonGroup(buttonGroup)
				.Build();

			colorSelectionContainer = UIBuilder
				.VBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Medium)
				.Visible(false)
				.Children(
					UIBuilder.Label("Choose Your Color").Subtitle().Build(),
					UIBuilder
						.HBox()
						.CenterAlign()
						.Spacing(StylePresets.Spacing.Medium)
						.Children(whiteButton, blackButton)
						.Build()
				)
				.Build();

			return colorSelectionContainer;
		}

		private Control BuildAIDifficultySection()
		{
			var buttonGroup = new ButtonGroup();

			easyAIButton = UIBuilder
				.Button("Easy")
				.Size(StylePresets.ButtonSizes.Small)
				.Toggle()
				.ButtonGroup(buttonGroup)
				.Build();

			mediumAIButton = UIBuilder
				.Button("Medium")
				.Size(StylePresets.ButtonSizes.Small)
				.Toggle()
				.Pressed(true)
				.ButtonGroup(buttonGroup)
				.Build();

			hardAIButton = UIBuilder
				.Button("Hard")
				.Size(StylePresets.ButtonSizes.Small)
				.Toggle()
				.ButtonGroup(buttonGroup)
				.Build();

			aiDifficultyContainer = UIBuilder
				.VBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Small)
				.Visible(false)
				.Children(
					UIBuilder.Label("AI Difficulty").Subtitle().Build(),
					UIBuilder
						.HBox()
						.CenterAlign()
						.Spacing(StylePresets.Spacing.Small)
						.Children(easyAIButton, mediumAIButton, hardAIButton)
						.Build()
				)
				.Build();

			return aiDifficultyContainer;
		}

		private Control BuildNetworkSection()
		{
			hostGameButton = UIBuilder
				.Button("Host Game")
				.Size(StylePresets.ButtonSizes.Medium)
				.OnPress(OnHostGame)
				.Build();

			joinGameButton = UIBuilder
				.Button("Join Game")
				.Size(StylePresets.ButtonSizes.Medium)
				.OnPress(OnJoinGame)
				.Build();

			ipAddressInput = UIBuilder.LineEdit("Enter host IP address").Size(200, 35).Text("127.0.0.1").Build();

			connectionStatusLabel = UIBuilder.Label("").Muted().Centered().Build();

			networkContainer = UIBuilder
				.VBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Medium)
				.Visible(false)
				.Children(
					UIBuilder
						.HBox()
						.CenterAlign()
						.Spacing(StylePresets.Spacing.Medium)
						.Children(hostGameButton, joinGameButton)
						.Build(),
					UIBuilder
						.HBox()
						.CenterAlign()
						.Spacing(StylePresets.Spacing.Small)
						.Children(UIBuilder.Label("Host IP:").Build(), ipAddressInput)
						.Build(),
					connectionStatusLabel
				)
				.Build();

			return networkContainer;
		}

		private Control BuildArmyCustomizationSection()
		{
			var toggleButton = UIBuilder
				.Button("Standard Army")
				.Size(StylePresets.ButtonSizes.Wide)
				.Toggle()
				.OnToggle(OnToggleCustomArmy)
				.Build();

			customizeArmyContainer = UIBuilder
				.VBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Small)
				.Visible(false)
				.Children(UIBuilder.Label("Army Type").Subtitle().Build(), toggleButton)
				.Build();

			return customizeArmyContainer;
		}

		private Control BuildActionButtons()
		{
			return UIBuilder
				.HBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Large)
				.Children(
					UIBuilder
						.Button("Quit")
						.Size(StylePresets.ButtonSizes.Large)
						.OnPress(() => GetTree().Quit())
						.Build(),
					UIBuilder
						.Button("Start Game")
						.Size(StylePresets.ButtonSizes.Large)
						.Primary()
						.OnPress(OnStartGame)
						.Build()
				)
				.Build();
		}

		protected override void OnReady()
		{
			// Initialize NetworkManager
			var networkManager = NetworkManager.Instance;
			networkManager.PlayerConnected += OnPlayerConnected;
			networkManager.PlayerDisconnected += OnPlayerDisconnected;
			networkManager.ConnectionFailed += OnConnectionFailed;
		}

		// Event handlers
		private void OnDevModeSelected()
		{
			colorSelectionContainer!.Visible = false;
			aiDifficultyContainer!.Visible = false;
			networkContainer!.Visible = false;
			customizeArmyContainer!.Visible = false;
		}

		private void OnAIModeSelected()
		{
			colorSelectionContainer!.Visible = true;
			aiDifficultyContainer!.Visible = true;
			networkContainer!.Visible = false;
			customizeArmyContainer!.Visible = true;
		}

		private void OnNetworkModeSelected()
		{
			colorSelectionContainer!.Visible = true;
			aiDifficultyContainer!.Visible = false;
			networkContainer!.Visible = true;
			customizeArmyContainer!.Visible = false;
		}

		private void OnToggleCustomArmy(bool pressed)
		{
			useCustomArmy = pressed;
			var button = customizeArmyContainer!.GetChild(1) as Button;
			if (button != null)
			{
				button.Text = pressed ? "Customize Army" : "Standard Army";
			}
		}

		private void OnHostGame()
		{
			connectionStatusLabel!.Text = "Starting server...";
			connectionStatusLabel.AddThemeColorOverride("font_color", ColorPalette.TextMuted);

			NetworkManager.Instance.HostGame();
			connectionStatusLabel.Text = "Waiting for opponent...";
		}

		private void OnJoinGame()
		{
			var ipAddress = ipAddressInput!.Text;
			if (string.IsNullOrEmpty(ipAddress))
			{
				connectionStatusLabel!.Text = "Please enter a valid IP address";
				connectionStatusLabel.AddThemeColorOverride("font_color", ColorPalette.Error);
				return;
			}

			connectionStatusLabel!.Text = $"Connecting to {ipAddress}...";
			connectionStatusLabel.AddThemeColorOverride("font_color", ColorPalette.TextMuted);

			NetworkManager.Instance.JoinGame(ipAddress);
		}

		private void OnStartGame()
		{
			var config = GameConfig.Instance;

			// Set game mode
			if (devModeButton!.ButtonPressed)
			{
				config.Mode = GameMode.PlayerVsPlayer;
			}
			else if (aiModeButton!.ButtonPressed)
			{
				config.Mode = GameMode.PlayerVsAI;
			}
			else if (networkModeButton!.ButtonPressed && NetworkManager.Instance.IsConnected)
			{
				// Network mode is handled through NetworkManager
			}

			// Set player color
			config.PlayerColor = whiteButton!.ButtonPressed ? PieceColor.White : PieceColor.Black;

			// Set AI difficulty
			if (easyAIButton!.ButtonPressed)
				config.AIDifficulty = AIDifficulty.Easy;
			else if (mediumAIButton!.ButtonPressed)
				config.AIDifficulty = AIDifficulty.Medium;
			else if (hardAIButton!.ButtonPressed)
				config.AIDifficulty = AIDifficulty.Hard;

			// Handle army customization
			if (useCustomArmy)
			{
				// Transition to army customization screen
				// GetTree().ChangeSceneToFile("res://Scenes/ArmyCustomization.tscn");
			}
			else
			{
				// Start the game
				GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
			}
		}

		// Network event handlers
		private void OnPlayerConnected(int peerId)
		{
			connectionStatusLabel!.Text = "Connected! Starting game...";
			connectionStatusLabel.AddThemeColorOverride("font_color", ColorPalette.Success);

			// Auto-start the game after a short delay
			GetTree().CreateTimer(1.0).Timeout += () => GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
		}

		private void OnPlayerDisconnected(int peerId)
		{
			connectionStatusLabel!.Text = "Disconnected from server";
			connectionStatusLabel.AddThemeColorOverride("font_color", ColorPalette.Warning);
		}

		private void OnConnectionFailed()
		{
			connectionStatusLabel!.Text = "Failed to connect";
			connectionStatusLabel.AddThemeColorOverride("font_color", ColorPalette.Error);
		}

		protected override void OnExit()
		{
			var networkManager = NetworkManager.Instance;
			networkManager.PlayerConnected -= OnPlayerConnected;
			networkManager.PlayerDisconnected -= OnPlayerDisconnected;
			networkManager.ConnectionFailed -= OnConnectionFailed;
		}
	}
}

namespace ChessPlusPlus.UI
{
	using System;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Network;
	using ChessPlusPlus.Pieces;
	using Godot;

	public partial class LANSetupScreen : Control
	{
		private NetworkManager networkManager = null!;
		private VBoxContainer mainContainer = null!;
		private Label statusLabel = null!;
		private Button customizeArmyButton = null!;
		private Button readyButton = null!;
		private Button startGameButton = null!;
		private HBoxContainer colorSelectionContainer = null!;
		private Button whiteButton = null!;
		private Button blackButton = null!;

		private Army? localArmy;
		private Army? remoteArmy;
		private PieceColor localColor = PieceColor.White;
		private bool isLocalReady = false;
		private bool isRemoteReady = false;

		[Signal]
		public delegate void BackToMenuEventHandler();

		[Signal]
		public delegate void StartLANGameEventHandler();

		public override void _Ready()
		{
			networkManager = NetworkManager.Instance;
			SetupUI();
			ConnectNetworkSignals();
		}

		private void SetupUI()
		{
			// Background
			var background = new ColorRect();
			background.Color = new Color(0.15f, 0.15f, 0.2f);
			background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			AddChild(background);

			// Main container
			var centerContainer = new CenterContainer();
			centerContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			AddChild(centerContainer);

			mainContainer = new VBoxContainer();
			mainContainer.AddThemeConstantOverride("separation", 20);
			centerContainer.AddChild(mainContainer);

			// Title
			var titleLabel = new Label();
			titleLabel.Text = "LAN Game Setup";
			titleLabel.AddThemeFontSizeOverride("font_size", 32);
			titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			mainContainer.AddChild(titleLabel);

			// Status label
			statusLabel = new Label();
			statusLabel.Text = networkManager.IsHost ? "Waiting for player to connect..." : "Connected to host";
			statusLabel.AddThemeFontSizeOverride("font_size", 18);
			statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
			mainContainer.AddChild(statusLabel);

			// Color selection (host only)
			if (networkManager.IsHost)
			{
				var colorLabel = new Label();
				colorLabel.Text = "Choose your color:";
				colorLabel.HorizontalAlignment = HorizontalAlignment.Center;
				mainContainer.AddChild(colorLabel);

				colorSelectionContainer = new HBoxContainer();
				colorSelectionContainer.Alignment = BoxContainer.AlignmentMode.Center;
				mainContainer.AddChild(colorSelectionContainer);

				whiteButton = new Button();
				whiteButton.Text = "Play as White";
				whiteButton.CustomMinimumSize = new Vector2(150, 40);
				whiteButton.ToggleMode = true;
				whiteButton.ButtonPressed = true;
				whiteButton.Pressed += () => OnColorSelected(PieceColor.White);
				colorSelectionContainer.AddChild(whiteButton);

				var spacer = new Control();
				spacer.CustomMinimumSize = new Vector2(20, 0);
				colorSelectionContainer.AddChild(spacer);

				blackButton = new Button();
				blackButton.Text = "Play as Black";
				blackButton.CustomMinimumSize = new Vector2(150, 40);
				blackButton.ToggleMode = true;
				blackButton.Pressed += () => OnColorSelected(PieceColor.Black);
				colorSelectionContainer.AddChild(blackButton);
			}
			else
			{
				// Client waits for color assignment
				var waitingLabel = new Label();
				waitingLabel.Text = "Waiting for host to assign colors...";
				waitingLabel.HorizontalAlignment = HorizontalAlignment.Center;
				waitingLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
				mainContainer.AddChild(waitingLabel);
			}

			// Army customization button
			customizeArmyButton = new Button();
			customizeArmyButton.Text = "Customize Army";
			customizeArmyButton.CustomMinimumSize = new Vector2(200, 45);
			customizeArmyButton.Pressed += OnCustomizeArmy;
			customizeArmyButton.Disabled = !networkManager.IsHost; // Initially disabled for client
			mainContainer.AddChild(customizeArmyButton);

			// Ready button
			readyButton = new Button();
			readyButton.Text = "Ready";
			readyButton.CustomMinimumSize = new Vector2(200, 45);
			readyButton.Pressed += OnReady;
			readyButton.Disabled = true; // Enabled after army customization
			mainContainer.AddChild(readyButton);

			// Start game button (host only)
			if (networkManager.IsHost)
			{
				startGameButton = new Button();
				startGameButton.Text = "Start Game";
				startGameButton.CustomMinimumSize = new Vector2(200, 45);
				startGameButton.Pressed += OnStartGame;
				startGameButton.Disabled = true; // Enabled when both players are ready
				mainContainer.AddChild(startGameButton);
			}

			// Back button
			var backButton = new Button();
			backButton.Text = "Back to Menu";
			backButton.CustomMinimumSize = new Vector2(150, 40);
			backButton.Pressed += () => EmitSignal(SignalName.BackToMenu);
			mainContainer.AddChild(backButton);
		}

		private void ConnectNetworkSignals()
		{
			networkManager.ColorAssignmentReceived += OnColorAssignmentReceived;
			networkManager.ArmyConfigReceived += OnArmyConfigReceived;

			if (networkManager.IsHost)
			{
				// Host sends initial color assignment
				networkManager.SendColorAssignment(localColor);
				// Host listens for client ready
				networkManager.ClientReadyReceived += OnClientReady;
			}
		}

		private void OnColorSelected(PieceColor color)
		{
			localColor = color;

			if (color == PieceColor.White)
			{
				whiteButton.ButtonPressed = true;
				blackButton.ButtonPressed = false;
			}
			else
			{
				whiteButton.ButtonPressed = false;
				blackButton.ButtonPressed = true;
			}

			// Send color assignment to client
			networkManager.SendColorAssignment(localColor);

			// Update status
			statusLabel.Text = $"You are playing as {localColor}";
		}

		private void OnColorAssignmentReceived(int hostColorInt)
		{
			// Client receives color assignment
			var hostColor = (PieceColor)hostColorInt;
			localColor = hostColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
			statusLabel.Text = $"You are playing as {localColor}";

			// Enable army customization for client
			customizeArmyButton.Disabled = false;
		}

		private void OnCustomizeArmy()
		{
			// Load army customization scene
			var armyScene = GD.Load<PackedScene>("res://Scenes/army_customization.tscn");
			if (armyScene != null)
			{
				var armyCustomization = armyScene.Instantiate<ArmyCustomization>();
				armyCustomization.Initialize(localColor, true); // Pass true for LAN mode

				// Connect to completion signal
				armyCustomization.StartCustomGame += () => OnArmyCustomizationComplete(armyCustomization);
				armyCustomization.BackToMenu += () =>
				{
					armyCustomization.QueueFree();
					Show();
				};

				GetParent().AddChild(armyCustomization);
				Hide();
			}
		}

		private void OnArmyCustomizationComplete(ArmyCustomization customization)
		{
			// Get the customized army from ArmyCustomization
			localArmy = customization.GetCustomArmy();
			GD.Print($"Received customized army for {localColor}");

			// Clean up customization screen
			customization.QueueFree();
			Show();

			// Enable ready button
			readyButton.Disabled = false;
			customizeArmyButton.Text = "Re-customize Army";

			statusLabel.Text = "Army customized! Click Ready when done.";
		}

		private void OnReady()
		{
			if (localArmy == null)
			{
				// Use standard army if not customized
				localArmy = new Army(localColor);
			}

			isLocalReady = true;
			readyButton.Disabled = true;
			readyButton.Text = "Ready ✓";

			if (networkManager.IsHost)
			{
				// Host marks self as ready and waits for client
				statusLabel.Text = "Waiting for other player...";
				GD.Print($"Host is ready with {localColor} army");
				CheckIfBothReady();
			}
			else
			{
				// Client sends ready status with army to host
				GD.Print($"Client sending {localColor} army to host");
				networkManager.SendClientReady(localArmy.Serialize());
				statusLabel.Text = "Waiting for host to start game...";
			}
		}

		private void OnArmyConfigReceived(string whiteArmyData, string blackArmyData)
		{
			// Client receives army configurations from host
			GD.Print("Client received army configurations from host - game is starting");
			GD.Print($"Received white army: {whiteArmyData.Substring(0, Math.Min(100, whiteArmyData.Length))}...");
			GD.Print($"Received black army: {blackArmyData.Substring(0, Math.Min(100, blackArmyData.Length))}...");

			var whiteArmy = Army.Deserialize(whiteArmyData, PieceColor.White);
			var blackArmy = Army.Deserialize(blackArmyData, PieceColor.Black);

			GD.Print(
				$"Client deserialized armies - White: {(whiteArmy != null ? "Success" : "Failed")}, Black: {(blackArmy != null ? "Success" : "Failed")}"
			);

			// Store armies in GameConfig for the game to use
			GameConfig.Instance.SetLANArmies(whiteArmy, blackArmy);

			statusLabel.Text = "Starting game...";

			// Emit signal to transition to game scene
			EmitSignal(SignalName.StartLANGame);
		}

		private void CheckIfBothReady()
		{
			if (isLocalReady && isRemoteReady && networkManager.IsHost)
			{
				startGameButton.Disabled = false;
				statusLabel.Text = "Both players ready! Click Start Game.";
				GD.Print($"Both players ready - Local army: {localArmy != null}, Remote army: {remoteArmy != null}");
			}
			else if (networkManager.IsHost)
			{
				GD.Print($"Waiting for players - Local ready: {isLocalReady}, Remote ready: {isRemoteReady}");
			}
		}

		private void OnStartGame()
		{
			if (!networkManager.IsHost)
				return;

			// Ensure both players are ready
			if (!isLocalReady || !isRemoteReady)
			{
				GD.PrintErr("Cannot start game - not all players are ready!");
				return;
			}

			// Prepare armies
			var whiteArmy = localColor == PieceColor.White ? localArmy : remoteArmy;
			var blackArmy = localColor == PieceColor.Black ? localArmy : remoteArmy;

			// Use standard armies if not customized
			whiteArmy ??= new Army(PieceColor.White);
			blackArmy ??= new Army(PieceColor.Black);

			GD.Print($"Host localColor: {localColor}");
			GD.Print($"Host localArmy: {(localArmy != null ? "Custom" : "null")}");
			GD.Print($"Host remoteArmy: {(remoteArmy != null ? "Custom" : "null")}");
			GD.Print(
				$"Starting game with armies - White: {(whiteArmy != null ? "Set" : "null")}, Black: {(blackArmy != null ? "Set" : "null")}"
			);

			// Store armies in GameConfig for the game to use
			GameConfig.Instance.SetLANArmies(whiteArmy, blackArmy);

			// Send army configurations to client
			var whiteArmyData = whiteArmy.Serialize();
			var blackArmyData = blackArmy.Serialize();
			GD.Print($"Sending white army: {whiteArmyData.Substring(0, Math.Min(100, whiteArmyData.Length))}...");
			GD.Print($"Sending black army: {blackArmyData.Substring(0, Math.Min(100, blackArmyData.Length))}...");
			networkManager.SendArmyConfig(whiteArmyData, blackArmyData);

			// Emit signal to transition to game scene
			EmitSignal(SignalName.StartLANGame);
		}

		private void OnClientReady(int clientId, string armyData)
		{
			GD.Print($"Client {clientId} is ready with army data");
			isRemoteReady = true;

			if (!string.IsNullOrEmpty(armyData))
			{
				// Parse remote army
				var remoteColor = localColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
				remoteArmy = Army.Deserialize(armyData, remoteColor);
				GD.Print($"Received {remoteColor} army from client");
			}

			CheckIfBothReady();
		}

		public void OnRemotePlayerReady(string? armyData = null)
		{
			isRemoteReady = true;

			if (!string.IsNullOrEmpty(armyData))
			{
				// Parse remote army
				var remoteColor = localColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
				remoteArmy = Army.Deserialize(armyData, remoteColor);
			}

			CheckIfBothReady();
		}
	}
}

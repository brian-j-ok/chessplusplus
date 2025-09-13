using Godot;
using System.Collections.Generic;
using System.Linq;
using ChessPlusPlus.Core;
using ChessPlusPlus.Pieces;

namespace ChessPlusPlus.UI
{
	public partial class ArmyCustomization : Control
	{
		[Export] public float MinSquareSize { get; set; } = 40.0f;
		[Export] public float MaxSquareSize { get; set; } = 100.0f;
		[Export] public Color LightSquareColor = new Color(0.9f, 0.9f, 0.8f);
		[Export] public Color DarkSquareColor = new Color(0.4f, 0.3f, 0.2f);
		[Export] public Color SelectedSquareColor = new Color(1.0f, 1.0f, 0.3f, 0.7f);

		public float SquareSize { get; private set; } = 64.0f;

		private PieceColor playerColor = PieceColor.White;
		private Army customArmy = null!;
		private Node2D boardContainer = null!;
		private Panel selectionPanel = null!;
		private VBoxContainer classSelectionContainer = null!;
		private Label titleLabel = null!;
		private Button backButton = null!;
		private Button startGameButton = null!;

		private Vector2I? selectedPosition = null;
		private PieceType? selectedPieceType = null;
		private int selectedPositionIndex = -1;
		private ColorRect? highlightRect = null;
		private Dictionary<Vector2I, Node2D> pieceNodes = new();

		[Signal]
		public delegate void BackToMenuEventHandler();

		[Signal]
		public delegate void StartCustomGameEventHandler();

		public override void _Ready()
		{
			PieceRegistry.Initialize();
			CalculateSquareSize();
			SetupUI();
			GetViewport().SizeChanged += OnViewportSizeChanged;
		}

		private void OnViewportSizeChanged()
		{
			CalculateSquareSize();
			RefreshDisplay();
			UpdatePieceScaling();
		}

		private void UpdatePieceScaling()
		{
			// Scale all pieces to match the new square size
			foreach (var piece in pieceNodes.Values)
			{
				if (piece is ChessPlusPlus.Pieces.Piece chessPiece)
				{
					chessPiece.ScaleToFitSquare(SquareSize);
				}
			}
		}

		private void CalculateSquareSize()
		{
			var viewportSize = GetViewport().GetVisibleRect().Size;

			// Reserve space for the selection panel (roughly 40% of width)
			float availableWidth = viewportSize.X * 0.4f;
			float availableHeight = viewportSize.Y * 0.8f; // Account for title and buttons

			// Use smaller dimension, accounting for 2 rows of pieces
			float availableSpace = Mathf.Min(availableWidth / 8.0f, availableHeight / 2.0f);

			SquareSize = Mathf.Clamp(availableSpace, MinSquareSize, MaxSquareSize);
		}

		public void Initialize(PieceColor color)
		{
			playerColor = color;
			customArmy = new Army(color);

			// Only refresh if UI is already set up
			if (titleLabel != null)
			{
				RefreshDisplay();
			}
		}

		private void SetupUI()
		{
			// Background
			var background = new ColorRect();
			background.Color = new Color(0.15f, 0.15f, 0.2f);
			background.AnchorLeft = 0;
			background.AnchorTop = 0;
			background.AnchorRight = 1;
			background.AnchorBottom = 1;
			AddChild(background);

			// Main container with margins
			var mainContainer = new MarginContainer();
			mainContainer.AnchorLeft = 0;
			mainContainer.AnchorTop = 0;
			mainContainer.AnchorRight = 1;
			mainContainer.AnchorBottom = 1;
			mainContainer.AddThemeConstantOverride("margin_left", 20);
			mainContainer.AddThemeConstantOverride("margin_right", 20);
			mainContainer.AddThemeConstantOverride("margin_top", 20);
			mainContainer.AddThemeConstantOverride("margin_bottom", 20);
			AddChild(mainContainer);

			var hContainer = new HBoxContainer();
			hContainer.AddThemeConstantOverride("separation", 20);
			mainContainer.AddChild(hContainer);

			// Left side - Board display
			var leftContainer = new VBoxContainer();
			leftContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			leftContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			leftContainer.AddThemeConstantOverride("separation", 15);
			hContainer.AddChild(leftContainer);

			// Title
			titleLabel = new Label();
			titleLabel.Text = "Customize Your Army";
			titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
			var titleFont = titleLabel.GetThemeFont("font");
			titleLabel.AddThemeFontSizeOverride("font_size", 28);
			leftContainer.AddChild(titleLabel);

			// Instructions
			var instructionLabel = new Label();
			instructionLabel.Text = "Click on a piece to customize it";
			instructionLabel.HorizontalAlignment = HorizontalAlignment.Center;
			instructionLabel.AddThemeFontSizeOverride("font_size", 16);
			instructionLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
			leftContainer.AddChild(instructionLabel);

			// Add some spacing
			var spacer = new Control();
			spacer.CustomMinimumSize = new Vector2(0, 20);
			leftContainer.AddChild(spacer);

			// Board container with proper centering
			var boardPanel = new Panel();
			boardPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			boardPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			var boardStyle = new StyleBoxFlat();
			boardStyle.BgColor = new Color(0.25f, 0.25f, 0.3f);
			boardStyle.BorderWidthLeft = 2;
			boardStyle.BorderWidthRight = 2;
			boardStyle.BorderWidthTop = 2;
			boardStyle.BorderWidthBottom = 2;
			boardStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
			boardStyle.CornerRadiusTopLeft = 8;
			boardStyle.CornerRadiusTopRight = 8;
			boardStyle.CornerRadiusBottomLeft = 8;
			boardStyle.CornerRadiusBottomRight = 8;
			boardPanel.AddThemeStyleboxOverride("panel", boardStyle);
			leftContainer.AddChild(boardPanel);

			// Center container for the board
			var centerContainer = new CenterContainer();
			centerContainer.AnchorLeft = 0;
			centerContainer.AnchorTop = 0;
			centerContainer.AnchorRight = 1;
			centerContainer.AnchorBottom = 1;
			boardPanel.AddChild(centerContainer);

			// Board control that properly handles the Node2D
			var boardControl = new Control();
			boardControl.CustomMinimumSize = new Vector2(8 * SquareSize, 2 * SquareSize);
			centerContainer.AddChild(boardControl);

			// Position the board at origin within the centered control
			boardContainer = new Node2D();
			boardContainer.Name = "BoardContainer";
			boardContainer.Position = Vector2.Zero;
			boardControl.AddChild(boardContainer);

			// Buttons with better styling
			var buttonContainer = new HBoxContainer();
			buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
			buttonContainer.AddThemeConstantOverride("separation", 15);
			leftContainer.AddChild(buttonContainer);

			backButton = new Button();
			backButton.Text = "Back to Menu";
			backButton.CustomMinimumSize = new Vector2(120, 40);
			backButton.Pressed += () => EmitSignal(SignalName.BackToMenu);
			buttonContainer.AddChild(backButton);

			startGameButton = new Button();
			startGameButton.Text = "Start Game";
			startGameButton.CustomMinimumSize = new Vector2(120, 40);
			startGameButton.Pressed += OnStartGame;
			buttonContainer.AddChild(startGameButton);

			// Right side - Selection panel with better styling
			selectionPanel = new Panel();
			selectionPanel.CustomMinimumSize = new Vector2(320, 0);
			selectionPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			selectionPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			selectionPanel.Visible = false;
			var selectionStyle = new StyleBoxFlat();
			selectionStyle.BgColor = new Color(0.2f, 0.2f, 0.25f);
			selectionStyle.BorderWidthLeft = 2;
			selectionStyle.BorderWidthRight = 2;
			selectionStyle.BorderWidthTop = 2;
			selectionStyle.BorderWidthBottom = 2;
			selectionStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
			selectionStyle.CornerRadiusTopLeft = 8;
			selectionStyle.CornerRadiusTopRight = 8;
			selectionStyle.CornerRadiusBottomLeft = 8;
			selectionStyle.CornerRadiusBottomRight = 8;
			selectionPanel.AddThemeStyleboxOverride("panel", selectionStyle);
			hContainer.AddChild(selectionPanel);

			var selectionMargin = new MarginContainer();
			selectionMargin.AnchorLeft = 0;
			selectionMargin.AnchorTop = 0;
			selectionMargin.AnchorRight = 1;
			selectionMargin.AnchorBottom = 1;
			selectionMargin.AddThemeConstantOverride("margin_left", 8);
			selectionMargin.AddThemeConstantOverride("margin_right", 8);
			selectionMargin.AddThemeConstantOverride("margin_top", 15);
			selectionMargin.AddThemeConstantOverride("margin_bottom", 15);
			selectionPanel.AddChild(selectionMargin);

			var selectionContainer = new VBoxContainer();
			selectionContainer.AddThemeConstantOverride("separation", 15);
			selectionMargin.AddChild(selectionContainer);

			var selectionTitle = new Label();
			selectionTitle.Text = "Select Piece Class";
			selectionTitle.HorizontalAlignment = HorizontalAlignment.Center;
			selectionTitle.AddThemeFontSizeOverride("font_size", 20);
			selectionContainer.AddChild(selectionTitle);

			// Scrollable area for class options
			var scrollContainer = new ScrollContainer();
			scrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			selectionContainer.AddChild(scrollContainer);

			classSelectionContainer = new VBoxContainer();
			classSelectionContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			classSelectionContainer.AddThemeConstantOverride("separation", 8);
			scrollContainer.AddChild(classSelectionContainer);

			SetupBoard();

			// If Initialize was called before _Ready, refresh now
			if (customArmy != null)
			{
				RefreshDisplay();
			}
		}

		private void SetupBoard()
		{
			// Draw squares for player's side only
			var positions = GetPlayerSidePositions();

			foreach (var pos in positions)
			{
				// Adjust position for the custom layout (show only player's rows)
				var displayY = pos.Y == (playerColor == PieceColor.White ? 6 : 1) ? 0 : 1; // Pawns on first row, pieces on second row
				var displayPos = new Vector2(pos.X * SquareSize, displayY * SquareSize);

				// Draw square with better styling
				var square = new Panel();
				square.Size = new Vector2(SquareSize, SquareSize);
				square.Position = displayPos;
				square.Name = $"Square_{pos.X}_{pos.Y}";

				// Create styled background for square
				var squareStyle = new StyleBoxFlat();
				squareStyle.BgColor = (pos.X + displayY) % 2 == 0 ? LightSquareColor : DarkSquareColor;
				squareStyle.BorderWidthLeft = 1;
				squareStyle.BorderWidthRight = 1;
				squareStyle.BorderWidthTop = 1;
				squareStyle.BorderWidthBottom = 1;
				squareStyle.BorderColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
				square.AddThemeStyleboxOverride("panel", squareStyle);

				boardContainer.AddChild(square);

				// Make square clickable with hover effect
				var button = new Button();
				button.Size = new Vector2(SquareSize, SquareSize);
				button.Position = Vector2.Zero;
				button.Flat = true;
				button.Name = $"Button_{pos.X}_{pos.Y}";

				// Add subtle hover effect
				var hoverStyle = new StyleBoxFlat();
				hoverStyle.BgColor = new Color(1.0f, 1.0f, 0.5f, 0.3f);
				button.AddThemeStyleboxOverride("hover", hoverStyle);

				var normalStyle = new StyleBoxFlat();
				normalStyle.BgColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
				button.AddThemeStyleboxOverride("normal", normalStyle);

				button.Pressed += () => OnSquareClicked(pos);
				square.AddChild(button);
			}
		}

		private List<Vector2I> GetPlayerSidePositions()
		{
			var positions = new List<Vector2I>();

			if (playerColor == PieceColor.White)
			{
				// White pieces are on bottom rows (6, 7)
				for (int x = 0; x < 8; x++)
				{
					positions.Add(new Vector2I(x, 6)); // Pawns
					positions.Add(new Vector2I(x, 7)); // Back pieces
				}
			}
			else
			{
				// Black pieces are on top rows (0, 1)
				for (int x = 0; x < 8; x++)
				{
					positions.Add(new Vector2I(x, 1)); // Pawns
					positions.Add(new Vector2I(x, 0)); // Back pieces
				}
			}

			return positions;
		}

		private void OnSquareClicked(Vector2I position)
		{
			var pieceType = GetPieceTypeAtPosition(position);
			var positionIndex = GetPositionIndex(position);

			if (pieceType != null)
			{
				selectedPosition = position;
				selectedPieceType = pieceType;
				selectedPositionIndex = positionIndex;

				ShowHighlight(position);
				ShowClassSelection(pieceType.Value);
			}
		}

		private PieceType? GetPieceTypeAtPosition(Vector2I position)
		{
			bool isPawnRow = (playerColor == PieceColor.White && position.Y == 6) ||
							 (playerColor == PieceColor.Black && position.Y == 1);

			if (isPawnRow)
			{
				return PieceType.Pawn;
			}

			// Back row pieces
			return position.X switch
			{
				0 or 7 => PieceType.Rook,
				1 or 6 => PieceType.Knight,
				2 or 5 => PieceType.Bishop,
				3 => PieceType.Queen,
				4 => PieceType.King,
				_ => null
			};
		}

		private int GetPositionIndex(Vector2I position)
		{
			bool isPawnRow = (playerColor == PieceColor.White && position.Y == 6) ||
							 (playerColor == PieceColor.Black && position.Y == 1);

			if (isPawnRow)
			{
				return position.X; // Pawn index 0-7
			}

			// Back row pieces - map to position index
			return position.X switch
			{
				0 => 0, // Left rook
				1 => 1, // Left knight
				2 => 2, // Left bishop
				3 => 3, // Queen
				4 => 4, // King
				5 => 5, // Right bishop
				6 => 6, // Right knight
				7 => 7, // Right rook
				_ => -1
			};
		}

		private void ShowHighlight(Vector2I position)
		{
			// Remove existing highlight safely
			if (highlightRect != null && IsInstanceValid(highlightRect))
			{
				highlightRect.QueueFree();
			}
			highlightRect = null;

			// Create new highlight
			highlightRect = new ColorRect();
			highlightRect.Size = new Vector2(SquareSize, SquareSize);

			// Use the same display position as squares and pieces
			var displayY = position.Y == (playerColor == PieceColor.White ? 6 : 1) ? 0 : 1;
			var displayPos = new Vector2(position.X * SquareSize, displayY * SquareSize);
			highlightRect.Position = displayPos;

			highlightRect.Color = SelectedSquareColor;
			highlightRect.Name = "Highlight";
			boardContainer.AddChild(highlightRect);
		}

		private void ShowClassSelection(PieceType pieceType)
		{
			// Clear existing selection UI
			foreach (Node child in classSelectionContainer.GetChildren())
			{
				child.QueueFree();
			}

			var availableClasses = PieceRegistry.GetAvailableClasses(pieceType);

			foreach (var classInfo in availableClasses)
			{
				// Create a styled container for each option
				var optionPanel = new Panel();
				optionPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				optionPanel.CustomMinimumSize = new Vector2(0, 80); // Give more vertical space
				optionPanel.ClipContents = true; // Ensure content is clipped to panel bounds
				var optionStyle = new StyleBoxFlat();
				optionStyle.BgColor = new Color(0.3f, 0.3f, 0.35f);
				optionStyle.BorderWidthLeft = 1;
				optionStyle.BorderWidthRight = 1;
				optionStyle.BorderWidthTop = 1;
				optionStyle.BorderWidthBottom = 1;
				optionStyle.BorderColor = new Color(0.5f, 0.5f, 0.6f);
				optionStyle.CornerRadiusTopLeft = 6;
				optionStyle.CornerRadiusTopRight = 6;
				optionStyle.CornerRadiusBottomLeft = 6;
				optionStyle.CornerRadiusBottomRight = 6;
				optionPanel.AddThemeStyleboxOverride("panel", optionStyle);

				var optionMargin = new MarginContainer();
				optionMargin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
				optionMargin.AddThemeConstantOverride("margin_left", 8);
				optionMargin.AddThemeConstantOverride("margin_right", 8);
				optionMargin.AddThemeConstantOverride("margin_top", 12);
				optionMargin.AddThemeConstantOverride("margin_bottom", 12);
				optionPanel.AddChild(optionMargin);

				// Use HBoxContainer to place button and text side by side for better space usage
				var mainContainer = new VBoxContainer();
				mainContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				mainContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
				mainContainer.AddThemeConstantOverride("separation", 8);
				optionMargin.AddChild(mainContainer);

				// Button container
				var buttonContainer = new HBoxContainer();
				buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				mainContainer.AddChild(buttonContainer);

				var classButton = new Button();
				classButton.Text = classInfo.DisplayName;
				classButton.CustomMinimumSize = new Vector2(80, 40);
				classButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

				// Style the button based on selection
				var currentClass = customArmy.GetPieceClass(pieceType, selectedPositionIndex);
				if (currentClass == classInfo.ClassName)
				{
					var selectedStyle = new StyleBoxFlat();
					selectedStyle.BgColor = new Color(0.2f, 0.6f, 0.9f);
					selectedStyle.BorderWidthLeft = 2;
					selectedStyle.BorderWidthRight = 2;
					selectedStyle.BorderWidthTop = 2;
					selectedStyle.BorderWidthBottom = 2;
					selectedStyle.BorderColor = new Color(0.3f, 0.7f, 1.0f);
					selectedStyle.CornerRadiusTopLeft = 4;
					selectedStyle.CornerRadiusTopRight = 4;
					selectedStyle.CornerRadiusBottomLeft = 4;
					selectedStyle.CornerRadiusBottomRight = 4;
					classButton.AddThemeStyleboxOverride("normal", selectedStyle);
					classButton.AddThemeStyleboxOverride("hover", selectedStyle);
					classButton.AddThemeStyleboxOverride("pressed", selectedStyle);
				}
				else
				{
					var normalStyle = new StyleBoxFlat();
					normalStyle.BgColor = new Color(0.4f, 0.4f, 0.45f);
					normalStyle.CornerRadiusTopLeft = 4;
					normalStyle.CornerRadiusTopRight = 4;
					normalStyle.CornerRadiusBottomLeft = 4;
					normalStyle.CornerRadiusBottomRight = 4;
					classButton.AddThemeStyleboxOverride("normal", normalStyle);

					var hoverStyle = new StyleBoxFlat();
					hoverStyle.BgColor = new Color(0.5f, 0.5f, 0.55f);
					hoverStyle.CornerRadiusTopLeft = 4;
					hoverStyle.CornerRadiusTopRight = 4;
					hoverStyle.CornerRadiusBottomLeft = 4;
					hoverStyle.CornerRadiusBottomRight = 4;
					classButton.AddThemeStyleboxOverride("hover", hoverStyle);
				}

				classButton.Pressed += () => OnClassSelected(classInfo.ClassName);
				buttonContainer.AddChild(classButton);

				// Add description with proper text containment
				var descLabel = new RichTextLabel();
				descLabel.Text = classInfo.Description;
				descLabel.FitContent = false;
				descLabel.ScrollActive = false;
				descLabel.BbcodeEnabled = false;
				descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
				descLabel.AddThemeFontSizeOverride("normal_font_size", 13);
				descLabel.Modulate = new Color(0.9f, 0.9f, 0.9f);
				descLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				descLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
				descLabel.CustomMinimumSize = new Vector2(0, 20);
				// Add more line height for better readability
				descLabel.AddThemeConstantOverride("line_separation", 2);

				mainContainer.AddChild(descLabel);

				classSelectionContainer.AddChild(optionPanel);
			}

			selectionPanel.Visible = true;
		}

		private void OnClassSelected(string className)
		{
			if (selectedPieceType != null && selectedPositionIndex >= 0)
			{
				customArmy.SetPieceClass(selectedPieceType.Value, selectedPositionIndex, className);
				RefreshPieceDisplay();

				// Hide selection panel and clear highlight safely
				selectionPanel.Visible = false;
				if (highlightRect != null && IsInstanceValid(highlightRect))
				{
					highlightRect.QueueFree();
					highlightRect = null;
				}
				selectedPosition = null;
			}
		}

		private void RefreshDisplay()
		{
			titleLabel.Text = $"Customize Your Army - Playing as {playerColor}";
			RefreshPieceDisplay();
		}

		private void RefreshPieceDisplay()
		{
			// Clear existing piece displays safely
			foreach (var node in pieceNodes.Values)
			{
				if (IsInstanceValid(node))
				{
					node.QueueFree();
				}
			}
			pieceNodes.Clear();

			// Add piece displays
			var positions = GetPlayerSidePositions();
			foreach (var pos in positions)
			{
				var pieceType = GetPieceTypeAtPosition(pos);
				var positionIndex = GetPositionIndex(pos);

				if (pieceType != null)
				{
					var piece = customArmy.CreatePiece(pieceType.Value, positionIndex);
					piece.BoardPosition = pos;

					// Position piece at top-left of square, ScaleToFitSquare will center the sprite
					var displayY = pos.Y == (playerColor == PieceColor.White ? 6 : 1) ? 0 : 1;
					var squareTopLeft = new Vector2(pos.X * SquareSize, displayY * SquareSize);
					piece.Position = squareTopLeft;

					pieceNodes[pos] = piece;
					boardContainer.AddChild(piece);

					// Scale piece to fit the current square size
					if (piece is ChessPlusPlus.Pieces.Piece chessPiece)
					{
						chessPiece.ScaleToFitSquare(SquareSize);
					}
				}
			}
		}

		private void OnStartGame()
		{
			// Store the custom army in GameConfig for the game scene to use
			GameConfig.Instance.SetCustomArmy(customArmy);

			// Load and start the game scene directly instead of using signals
			var gameScene = GD.Load<PackedScene>("res://Scenes/game.tscn");
			if (gameScene != null)
			{
				// Get tree reference before removing from tree
				var tree = GetTree();

				// Remove this scene from the tree first to prevent overlay
				GetParent()?.RemoveChild(this);
				QueueFree();

				tree.ChangeSceneToPacked(gameScene);
			}
			else
			{
				GD.PrintErr("Failed to load game scene!");
			}
		}
	}
}

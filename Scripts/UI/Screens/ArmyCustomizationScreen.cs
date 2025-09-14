using System.Collections.Generic;
using System.Linq;
using ChessPlusPlus.Core;
using ChessPlusPlus.Pieces;
using ChessPlusPlus.UI.Builders;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;
using StylePresets = ChessPlusPlus.UI.Styles.StylePresets;
using UIBuilder = ChessPlusPlus.UI.Builders.UI;

namespace ChessPlusPlus.UI.Screens
{
	public partial class ArmyCustomizationScreen : ScreenBase
	{
		[Export]
		public float MinSquareSize { get; set; } = 40.0f;

		[Export]
		public float MaxSquareSize { get; set; } = 100.0f;

		[Export]
		public Color LightSquareColor = new Color(0.9f, 0.9f, 0.8f);

		[Export]
		public Color DarkSquareColor = new Color(0.4f, 0.3f, 0.2f);

		[Export]
		public Color SelectedSquareColor = new Color(1.0f, 1.0f, 0.3f, 0.7f);

		public float SquareSize { get; private set; } = 64.0f;

		private PieceColor playerColor = PieceColor.White;
		private Army customArmy = null!;
		private Node2D boardContainer = null!;
		private Control selectionPanel = null!;
		private VBoxContainer classSelectionContainer = null!;
		private Label selectedPositionLabel = null!;
		private Label selectedPieceLabel = null!;

		private Vector2I? selectedPosition = null;
		private PieceType selectedPieceType = PieceType.Pawn;
		private int selectedPositionIndex = -1;
		private ColorRect? highlightRect = null;
		private Dictionary<Vector2I, Node2D> pieceNodes = new();

		// Map board positions to piece types for standard setup
		private Dictionary<int, PieceType> backRowSetup =
			new()
			{
				{ 0, PieceType.Rook },
				{ 1, PieceType.Knight },
				{ 2, PieceType.Bishop },
				{ 3, PieceType.Queen },
				{ 4, PieceType.King },
				{ 5, PieceType.Bishop },
				{ 6, PieceType.Knight },
				{ 7, PieceType.Rook },
			};

		[Signal]
		public delegate void BackToMenuEventHandler();

		[Signal]
		public delegate void StartCustomGameEventHandler();

		public override void _Ready()
		{
			base._Ready();
			PieceRegistry.Initialize();
			CalculateSquareSize();
			GetViewport().SizeChanged += OnViewportSizeChanged;
		}

		protected override Control BuildUI()
		{
			// Main background
			var background = UIBuilder.ColorRect(ColorPalette.BackgroundDark).FullRect().Build();

			// Main container with padding
			var mainContainer = UIBuilder
				.HBox()
				.ExpandFill()
				.Spacing(StylePresets.Spacing.Large)
				.Children(BuildLeftPanel(), BuildSelectionPanel())
				.Build();

			// Apply padding manually since we're working with HBox
			var marginBuilder = UIBuilder.Margins();
			marginBuilder.ExpandFill();
			marginBuilder.Margins(StylePresets.Spacing.Large);
			marginBuilder.Child(mainContainer);
			marginBuilder.AddTo(background);

			return background;
		}

		private Control BuildLeftPanel()
		{
			// Create board control container
			var boardControl = UIBuilder.Control().Size((int)(8 * SquareSize), (int)(2 * SquareSize)).Build();

			// Add Node2D for board pieces
			boardContainer = new Node2D { Name = "BoardContainer", Position = Vector2.Zero };
			boardControl.AddChild(boardContainer);
			SetupBoard();

			return UIBuilder
				.VBox()
				.ExpandFill()
				.Spacing(StylePresets.Spacing.Medium)
				.Children(
					UIBuilder.Label("Customize Your Army").Title().Build(),
					UIBuilder.Label("Click on a piece to customize it").Subtitle().Muted().Build(),
					UIBuilder.Spacer(0, StylePresets.Spacing.Large),
					UIBuilder
						.Panel()
						.ExpandFill()
						.Style(ColorPalette.BackgroundMedium, ColorPalette.BorderLight, 2, 8)
						.Child(UIBuilder.CenterContainer().ExpandFill().Child(boardControl).Build())
						.Build(),
					BuildActionButtons()
				)
				.Build();
		}

		private Control BuildSelectionPanel()
		{
			selectedPositionLabel = UIBuilder.Label("").Subtitle().Build();
			selectedPieceLabel = UIBuilder.Label("").Muted().Build();

			classSelectionContainer = UIBuilder.VBox().ExpandFill().Spacing(StylePresets.Spacing.Small).Build();

			// Create the inner content with padding
			var innerContent = UIBuilder
				.VBox()
				.ExpandFill()
				.Spacing(StylePresets.Spacing.Medium)
				.Children(
					UIBuilder.Label("Select Piece Class").Heading().Centered().Build(),
					selectedPositionLabel,
					selectedPieceLabel,
					UIBuilder.HSeparator(),
					UIBuilder.ScrollContainer().ExpandFill().Child(classSelectionContainer).Build()
				)
				.Build();

			// Wrap in margin container for padding
			var marginBuilder2 = UIBuilder.Margins();
			marginBuilder2.ExpandFill();
			marginBuilder2.Margins(StylePresets.Spacing.Medium);
			marginBuilder2.Child(innerContent);
			var paddedContent = marginBuilder2.Build();

			selectionPanel = UIBuilder
				.Panel()
				.MinSize(320, 0)
				.ExpandFill()
				.Visible(false)
				.Style(ColorPalette.BackgroundMedium, ColorPalette.BorderLight, 2, 8)
				.Child(paddedContent)
				.Build();

			return selectionPanel;
		}

		private Control BuildActionButtons()
		{
			return UIBuilder
				.HBox()
				.CenterAlign()
				.Spacing(StylePresets.Spacing.Large)
				.Children(
					UIBuilder
						.Button("Back to Menu")
						.Size(StylePresets.ButtonSizes.Large)
						.OnPress(() => EmitSignal(SignalName.BackToMenu))
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

		public void Initialize(PieceColor color)
		{
			playerColor = color;
			customArmy = new Army(color);

			// Refresh display if UI is ready
			if (boardContainer != null)
			{
				RefreshDisplay();
			}
		}

		private void OnViewportSizeChanged()
		{
			CalculateSquareSize();
			RefreshDisplay();
			UpdatePieceScaling();
		}

		private void UpdatePieceScaling()
		{
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
			float availableWidth = viewportSize.X * 0.4f;
			float availableHeight = viewportSize.Y * 0.8f;
			float availableSpace = Mathf.Min(availableWidth / 8.0f, availableHeight / 2.0f);
			SquareSize = Mathf.Clamp(availableSpace, MinSquareSize, MaxSquareSize);
		}

		private void SetupBoard()
		{
			var positions = GetPlayerSidePositions();

			foreach (var pos in positions)
			{
				var displayY = pos.Y == (playerColor == PieceColor.White ? 6 : 1) ? 0 : 1;
				var displayPos = new Vector2(pos.X * SquareSize, displayY * SquareSize);

				// Create square
				var square = new Panel
				{
					Size = new Vector2(SquareSize, SquareSize),
					Position = displayPos,
					Name = $"Square_{pos.X}_{pos.Y}",
				};

				var squareStyle = new StyleBoxFlat
				{
					BgColor = (pos.X + displayY) % 2 == 0 ? LightSquareColor : DarkSquareColor,
					BorderWidthLeft = 1,
					BorderWidthRight = 1,
					BorderWidthTop = 1,
					BorderWidthBottom = 1,
					BorderColor = new Color(0.3f, 0.3f, 0.3f, 0.5f),
				};
				square.AddThemeStyleboxOverride("panel", squareStyle);
				boardContainer.AddChild(square);

				// Create button for interaction
				var button = new Button
				{
					Size = new Vector2(SquareSize, SquareSize),
					Position = displayPos,
					Flat = true,
					MouseDefaultCursorShape = Control.CursorShape.PointingHand,
				};

				int index = positions.IndexOf(pos);
				button.Pressed += () => OnSquareClicked(pos, index);
				boardContainer.AddChild(button);
			}
		}

		private void RefreshDisplay()
		{
			if (boardContainer == null)
				return;

			// Clear existing pieces
			foreach (var pieceNode in pieceNodes.Values)
			{
				pieceNode.QueueFree();
			}
			pieceNodes.Clear();

			// Clear highlight
			highlightRect?.QueueFree();
			highlightRect = null;

			// Add pieces from army based on position
			var positions = GetPlayerSidePositions();
			foreach (var pos in positions)
			{
				var pieceType = GetPieceTypeForPosition(pos);
				var piece = customArmy.CreatePiece(pieceType, pos.X);
				if (piece != null)
				{
					AddPieceToBoard(pos, piece);
				}
			}
		}

		private PieceType GetPieceTypeForPosition(Vector2I position)
		{
			// Check if it's a pawn row
			if (position.Y == (playerColor == PieceColor.White ? 6 : 1))
			{
				return PieceType.Pawn;
			}
			// Otherwise it's a back row piece
			else if (backRowSetup.TryGetValue(position.X, out var pieceType))
			{
				return pieceType;
			}
			return PieceType.Pawn;
		}

		private void AddPieceToBoard(Vector2I position, ChessPlusPlus.Pieces.Piece piece)
		{
			var displayY = position.Y == (playerColor == PieceColor.White ? 6 : 1) ? 0 : 1;
			var displayPos = new Vector2(
				position.X * SquareSize + SquareSize / 2,
				displayY * SquareSize + SquareSize / 2
			);

			piece.Position = displayPos;
			piece.BoardPosition = position;
			piece.ScaleToFitSquare(SquareSize);
			boardContainer.AddChild(piece);
			pieceNodes[position] = piece;
		}

		private void OnSquareClicked(Vector2I position, int index)
		{
			selectedPosition = position;
			selectedPositionIndex = index;
			selectedPieceType = GetPieceTypeForPosition(position);

			// Update highlight
			highlightRect?.QueueFree();
			highlightRect = new ColorRect { Color = SelectedSquareColor, Size = new Vector2(SquareSize, SquareSize) };

			var displayY = position.Y == (playerColor == PieceColor.White ? 6 : 1) ? 0 : 1;
			highlightRect.Position = new Vector2(position.X * SquareSize, displayY * SquareSize);
			boardContainer.AddChild(highlightRect);

			// Show selection panel
			ShowSelectionPanel(position);
		}

		private void ShowSelectionPanel(Vector2I position)
		{
			selectionPanel.Visible = true;

			// Update labels
			string positionName = GetPositionName(position);
			selectedPositionLabel.Text = $"Position: {positionName}";

			var currentPieceType = GetPieceTypeForPosition(position);
			var currentClass = customArmy.GetPieceClass(currentPieceType, position.X);
			selectedPieceLabel.Text = $"Current: {currentPieceType} ({currentClass})";

			// Clear and populate class options
			foreach (Node child in classSelectionContainer.GetChildren())
			{
				child.QueueFree();
			}

			// Get available classes for this piece type
			var availableClasses = GetAvailableClassesForPieceType(currentPieceType);
			foreach (var className in availableClasses)
			{
				var button = UIBuilder
					.Button(className)
					.Size(StylePresets.ButtonSizes.Wide)
					.OnPress(() => OnClassSelected(className))
					.Build();

				// Highlight current selection
				if (currentClass == className)
				{
					button.Modulate = ColorPalette.TextAccent;
				}

				classSelectionContainer.AddChild(button);
			}
		}

		private void OnClassSelected(string className)
		{
			if (!selectedPosition.HasValue || selectedPositionIndex < 0)
				return;

			var position = selectedPosition.Value;
			var pieceType = GetPieceTypeForPosition(position);

			// Update the army's piece class
			customArmy.SetPieceClass(pieceType, position.X, className);

			// Refresh the display
			RefreshDisplay();

			// Re-highlight the selected square
			if (highlightRect != null)
			{
				var displayY = position.Y == (playerColor == PieceColor.White ? 6 : 1) ? 0 : 1;
				highlightRect = new ColorRect
				{
					Color = SelectedSquareColor,
					Size = new Vector2(SquareSize, SquareSize),
					Position = new Vector2(position.X * SquareSize, displayY * SquareSize),
				};
				boardContainer.AddChild(highlightRect);
			}

			// Update the selection panel
			ShowSelectionPanel(position);
		}

		private void OnStartGame()
		{
			// The customArmy is already configured and will be used when creating the board
			// You might want to pass this to the game scene through a different mechanism
			EmitSignal(SignalName.StartCustomGame);
		}

		private List<Vector2I> GetPlayerSidePositions()
		{
			var positions = new List<Vector2I>();

			if (playerColor == PieceColor.White)
			{
				// White pieces - back row then pawns
				for (int x = 0; x < 8; x++)
				{
					positions.Add(new Vector2I(x, 7));
				}
				for (int x = 0; x < 8; x++)
				{
					positions.Add(new Vector2I(x, 6));
				}
			}
			else
			{
				// Black pieces - back row then pawns
				for (int x = 0; x < 8; x++)
				{
					positions.Add(new Vector2I(x, 0));
				}
				for (int x = 0; x < 8; x++)
				{
					positions.Add(new Vector2I(x, 1));
				}
			}

			return positions;
		}

		private string GetPositionName(Vector2I position)
		{
			char file = (char)('a' + position.X);
			int rank = 8 - position.Y;
			return $"{file}{rank}";
		}

		private List<string> GetAvailableClassesForPieceType(PieceType pieceType)
		{
			// Get available classes from PieceRegistry
			var classes = PieceRegistry.GetAvailableClasses(pieceType);

			// If no classes found, return standard
			if (classes.Count == 0)
			{
				return new List<string> { "Standard" };
			}

			return classes.Select(c => c.ClassName).ToList();
		}

		protected override void OnExit()
		{
			// Cleanup if needed
		}
	}
}

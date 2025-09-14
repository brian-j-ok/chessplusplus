using System.Collections.Generic;
using ChessPlusPlus.Core;
using Godot;

namespace ChessPlusPlus.UI
{
	public partial class BoardVisual : Node2D
	{
		[Export]
		public Color LightSquareColor = new Color(0.9f, 0.9f, 0.8f);

		[Export]
		public Color DarkSquareColor = new Color(0.4f, 0.3f, 0.2f);

		[Export]
		public Color SelectedSquareColor = new Color(1.0f, 1.0f, 0.3f, 0.7f);

		[Export]
		public Color ValidMoveColor = new Color(0.3f, 1.0f, 0.3f, 0.5f);

		[Export]
		public Color CaptureColor = new Color(1.0f, 0.3f, 0.3f, 0.7f);

		[Export]
		public float MinSquareSize = 40.0f;

		[Export]
		public float MaxSquareSize = 120.0f;

		[Export]
		public float BoardPadding = 40.0f;

		public float SquareSize { get; private set; } = 64.0f;

		private ColorRect[,] squares = new ColorRect[8, 8];
		private List<ColorRect> highlights = new List<ColorRect>();

		public override void _Ready()
		{
			AddToGroup("board_visual");
			CalculateSquareSize();
			DrawBoard();
			GetViewport().SizeChanged += OnViewportSizeChanged;
		}

		private void OnViewportSizeChanged()
		{
			CalculateSquareSize();
			RefreshBoardOrientation();
			UpdatePieceScaling();
		}

		private void UpdatePieceScaling()
		{
			// Update all piece sprites to match new square size
			GetTree().CallGroup("chess_pieces", "ScaleToFitSquare");

			// Update piece positions to match new square positions
			var board = GetParent<ChessPlusPlus.Core.Board>();
			board?.UpdatePiecePositions();
		}

		private void CalculateSquareSize()
		{
			var viewportSize = GetViewport().GetVisibleRect().Size;

			// Calculate available space for the board (accounting for UI and padding)
			float availableWidth = viewportSize.X - BoardPadding * 2;
			float availableHeight = viewportSize.Y - BoardPadding * 2;

			// Use the smaller dimension to ensure the board fits
			float availableSpace = Mathf.Min(availableWidth, availableHeight);

			// Calculate square size (board is 8x8)
			float calculatedSize = availableSpace / 8.0f;

			// Clamp to min/max values
			SquareSize = Mathf.Clamp(calculatedSize, MinSquareSize, MaxSquareSize);

			// Center the board in the viewport
			float boardSize = SquareSize * 8;
			Position = new Vector2((viewportSize.X - boardSize) * 0.5f, (viewportSize.Y - boardSize) * 0.5f);
		}

		public void RefreshBoardOrientation()
		{
			// Clear and redraw the board with the new orientation
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					if (squares[x, y] != null)
					{
						squares[x, y].QueueFree();
					}
				}
			}
			DrawBoard();
		}

		private void DrawBoard()
		{
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var square = new ColorRect();
					square.Name = $"Square_{x}_{y}";
					square.Size = new Vector2(SquareSize, SquareSize);

					// Apply board orientation
					var displayPos = GameConfig.Instance.ShouldFlipBoard()
						? FlipPosition(new Vector2I(x, y))
						: new Vector2I(x, y);
					square.Position = new Vector2(displayPos.X * SquareSize, displayPos.Y * SquareSize);

					// Calculate the square color based on the original position, not display position
					square.Color = (x + y) % 2 == 0 ? LightSquareColor : DarkSquareColor;
					squares[x, y] = square;
					AddChild(square);
				}
			}
		}

		public void HighlightSquare(Vector2I position, Color color)
		{
			if (position.X < 0 || position.X >= 8 || position.Y < 0 || position.Y >= 8)
				return;

			var highlight = new ColorRect();
			highlight.Size = new Vector2(SquareSize, SquareSize);
			var displayPos = GameConfig.Instance.ShouldFlipBoard() ? FlipPosition(position) : position;
			highlight.Position = new Vector2(displayPos.X * SquareSize, displayPos.Y * SquareSize);
			highlight.Color = color;
			highlights.Add(highlight);
			AddChild(highlight);
		}

		public void HighlightSelectedSquare(Vector2I position)
		{
			HighlightSquare(position, SelectedSquareColor);
		}

		public void HighlightValidMoves(List<Vector2I> positions, ChessPlusPlus.Core.Board board)
		{
			foreach (var pos in positions)
			{
				var piece = board.GetPieceAt(pos);
				Color highlightColor = piece != null ? CaptureColor : ValidMoveColor;
				HighlightSquare(pos, highlightColor);
			}
		}

		public void ClearHighlights()
		{
			foreach (var highlight in highlights)
			{
				highlight.QueueFree();
			}
			highlights.Clear();
		}

		public void ResetSquareColors()
		{
			ClearHighlights();
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					squares[x, y].Color = (x + y) % 2 == 0 ? LightSquareColor : DarkSquareColor;
				}
			}
		}

		private Vector2I FlipPosition(Vector2I position)
		{
			return new Vector2I(position.X, 7 - position.Y);
		}
	}
}

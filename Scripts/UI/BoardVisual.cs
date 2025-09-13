using Godot;
using System.Collections.Generic;
using ChessPlusPlus.Core;

namespace ChessPlusPlus.UI
{
	public partial class BoardVisual : Node2D
	{
		[Export] public Color LightSquareColor = new Color(0.9f, 0.9f, 0.8f);
		[Export] public Color DarkSquareColor = new Color(0.4f, 0.3f, 0.2f);
		[Export] public Color SelectedSquareColor = new Color(1.0f, 1.0f, 0.3f, 0.7f);
		[Export] public Color ValidMoveColor = new Color(0.3f, 1.0f, 0.3f, 0.5f);
		[Export] public Color CaptureColor = new Color(1.0f, 0.3f, 0.3f, 0.7f);
		[Export] public float SquareSize = 64.0f;

		private ColorRect[,] squares = new ColorRect[8, 8];
		private List<ColorRect> highlights = new List<ColorRect>();

		public override void _Ready()
		{
			DrawBoard();
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
					var displayPos = GameConfig.Instance.ShouldFlipBoard() ? FlipPosition(new Vector2I(x, y)) : new Vector2I(x, y);
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

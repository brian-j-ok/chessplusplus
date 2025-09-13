using Godot;
using System.Collections.Generic;

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

		private void DrawBoard()
		{
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var square = new ColorRect();
					square.Name = $"Square_{x}_{y}";
					square.Size = new Vector2(SquareSize, SquareSize);
					square.Position = new Vector2(x * SquareSize, y * SquareSize);
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
			highlight.Position = new Vector2(position.X * SquareSize, position.Y * SquareSize);
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
	}
}

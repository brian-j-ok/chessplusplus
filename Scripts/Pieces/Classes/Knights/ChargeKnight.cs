using System.Collections.Generic;
using ChessPlusPlus.Core;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class ChargeKnight : Knight
	{
		// All possible three-cell line movements (can hop over 2 pieces)
		private Vector2I[] chargeDirections = new Vector2I[]
		{
			// Horizontal and vertical charges
			new Vector2I(3, 0),
			new Vector2I(-3, 0), // Left/Right
			new Vector2I(0, 3),
			new Vector2I(0, -3), // Up/Down
			// Diagonal charges
			new Vector2I(3, 3),
			new Vector2I(-3, -3), // Main diagonal
			new Vector2I(3, -3),
			new Vector2I(-3, 3), // Anti-diagonal
		};

		public ChargeKnight()
		{
			Type = PieceType.Knight;
			ClassName = "Charge";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = new List<Vector2I>();

			// Charge Knight can move in any straight line of exactly 3 cells
			// It can hop over any pieces in between
			foreach (var direction in chargeDirections)
			{
				var targetPos = BoardPosition + direction;
				if (IsValidPosition(targetPos))
				{
					var targetPiece = board.GetPieceAt(targetPos);
					// Can move to empty squares or capture enemy pieces
					if (targetPiece == null || IsEnemyPiece(targetPiece))
					{
						moves.Add(targetPos);
					}
				}
			}

			return moves;
		}
	}
}

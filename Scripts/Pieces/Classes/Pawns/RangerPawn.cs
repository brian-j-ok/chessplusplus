using Godot;
using System.Collections.Generic;
using ChessPlusPlus.Core;

namespace ChessPlusPlus.Pieces
{
	public partial class RangerPawn : Pawn
	{
		public RangerPawn()
		{
			Type = PieceType.Pawn;
			ClassName = "Ranger";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = new List<Vector2I>();
			int direction = Color == PieceColor.White ? 1 : -1;

			var oneStep = BoardPosition + new Vector2I(0, direction);
			if (board.IsSquareEmpty(oneStep))
			{
				moves.Add(oneStep);

				var twoStep = BoardPosition + new Vector2I(0, direction * 2);
				if (board.IsSquareEmpty(twoStep))
				{
					moves.Add(twoStep);
				}
			}

			var leftCapture = BoardPosition + new Vector2I(-1, direction);
			if (IsValidPosition(leftCapture) && board.IsSquareOccupiedByEnemy(leftCapture, Color))
			{
				moves.Add(leftCapture);
			}

			var rightCapture = BoardPosition + new Vector2I(1, direction);
			if (IsValidPosition(rightCapture) && board.IsSquareOccupiedByEnemy(rightCapture, Color))
			{
				moves.Add(rightCapture);
			}

			return moves;
		}
	}
}

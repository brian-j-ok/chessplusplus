using System.Collections.Generic;
using ChessPlusPlus.Core;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class Pawn : Piece
	{
		public Pawn()
		{
			Type = PieceType.Pawn;
			ClassName = "Standard";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = new List<Vector2I>();
			int direction = Color == PieceColor.White ? 1 : -1;

			var oneStep = BoardPosition + new Vector2I(0, direction);
			if (board.IsSquareEmpty(oneStep))
			{
				moves.Add(oneStep);

				if (!HasMoved)
				{
					var twoStep = BoardPosition + new Vector2I(0, direction * 2);
					if (board.IsSquareEmpty(twoStep))
					{
						moves.Add(twoStep);
					}
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

		public virtual bool CanBePromoted()
		{
			int promotionRank = Color == PieceColor.White ? 0 : 7;
			return BoardPosition.Y == promotionRank;
		}
	}
}

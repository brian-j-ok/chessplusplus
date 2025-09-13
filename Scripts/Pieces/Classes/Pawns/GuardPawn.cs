using Godot;
using System.Collections.Generic;
using ChessPlusPlus.Core;

namespace ChessPlusPlus.Pieces
{
	public partial class GuardPawn : Pawn
	{
		public GuardPawn()
		{
			Type = PieceType.Pawn;
			ClassName = "Guard";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = base.GetPossibleMoves(board);

			var leftDefend = BoardPosition + new Vector2I(-1, 0);
			if (IsValidPosition(leftDefend))
			{
				var piece = board.GetPieceAt(leftDefend);
				if (piece != null && IsEnemyPiece(piece))
				{
					moves.Add(leftDefend);
				}
			}

			var rightDefend = BoardPosition + new Vector2I(1, 0);
			if (IsValidPosition(rightDefend))
			{
				var piece = board.GetPieceAt(rightDefend);
				if (piece != null && IsEnemyPiece(piece))
				{
					moves.Add(rightDefend);
				}
			}

			return moves;
		}

		public List<Vector2I> GetDefendedSquares(Board board)
		{
			var defended = new List<Vector2I>();
			int direction = Color == PieceColor.White ? 1 : -1;

			var leftDiagonal = BoardPosition + new Vector2I(-1, direction);
			if (IsValidPosition(leftDiagonal))
				defended.Add(leftDiagonal);

			var rightDiagonal = BoardPosition + new Vector2I(1, direction);
			if (IsValidPosition(rightDiagonal))
				defended.Add(rightDiagonal);

			var leftSide = BoardPosition + new Vector2I(-1, 0);
			if (IsValidPosition(leftSide))
				defended.Add(leftSide);

			var rightSide = BoardPosition + new Vector2I(1, 0);
			if (IsValidPosition(rightSide))
				defended.Add(rightSide);

			return defended;
		}
	}
}

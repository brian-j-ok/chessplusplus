using Godot;
using System;
using System.Collections.Generic;
using ChessPlusPlus.Core;

namespace ChessPlusPlus.Pieces
{
	public partial class King : Piece
	{
		public King()
		{
			Type = PieceType.King;
			ClassName = "Standard";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = new List<Vector2I>();

			Vector2I[] directions = new Vector2I[]
			{
				new Vector2I(0, 1), new Vector2I(0, -1),
				new Vector2I(1, 0), new Vector2I(-1, 0),
				new Vector2I(1, 1), new Vector2I(1, -1),
				new Vector2I(-1, 1), new Vector2I(-1, -1)
			};

			foreach (var direction in directions)
			{
				var targetPos = BoardPosition + direction;

				if (IsValidPosition(targetPos))
				{
					var targetPiece = board.GetPieceAt(targetPos);
					if (targetPiece == null || IsEnemyPiece(targetPiece))
					{
						moves.Add(targetPos);
					}
				}
			}

			AddCastlingMoves(board, moves);

			return moves;
		}

		private void AddCastlingMoves(Board board, List<Vector2I> moves)
		{
			if (HasMoved)
				return;

			int row = Color == PieceColor.White ? 0 : 7;

			var kingRook = board.GetPieceAt(new Vector2I(7, row)) as Rook;
			if (kingRook != null && !kingRook.HasMoved)
			{
				bool pathClear = true;
				for (int x = 5; x <= 6; x++)
				{
					if (!board.IsSquareEmpty(new Vector2I(x, row)))
					{
						pathClear = false;
						break;
					}
				}
				if (pathClear)
				{
					moves.Add(new Vector2I(6, row));
				}
			}

			var queenRook = board.GetPieceAt(new Vector2I(0, row)) as Rook;
			if (queenRook != null && !queenRook.HasMoved)
			{
				bool pathClear = true;
				for (int x = 1; x <= 3; x++)
				{
					if (!board.IsSquareEmpty(new Vector2I(x, row)))
					{
						pathClear = false;
						break;
					}
				}
				if (pathClear)
				{
					moves.Add(new Vector2I(2, row));
				}
			}
		}

		public bool IsCastlingMove(Vector2I to)
		{
			return Math.Abs(to.X - BoardPosition.X) == 2;
		}
	}
}

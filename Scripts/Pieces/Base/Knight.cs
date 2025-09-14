using System.Collections.Generic;
using ChessPlusPlus.Core;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class Knight : Piece
	{
		protected Vector2I[] knightMoves = new Vector2I[]
		{
			new Vector2I(2, 1),
			new Vector2I(2, -1),
			new Vector2I(-2, 1),
			new Vector2I(-2, -1),
			new Vector2I(1, 2),
			new Vector2I(1, -2),
			new Vector2I(-1, 2),
			new Vector2I(-1, -2),
		};

		public Knight()
		{
			Type = PieceType.Knight;
			ClassName = "Standard";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = new List<Vector2I>();

			foreach (var move in knightMoves)
			{
				var targetPos = BoardPosition + move;
				if (IsValidPosition(targetPos))
				{
					var targetPiece = board.GetPieceAt(targetPos);
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

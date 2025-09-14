using System.Collections.Generic;
using ChessPlusPlus.Core;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class Rook : Piece
	{
		public Rook()
		{
			Type = PieceType.Rook;
			ClassName = "Standard";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = new List<Vector2I>();

			Vector2I[] directions = new Vector2I[]
			{
				new Vector2I(0, 1),
				new Vector2I(0, -1),
				new Vector2I(1, 0),
				new Vector2I(-1, 0),
			};

			foreach (var direction in directions)
			{
				AddMovesInDirection(board, moves, direction);
			}

			return moves;
		}

		protected void AddMovesInDirection(Board board, List<Vector2I> moves, Vector2I direction)
		{
			for (int distance = 1; distance < 8; distance++)
			{
				var targetPos = BoardPosition + (direction * distance);

				if (!IsValidPosition(targetPos))
					break;

				var targetPiece = board.GetPieceAt(targetPos);

				if (targetPiece == null)
				{
					moves.Add(targetPos);
				}
				else if (IsEnemyPiece(targetPiece))
				{
					moves.Add(targetPos);
					break;
				}
				else
				{
					break;
				}
			}
		}
	}
}

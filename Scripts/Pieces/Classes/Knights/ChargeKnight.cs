using Godot;
using System.Collections.Generic;
using ChessPlusPlus.Core;

namespace ChessPlusPlus.Pieces
{
	public partial class ChargeKnight : Knight
	{
		private Vector2I[] extendedMoves = new Vector2I[]
		{
			new Vector2I(3, 1), new Vector2I(3, -1),
			new Vector2I(-3, 1), new Vector2I(-3, -1),
			new Vector2I(1, 3), new Vector2I(1, -3),
			new Vector2I(-1, 3), new Vector2I(-1, -3)
		};

		public ChargeKnight()
		{
			Type = PieceType.Knight;
			ClassName = "Charge";
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = base.GetPossibleMoves(board);

			if (!HasMoved)
			{
				foreach (var move in extendedMoves)
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
			}

			return moves;
		}
	}
}

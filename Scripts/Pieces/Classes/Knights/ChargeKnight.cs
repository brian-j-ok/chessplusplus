using System.Collections.Generic;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class ChargeKnight : Knight, IMovementModifier
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

		// IMovementModifier implementation
		public string AbilityName => "Cavalry Charge";
		public string Description => "Moves exactly 3 squares in straight lines, jumping over pieces";

		public List<Vector2I> ModifyMovement(Piece piece, Board board, List<Vector2I> standardMoves)
		{
			// Replace standard knight movement with charge movement
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

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			// Use the ability system if available, otherwise fall back to direct implementation
			var baseMoves = base.GetPossibleMoves(board);
			return ModifyMovement(this, board, baseMoves);
		}
	}
}

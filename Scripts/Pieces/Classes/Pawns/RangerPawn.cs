using System.Collections.Generic;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class RangerPawn : Pawn, IMovementModifier
	{
		public RangerPawn()
		{
			Type = PieceType.Pawn;
			ClassName = "Ranger";
		}

		// IMovementModifier implementation
		public string AbilityName => "Ranger Training";
		public string Description => "Can always move 2 squares forward and capture diagonally backwards";

		public List<Vector2I> ModifyMovement(Piece piece, Board board, List<Vector2I> standardMoves)
		{
			// Start with empty list - we're replacing standard pawn movement
			var moves = new List<Vector2I>();
			int direction = Color == PieceColor.White ? 1 : -1;

			// Ranger pawns can always move 2 squares forward (not just on first move)
			var oneStep = BoardPosition + new Vector2I(0, direction);
			if (board.IsSquareEmpty(oneStep))
			{
				moves.Add(oneStep);

				var twoStep = BoardPosition + new Vector2I(0, direction * 2);
				if (IsValidPosition(twoStep) && board.IsSquareEmpty(twoStep))
				{
					moves.Add(twoStep);
				}
			}

			// Normal diagonal captures
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

			// Ranger special: can capture diagonally backwards (retreat capture)
			var leftRetreat = BoardPosition + new Vector2I(-1, -direction);
			if (IsValidPosition(leftRetreat) && board.IsSquareOccupiedByEnemy(leftRetreat, Color))
			{
				moves.Add(leftRetreat);
			}

			var rightRetreat = BoardPosition + new Vector2I(1, -direction);
			if (IsValidPosition(rightRetreat) && board.IsSquareOccupiedByEnemy(rightRetreat, Color))
			{
				moves.Add(rightRetreat);
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

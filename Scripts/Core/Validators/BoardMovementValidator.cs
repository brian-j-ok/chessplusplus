namespace ChessPlusPlus.Core.Validators
{
	using System;
	using ChessPlusPlus.Pieces;
	using Godot;

	/// <summary>
	/// Validates piece movements and board positions
	/// </summary>
	public static class BoardMovementValidator
	{
		/// <summary>
		/// Checks if a move is valid without executing it
		/// </summary>
		public static bool IsValidMove(Board board, Vector2I from, Vector2I to)
		{
			var piece = board.GetPieceAt(from);
			if (piece == null || !piece.CanMoveTo(to, board))
				return false;

			if (!board.IsMoveLegal(from, to, piece.Color))
				return false;

			return true;
		}

		/// <summary>
		/// Checks if a piece can be captured from a specific direction
		/// </summary>
		public static bool CanBeCapturedFrom(Piece target, Vector2I attackerFrom, Vector2I targetPos)
		{
			// Guard Pawns can't be captured from horizontal or vertical directions
			if (target is GuardPawn)
			{
				var delta = targetPos - attackerFrom;
				var absX = Math.Abs(delta.X);
				var absY = Math.Abs(delta.Y);

				// Check if movement is purely horizontal or vertical
				if (absX == 0 || absY == 0)
				{
					return false; // Guard Pawn is immune to horizontal/vertical captures
				}
			}
			return true; // Normal pieces can be captured from any direction
		}

		/// <summary>
		/// Validates if a position is within the board boundaries
		/// </summary>
		public static bool IsValidPosition(Vector2I position)
		{
			return position.X >= 0 && position.X < 8 && position.Y >= 0 && position.Y < 8;
		}

		/// <summary>
		/// Gets valid moves for a piece, filtering out illegal captures
		/// </summary>
		public static System.Collections.Generic.List<Vector2I> GetValidMovesForPiece(Board board, Piece piece)
		{
			var possibleMoves = piece.GetPossibleMoves(board);
			var validMoves = new System.Collections.Generic.List<Vector2I>();

			foreach (var move in possibleMoves)
			{
				var targetPiece = board.GetPieceAt(move);

				// If there's an enemy piece, check if it can actually be captured
				if (targetPiece != null && piece.IsEnemyPiece(targetPiece))
				{
					if (CanBeCapturedFrom(targetPiece, piece.BoardPosition, move))
					{
						validMoves.Add(move);
					}
				}
				else if (targetPiece == null)
				{
					// Empty square - always valid
					validMoves.Add(move);
				}
			}

			return validMoves;
		}
	}
}

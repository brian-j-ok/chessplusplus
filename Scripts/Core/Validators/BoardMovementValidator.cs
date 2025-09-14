namespace ChessPlusPlus.Core.Validators
{
	using System;
	using ChessPlusPlus.Core.Abilities;
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
		public static bool IsValidMove(Board board, Vector2I from, Vector2I to, BoardStateManager? stateManager = null)
		{
			var piece = board.GetPieceAt(from);
			if (piece == null)
				return false;

			// Check if piece is frozen
			if (stateManager != null && !stateManager.CanPieceMove(piece))
				return false;

			if (!piece.CanMoveTo(to, board))
				return false;

			if (!board.IsMoveLegal(from, to, piece.Color))
				return false;

			return true;
		}

		/// <summary>
		/// Checks if a piece can be captured from a specific direction
		/// </summary>
		public static bool CanBeCapturedFrom(
			Piece target,
			Vector2I attackerFrom,
			Vector2I targetPos,
			BoardStateManager? stateManager = null
		)
		{
			// Use ability system if available
			if (stateManager != null)
			{
				return stateManager.CanBeCapturedFrom(target, attackerFrom);
			}

			// Fallback for legacy code - check if target has defensive ability
			if (target is IDefensiveAbility defensiveAbility)
			{
				return defensiveAbility.CanBeCapturedFrom(target, attackerFrom, null!);
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
		/// Gets valid moves for a piece, filtering out illegal captures and applying abilities
		/// </summary>
		public static System.Collections.Generic.List<Vector2I> GetValidMovesForPiece(
			Board board,
			Piece piece,
			BoardStateManager? stateManager = null
		)
		{
			var possibleMoves = piece.GetPossibleMoves(board);

			// Apply ability modifications if state manager is available
			if (stateManager != null)
			{
				possibleMoves = stateManager.GetModifiedMoves(piece, possibleMoves);

				// Add additional captures from abilities
				var additionalCaptures = stateManager.GetAdditionalCaptures(piece);
				possibleMoves.AddRange(additionalCaptures);
			}

			var validMoves = new System.Collections.Generic.List<Vector2I>();

			foreach (var move in possibleMoves)
			{
				var targetPiece = board.GetPieceAt(move);

				// If there's an enemy piece, check if it can actually be captured
				if (targetPiece != null && piece.IsEnemyPiece(targetPiece))
				{
					if (CanBeCapturedFrom(targetPiece, piece.BoardPosition, move, stateManager))
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

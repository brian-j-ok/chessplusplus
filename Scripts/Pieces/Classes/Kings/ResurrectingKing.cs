using System;
using System.Collections.Generic;
using System.Linq;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class ResurrectingKing : King, IMovementModifier
	{
		private bool hasResurrected = false;

		public ResurrectingKing()
		{
			Type = PieceType.King;
			ClassName = "Resurrecting";
		}

		// IMovementModifier implementation
		public string AbilityName => "Phoenix Revival";
		public string Description => "Once per game, when about to be checkmated, teleports to a random safe square";

		public List<Vector2I> ModifyMovement(Piece piece, Board board, List<Vector2I> standardMoves)
		{
			// If we've already used resurrection, return standard moves
			if (hasResurrected)
			{
				return standardMoves;
			}

			// Check if we're in check
			bool inCheck = board.IsKingInCheck(Color);

			// Check if any standard moves would be legal (get us out of check)
			bool hasLegalMove = false;
			foreach (var move in standardMoves)
			{
				if (board.IsMoveLegal(BoardPosition, move, Color))
				{
					hasLegalMove = true;
					break;
				}
			}

			// If in check and no legal standard moves, activate resurrection
			if (inCheck && !hasLegalMove)
			{
				GD.Print($"ResurrectingKing detected checkmate! Searching for resurrection squares...");

				// Find all safe resurrection squares
				var resurrectionSquares = FindSafeResurrectionSquares(board);

				if (resurrectionSquares.Count > 0)
				{
					GD.Print(
						$"ResurrectingKing activating Phoenix Revival! {resurrectionSquares.Count} safe squares found."
					);

					// Choose a random safe square for teleportation
					var random = new Random();
					var chosenSquare = resurrectionSquares[random.Next(resurrectionSquares.Count)];

					GD.Print($"Chosen resurrection square: {chosenSquare}");

					// Add the resurrection square to the standard moves
					// This allows the player to select it as an escape
					var modifiedMoves = new List<Vector2I>(standardMoves);
					modifiedMoves.Add(chosenSquare);
					return modifiedMoves;
				}
				else
				{
					GD.Print("No safe resurrection squares found - true checkmate!");
				}
			}

			// Return standard moves if not in checkmate or no safe squares
			return standardMoves;
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			// Get standard king moves
			var baseMoves = base.GetPossibleMoves(board);

			// Debug output
			if (!hasResurrected && board.IsKingInCheck(Color))
			{
				GD.Print($"ResurrectingKing at {BoardPosition}: In check, {baseMoves.Count} standard moves available");
			}

			// Apply ability modifications
			return ModifyMovement(this, board, baseMoves);
		}

		/// <summary>
		/// Finds all safe squares the king could resurrect to
		/// </summary>
		private List<Vector2I> FindSafeResurrectionSquares(Board board)
		{
			var safeSquares = new List<Vector2I>();

			// Check all squares on the board
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var pos = new Vector2I(x, y);

					// Skip occupied squares
					if (!board.IsSquareEmpty(pos))
						continue;

					// Skip the current position
					if (pos == BoardPosition)
						continue;

					// Check if this square would be safe (not under attack)
					if (!WouldBeInCheckAt(pos, board))
					{
						safeSquares.Add(pos);
					}
				}
			}

			return safeSquares;
		}

		/// <summary>
		/// Checks if the king would be in check at a specific position
		/// </summary>
		private bool WouldBeInCheckAt(Vector2I position, Board board)
		{
			// Check all enemy pieces to see if they could attack this position
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = board.GetPieceAt(new Vector2I(x, y));
					if (piece != null && piece.Color != Color)
					{
						// Special case: enemy king can't put us in check from more than 1 square away
						if (piece is King)
						{
							var distance = position - piece.BoardPosition;
							if (Math.Abs(distance.X) <= 1 && Math.Abs(distance.Y) <= 1)
							{
								return true;
							}
						}
						else
						{
							// For other pieces, check their possible moves
							// We need to be careful here to avoid infinite recursion
							var enemyMoves = piece.GetPossibleMoves(board);
							if (enemyMoves.Contains(position))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public override void OnMoved(Vector2I from, Vector2I to, Board board)
		{
			base.OnMoved(from, to, board);

			// Check if this was a resurrection move (moved more than 2 squares - beyond castling)
			var distance = Math.Max(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
			if (distance > 2)
			{
				hasResurrected = true;
				GD.Print(
					$"ResurrectingKing used Phoenix Revival! Escaped checkmate by teleporting from {from} to {to}"
				);
			}
		}

		public override bool CanMoveTo(Vector2I targetPosition, Board board)
		{
			// Check if this is a resurrection move (distance > 2 from current position)
			var distance = Math.Max(
				Math.Abs(targetPosition.X - BoardPosition.X),
				Math.Abs(targetPosition.Y - BoardPosition.Y)
			);
			if (!hasResurrected && distance > 2)
			{
				// This is a potential resurrection move - check if we're in checkmate
				bool inCheck = board.IsKingInCheck(Color);
				var standardMoves = base.GetPossibleMoves(board);
				bool hasLegalMove = false;

				foreach (var move in standardMoves)
				{
					if (board.IsMoveLegal(BoardPosition, move, Color))
					{
						hasLegalMove = true;
						break;
					}
				}

				// If in checkmate and this is a safe square, allow the move
				if (inCheck && !hasLegalMove && !WouldBeInCheckAt(targetPosition, board))
				{
					GD.Print($"ResurrectingKing: Allowing resurrection move to {targetPosition}");
					return true;
				}
			}

			// Otherwise check standard king movement
			return base.CanMoveTo(targetPosition, board);
		}
	}
}

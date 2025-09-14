using System;
using System.Collections.Generic;
using System.Linq;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class GlassQueen : Queen, IMultiMoveAbility, IDefensiveAbility
	{
		private int movesThisTurn = 0;

		public GlassQueen()
		{
			Type = PieceType.Queen;
			ClassName = "Glass";
		}

		// IMultiMoveAbility implementation
		public string AbilityName => "Glass Cannon";
		public string Description => "Must move twice per turn but shatters if exposed to horizontal threats";
		public int MovesPerTurn => 2;
		public bool MandatoryMoves => true;

		public void OnMoveCompleted(int moveNumber, Vector2I from, Vector2I to, Board board)
		{
			movesThisTurn = moveNumber;
			GD.Print($"GlassQueen completed move {moveNumber} of {MovesPerTurn} from {from} to {to}");

			// Check if we're vulnerable after each move
			if (IsVulnerableToHorizontalAttack(to, board))
			{
				GD.Print($"WARNING: GlassQueen is exposed to horizontal attack at {to}!");
				// Mark for auto-capture in BoardStateManager
			}

			if (moveNumber < MovesPerTurn)
			{
				GD.Print($"GlassQueen must make {MovesPerTurn - moveNumber} more move(s) this turn!");
			}
		}

		public bool CanEndTurn(int movesMade)
		{
			// Can only end turn after making all required moves
			return movesMade >= MovesPerTurn;
		}

		public void ResetMoveCounter()
		{
			movesThisTurn = 0;
		}

		// IDefensiveAbility implementation - Glass Queen is vulnerable
		public bool CanBeCapturedFrom(Piece piece, Vector2I attackerPos, Board board)
		{
			// Glass Queen can always be captured normally
			// But additionally, she auto-shatters when exposed horizontally
			return true;
		}

		/// <summary>
		/// Checks if the Glass Queen is vulnerable to horizontal attack
		/// </summary>
		public bool IsVulnerableToHorizontalAttack(Vector2I position, Board board)
		{
			int y = position.Y;

			// Check left direction
			for (int x = position.X - 1; x >= 0; x--)
			{
				var checkPos = new Vector2I(x, y);
				var piece = board.GetPieceAt(checkPos);

				if (piece != null)
				{
					if (piece.Color != Color)
					{
						// Found an enemy piece with clear line of sight
						GD.Print($"GlassQueen vulnerable to {piece.Color} {piece.Type} at {checkPos}");
						return true;
					}
					else
					{
						// Friendly piece blocks line of sight
						break;
					}
				}
			}

			// Check right direction
			for (int x = position.X + 1; x < 8; x++)
			{
				var checkPos = new Vector2I(x, y);
				var piece = board.GetPieceAt(checkPos);

				if (piece != null)
				{
					if (piece.Color != Color)
					{
						// Found an enemy piece with clear line of sight
						GD.Print($"GlassQueen vulnerable to {piece.Color} {piece.Type} at {checkPos}");
						return true;
					}
					else
					{
						// Friendly piece blocks line of sight
						break;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Override to handle the double-move requirement
		/// </summary>
		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			var moves = base.GetPossibleMoves(board);

			// During second move, warn if all moves are dangerous
			if (movesThisTurn == 1)
			{
				var safeMoves = moves.Where(move => !IsVulnerableToHorizontalAttack(move, board)).ToList();
				if (safeMoves.Count == 0 && moves.Count > 0)
				{
					GD.Print("GlassQueen: WARNING - All available moves lead to shattering!");
				}
			}

			return moves;
		}

		/// <summary>
		/// Called after a move to check if the Glass Queen should shatter
		/// </summary>
		public bool ShouldShatter(Board board)
		{
			return IsVulnerableToHorizontalAttack(BoardPosition, board);
		}

		public override void OnMoved(Vector2I from, Vector2I to, Board board)
		{
			base.OnMoved(from, to, board);

			// The actual move tracking is now handled by BoardStateManager through IMultiMoveAbility
			// After moving, check if we should auto-shatter
			if (ShouldShatter(board))
			{
				// Schedule auto-capture
				// This would need to be handled by the game manager
				GD.Print($"GlassQueen at {to} shatters due to horizontal exposure!");
			}
		}
	}
}

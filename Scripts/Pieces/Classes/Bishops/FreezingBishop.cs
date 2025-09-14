using System.Collections.Generic;
using System.Linq;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class FreezingBishop : Bishop, IPassiveAbility, IBoardEffect
	{
		private List<Piece> currentlyFrozenPieces = new();

		public FreezingBishop()
		{
			Type = PieceType.Bishop;
			ClassName = "Freezing";
		}

		// IPassiveAbility implementation
		public string AbilityName => "Frost Aura";
		public string Description => "Freezes enemy pieces that land adjacent to this bishop until it moves";

		public void OnPieceMoved(Piece movedPiece, Vector2I from, Vector2I to, Board board)
		{
			// Check if an enemy piece moved adjacent to this bishop
			if (movedPiece != this && IsEnemyPiece(movedPiece) && AreAdjacent(BoardPosition, to))
			{
				// Get the board state manager and freeze the piece
				var stateManager = board.GetNode<BoardStateManager>("BoardStateManager");
				if (stateManager != null)
				{
					stateManager.FreezePiece(movedPiece, this, -1); // -1 for permanent until bishop moves
					if (!currentlyFrozenPieces.Contains(movedPiece))
					{
						currentlyFrozenPieces.Add(movedPiece);
					}
					GD.Print($"FreezingBishop at {BoardPosition} froze {movedPiece.Color} {movedPiece.Type} at {to}");
				}
			}
		}

		public void OnTurnStart(PieceColor currentTurn, Board board)
		{
			// No turn-based effects for this ability
		}

		public void OnSelfMoved(Vector2I from, Vector2I to, Board board)
		{
			// When this bishop moves, unfreeze all pieces it was freezing
			var stateManager = board.GetNode<BoardStateManager>("BoardStateManager");
			if (stateManager != null)
			{
				foreach (var frozenPiece in currentlyFrozenPieces)
				{
					// Only unfreeze if piece is no longer adjacent
					if (!AreAdjacent(to, frozenPiece.BoardPosition))
					{
						stateManager.UnfreezePiece(frozenPiece);
					}
				}

				// Update list to only include pieces still adjacent and frozen
				currentlyFrozenPieces = currentlyFrozenPieces.Where(p => AreAdjacent(to, p.BoardPosition)).ToList();
			}
		}

		// IBoardEffect implementation
		public List<Piece> GetAffectedPieces(Piece source, Board board)
		{
			var affected = new List<Piece>();

			// Get all adjacent squares
			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					if (dx == 0 && dy == 0)
						continue;

					var checkPos = BoardPosition + new Vector2I(dx, dy);
					if (IsValidPosition(checkPos))
					{
						var piece = board.GetPieceAt(checkPos);
						if (piece != null && IsEnemyPiece(piece))
						{
							affected.Add(piece);
						}
					}
				}
			}

			return affected;
		}

		public void ApplyEffect(Piece source, List<Piece> targets, Board board)
		{
			var stateManager = board.GetNode<BoardStateManager>("BoardStateManager");
			if (stateManager != null)
			{
				foreach (var target in targets)
				{
					stateManager.FreezePiece(target, source, -1);
					if (!currentlyFrozenPieces.Contains(target))
					{
						currentlyFrozenPieces.Add(target);
					}
				}
			}
		}

		public void RemoveEffect(Piece source, List<Piece> targets, Board board)
		{
			var stateManager = board.GetNode<BoardStateManager>("BoardStateManager");
			if (stateManager != null)
			{
				foreach (var target in targets)
				{
					stateManager.UnfreezePiece(target);
					currentlyFrozenPieces.Remove(target);
				}
			}
		}

		private bool AreAdjacent(Vector2I pos1, Vector2I pos2)
		{
			var diff = pos2 - pos1;
			return Mathf.Abs(diff.X) <= 1 && Mathf.Abs(diff.Y) <= 1 && diff != Vector2I.Zero;
		}
	}
}

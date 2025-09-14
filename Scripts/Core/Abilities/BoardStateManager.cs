using System.Collections.Generic;
using System.Linq;
using ChessPlusPlus.Pieces;
using Godot;

namespace ChessPlusPlus.Core.Abilities
{
	/// <summary>
	/// Tracks piece states and effects that persist across turns
	/// </summary>
	public class PieceState
	{
		public bool IsFrozen { get; set; }
		public Piece? FrozenBy { get; set; }
		public int FrozenDuration { get; set; }
		public Dictionary<string, object> CustomStates { get; set; } = new();

		public void ClearFrozenState()
		{
			IsFrozen = false;
			FrozenBy = null;
			FrozenDuration = 0;
		}
	}

	/// <summary>
	/// Manages board-wide effects and piece states
	/// </summary>
	public partial class BoardStateManager : Node
	{
		private Dictionary<Piece, PieceState> pieceStates = new();
		private Dictionary<Piece, List<IAbility>> pieceAbilities = new();
		private Board board = null!;

		public void Initialize(Board gameBoard)
		{
			board = gameBoard;
			pieceStates.Clear();
			pieceAbilities.Clear();
		}

		/// <summary>
		/// Registers a piece and its abilities with the state manager
		/// </summary>
		public void RegisterPiece(Piece piece)
		{
			if (!pieceStates.ContainsKey(piece))
			{
				pieceStates[piece] = new PieceState();
			}

			// Detect and register abilities
			var abilities = new List<IAbility>();
			if (piece is IMovementModifier moveMod)
				abilities.Add(moveMod);
			if (piece is ICaptureModifier capMod)
				abilities.Add(capMod);
			if (piece is IDefensiveAbility defAbility)
				abilities.Add(defAbility);
			if (piece is IPassiveAbility passiveAbility)
				abilities.Add(passiveAbility);
			if (piece is IBoardEffect boardEffect)
				abilities.Add(boardEffect);

			if (abilities.Count > 0)
			{
				pieceAbilities[piece] = abilities;
				GD.Print($"Registered {piece.ClassName} {piece.Type} with {abilities.Count} abilities");
			}
		}

		/// <summary>
		/// Gets the state for a specific piece
		/// </summary>
		public PieceState GetPieceState(Piece piece)
		{
			if (!pieceStates.ContainsKey(piece))
			{
				pieceStates[piece] = new PieceState();
			}
			return pieceStates[piece];
		}

		/// <summary>
		/// Checks if a piece can move (considering frozen state, etc.)
		/// </summary>
		public bool CanPieceMove(Piece piece)
		{
			var state = GetPieceState(piece);
			return !state.IsFrozen;
		}

		/// <summary>
		/// Freezes a piece for a duration
		/// </summary>
		public void FreezePiece(Piece target, Piece source, int duration = -1)
		{
			var state = GetPieceState(target);
			state.IsFrozen = true;
			state.FrozenBy = source;
			state.FrozenDuration = duration;
			GD.Print($"{target.Color} {target.Type} frozen by {source.Color} {source.Type}");
		}

		/// <summary>
		/// Unfreezes a piece
		/// </summary>
		public void UnfreezePiece(Piece target)
		{
			var state = GetPieceState(target);
			if (state.IsFrozen)
			{
				GD.Print($"{target.Color} {target.Type} unfrozen");
				state.ClearFrozenState();
			}
		}

		/// <summary>
		/// Called when a piece moves - triggers passive abilities
		/// </summary>
		public void OnPieceMoved(Piece piece, Vector2I from, Vector2I to)
		{
			// Trigger all passive abilities
			foreach (var kvp in pieceAbilities)
			{
				foreach (var ability in kvp.Value)
				{
					if (ability is IPassiveAbility passive)
					{
						passive.OnPieceMoved(piece, from, to, board);
					}
				}
			}

			// Handle self-movement abilities
			if (pieceAbilities.ContainsKey(piece))
			{
				foreach (var ability in pieceAbilities[piece])
				{
					if (ability is IPassiveAbility passive)
					{
						passive.OnSelfMoved(from, to, board);
					}
				}
			}

			// Check frozen states - unfreeze if freezer moved away
			foreach (var kvp in pieceStates)
			{
				if (kvp.Value.IsFrozen && kvp.Value.FrozenBy == piece)
				{
					// Check if still adjacent
					var frozenPiece = kvp.Key;
					if (!AreAdjacent(piece.BoardPosition, frozenPiece.BoardPosition))
					{
						UnfreezePiece(frozenPiece);
					}
				}
			}
		}

		/// <summary>
		/// Called at the start of each turn
		/// </summary>
		public void OnTurnStart(PieceColor currentTurn)
		{
			// Update duration-based effects
			foreach (var state in pieceStates.Values)
			{
				if (state.IsFrozen && state.FrozenDuration > 0)
				{
					state.FrozenDuration--;
					if (state.FrozenDuration == 0)
					{
						state.ClearFrozenState();
					}
				}
			}

			// Trigger turn-start abilities
			foreach (var kvp in pieceAbilities)
			{
				foreach (var ability in kvp.Value)
				{
					if (ability is IPassiveAbility passive)
					{
						passive.OnTurnStart(currentTurn, board);
					}
				}
			}
		}

		/// <summary>
		/// Gets modified possible moves for a piece, considering all abilities and states
		/// </summary>
		public List<Vector2I> GetModifiedMoves(Piece piece, List<Vector2I> baseMoves)
		{
			var moves = new List<Vector2I>(baseMoves);

			// Check if piece is frozen
			if (!CanPieceMove(piece))
			{
				return new List<Vector2I>(); // No moves if frozen
			}

			// Apply movement modifiers
			if (pieceAbilities.ContainsKey(piece))
			{
				foreach (var ability in pieceAbilities[piece])
				{
					if (ability is IMovementModifier modifier)
					{
						moves = modifier.ModifyMovement(piece, board, moves);
					}
				}
			}

			return moves;
		}

		/// <summary>
		/// Gets additional capture targets from abilities
		/// </summary>
		public List<Vector2I> GetAdditionalCaptures(Piece piece)
		{
			var captures = new List<Vector2I>();

			if (pieceAbilities.ContainsKey(piece))
			{
				foreach (var ability in pieceAbilities[piece])
				{
					if (ability is ICaptureModifier modifier)
					{
						captures.AddRange(modifier.GetAdditionalCaptures(piece, board));
					}
				}
			}

			return captures;
		}

		/// <summary>
		/// Checks if a piece can be captured from a specific position
		/// </summary>
		public bool CanBeCapturedFrom(Piece target, Vector2I attackerPos)
		{
			if (pieceAbilities.ContainsKey(target))
			{
				foreach (var ability in pieceAbilities[target])
				{
					if (ability is IDefensiveAbility defensive)
					{
						if (!defensive.CanBeCapturedFrom(target, attackerPos, board))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Removes a piece from tracking (when captured)
		/// </summary>
		public void UnregisterPiece(Piece piece)
		{
			// Remove any effects this piece was causing
			foreach (var kvp in pieceStates)
			{
				if (kvp.Value.FrozenBy == piece)
				{
					kvp.Value.ClearFrozenState();
				}
			}

			pieceStates.Remove(piece);
			pieceAbilities.Remove(piece);
		}

		/// <summary>
		/// Helper to check if two positions are adjacent
		/// </summary>
		private bool AreAdjacent(Vector2I pos1, Vector2I pos2)
		{
			var diff = pos2 - pos1;
			return Mathf.Abs(diff.X) <= 1 && Mathf.Abs(diff.Y) <= 1 && diff != Vector2I.Zero;
		}
	}
}

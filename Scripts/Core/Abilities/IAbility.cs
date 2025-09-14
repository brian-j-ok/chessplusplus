using System.Collections.Generic;
using ChessPlusPlus.Pieces;
using Godot;

namespace ChessPlusPlus.Core.Abilities
{
	/// <summary>
	/// Base interface for all piece abilities
	/// </summary>
	public interface IAbility
	{
		/// <summary>
		/// Name of the ability for identification
		/// </summary>
		string AbilityName { get; }

		/// <summary>
		/// Description of what the ability does
		/// </summary>
		string Description { get; }
	}

	/// <summary>
	/// Modifies how a piece moves on the board
	/// </summary>
	public interface IMovementModifier : IAbility
	{
		/// <summary>
		/// Modifies or replaces the piece's movement pattern
		/// </summary>
		List<Vector2I> ModifyMovement(Piece piece, Board board, List<Vector2I> standardMoves);
	}

	/// <summary>
	/// Modifies how a piece captures enemies
	/// </summary>
	public interface ICaptureModifier : IAbility
	{
		/// <summary>
		/// Returns additional capture targets beyond normal movement
		/// </summary>
		List<Vector2I> GetAdditionalCaptures(Piece piece, Board board);

		/// <summary>
		/// Validates if a capture is allowed with this ability
		/// </summary>
		bool CanCapture(Piece piece, Vector2I targetPos, Board board);
	}

	/// <summary>
	/// Provides defensive capabilities to a piece
	/// </summary>
	public interface IDefensiveAbility : IAbility
	{
		/// <summary>
		/// Checks if this piece can be captured from a specific position
		/// </summary>
		bool CanBeCapturedFrom(Piece piece, Vector2I attackerPos, Board board);
	}

	/// <summary>
	/// Ability that triggers based on game events
	/// </summary>
	public interface IPassiveAbility : IAbility
	{
		/// <summary>
		/// Called when any piece moves on the board
		/// </summary>
		void OnPieceMoved(Piece movedPiece, Vector2I from, Vector2I to, Board board);

		/// <summary>
		/// Called at the start of each turn
		/// </summary>
		void OnTurnStart(PieceColor currentTurn, Board board);

		/// <summary>
		/// Called when this piece is moved
		/// </summary>
		void OnSelfMoved(Vector2I from, Vector2I to, Board board);
	}

	/// <summary>
	/// Ability that affects other pieces on the board
	/// </summary>
	public interface IBoardEffect : IAbility
	{
		/// <summary>
		/// Gets all pieces currently affected by this ability
		/// </summary>
		List<Piece> GetAffectedPieces(Piece source, Board board);

		/// <summary>
		/// Applies the effect to affected pieces
		/// </summary>
		void ApplyEffect(Piece source, List<Piece> targets, Board board);

		/// <summary>
		/// Removes the effect from affected pieces
		/// </summary>
		void RemoveEffect(Piece source, List<Piece> targets, Board board);
	}

	/// <summary>
	/// Represents an effect that persists across turns
	/// </summary>
	public interface IPersistentEffect : IAbility
	{
		/// <summary>
		/// Duration of the effect in turns (-1 for permanent until condition met)
		/// </summary>
		int Duration { get; }

		/// <summary>
		/// Checks if the effect should end
		/// </summary>
		bool ShouldEnd(Piece source, Board board);
	}
}

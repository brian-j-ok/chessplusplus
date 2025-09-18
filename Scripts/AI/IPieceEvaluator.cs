namespace ChessPlusPlus.AI
{
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	/// <summary>
	/// Interface for evaluating chess pieces and positions dynamically
	/// </summary>
	public interface IPieceEvaluator
	{
		/// <summary>
		/// Get the base material value of a piece
		/// </summary>
		float GetPieceValue(Piece piece);

		/// <summary>
		/// Get positional bonus for a piece at a specific location
		/// </summary>
		float GetPositionalBonus(Piece piece, Vector2I position, Board board);

		/// <summary>
		/// Evaluate the value of a piece's special abilities
		/// </summary>
		float GetAbilityValue(Piece piece, Board board);

		/// <summary>
		/// Score based on piece mobility (number of possible moves)
		/// </summary>
		float GetMobilityScore(Piece piece, Board board);

		/// <summary>
		/// Evaluate threats this piece creates or faces
		/// </summary>
		float GetThreatScore(Piece piece, Vector2I position, Board board);

		/// <summary>
		/// Complete evaluation of a board position
		/// </summary>
		float EvaluatePosition(Board board, PieceColor perspective);

		/// <summary>
		/// Evaluate a specific move
		/// </summary>
		float EvaluateMove(Move move, Board board, int depth = 0);

		/// <summary>
		/// Set difficulty level to adjust evaluation complexity
		/// </summary>
		void SetDifficulty(AIDifficulty difficulty);
	}
}

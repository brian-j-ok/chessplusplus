namespace ChessPlusPlus.Core.Validators
{
	using System.Collections.Generic;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.UI;
	using Godot;

	/// <summary>
	/// Manages piece movement highlighting on the board
	/// </summary>
	public class PieceHighlighter
	{
		private readonly Board board;
		private readonly BoardVisual boardVisual;

		public PieceHighlighter(Board board, BoardVisual boardVisual)
		{
			this.board = board;
			this.boardVisual = boardVisual;
		}

		/// <summary>
		/// Highlights all possible moves for a selected piece
		/// </summary>
		public void HighlightPossibleMoves(Piece piece)
		{
			if (piece == null)
				return;

			ClearHighlights();
			boardVisual.HighlightSelectedSquare(piece.BoardPosition);

			var possibleMoves = GetValidMovesForHighlighting(piece);
			GD.Print(
				$"{piece.Color} {piece.Type} at {piece.BoardPosition} has {possibleMoves.Count} possible moves: [{string.Join(", ", possibleMoves)}]"
			);
			boardVisual.HighlightValidMoves(possibleMoves, board);
		}

		/// <summary>
		/// Clears all highlights from the board
		/// </summary>
		public void ClearHighlights()
		{
			boardVisual.ClearHighlights();
		}

		/// <summary>
		/// Gets valid moves for highlighting, filtering out illegal captures
		/// </summary>
		private List<Vector2I> GetValidMovesForHighlighting(Piece piece)
		{
			return BoardMovementValidator.GetValidMovesForPiece(board, piece);
		}
	}
}

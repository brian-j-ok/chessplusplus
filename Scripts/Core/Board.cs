namespace ChessPlusPlus.Core
{
	using System.Collections.Generic;
	using ChessPlusPlus.Pieces;
	using Godot;

	public partial class Board : Node2D
	{
		private Piece?[,] pieces = new Piece?[8, 8];
		private Army whiteArmy = null!;
		private Army blackArmy = null!;
		private ChessPlusPlus.UI.BoardVisual boardVisual = null!;

		[Signal]
		public delegate void PieceMovedEventHandler(Piece piece, Vector2I from, Vector2I to);

		[Signal]
		public delegate void PieceCapturedEventHandler(Piece captured, Piece capturer);

		[Signal]
		public delegate void PawnPromotionEventHandler(Pawn pawn, Vector2I position);

		public override void _Ready()
		{
			InitializeBoard();
		}

		private void InitializeBoard()
		{
			// Create board visual
			boardVisual = new ChessPlusPlus.UI.BoardVisual();
			boardVisual.Name = "BoardVisual";
			AddChild(boardVisual);
			MoveChild(boardVisual, 0); // Put visual behind pieces

			whiteArmy = new Army(PieceColor.White);
			blackArmy = new Army(PieceColor.Black);
		}

		public void SetupStandardBoard()
		{
			SetupArmy(PieceColor.White, 0, 1);
			SetupArmy(PieceColor.Black, 7, 6);
		}

		private void SetupArmy(PieceColor color, int backRow, int pawnRow)
		{
			Army army = color == PieceColor.White ? whiteArmy : blackArmy;

			for (int x = 0; x < 8; x++)
			{
				var pawn = army.CreatePiece(PieceType.Pawn, x);
				PlacePiece(pawn, new Vector2I(x, pawnRow));
			}

			PlacePiece(army.CreatePiece(PieceType.Rook, 0), new Vector2I(0, backRow));
			PlacePiece(army.CreatePiece(PieceType.Knight, 1), new Vector2I(1, backRow));
			PlacePiece(army.CreatePiece(PieceType.Bishop, 2), new Vector2I(2, backRow));
			PlacePiece(army.CreatePiece(PieceType.Queen, 3), new Vector2I(3, backRow));
			PlacePiece(army.CreatePiece(PieceType.King, 4), new Vector2I(4, backRow));
			PlacePiece(army.CreatePiece(PieceType.Bishop, 5), new Vector2I(5, backRow));
			PlacePiece(army.CreatePiece(PieceType.Knight, 6), new Vector2I(6, backRow));
			PlacePiece(army.CreatePiece(PieceType.Rook, 7), new Vector2I(7, backRow));
		}

		public void PlacePiece(Piece piece, Vector2I position)
		{
			if (!IsValidPosition(position))
				return;

			pieces[position.X, position.Y] = piece;
			piece.BoardPosition = position;
			AddChild(piece);

			piece.Position = BoardToWorldPosition(position);
			GD.Print($"Placed {piece.Color} {piece.Type} at board position {position}, world position {piece.Position}");
		}

		public bool MovePiece(Vector2I from, Vector2I to)
		{
			var piece = GetPieceAt(from);
			if (piece == null || !piece.CanMoveTo(to, this))
				return false;

			// Check if this move would leave the king in check
			if (!IsMoveLegal(from, to, piece.Color))
				return false;

			var targetPiece = GetPieceAt(to);
			if (targetPiece != null)
			{
				if (piece.IsEnemyPiece(targetPiece))
				{
					CapturePiece(targetPiece, piece);
				}
				else
				{
					return false;
				}
			}

			pieces[from.X, from.Y] = null;
			pieces[to.X, to.Y] = piece;

			// Handle castling
			if (piece is King king && king.IsCastlingMove(to))
			{
				HandleCastling(king, from, to);
			}

			piece.OnMoved(from, to, this);
			piece.Position = BoardToWorldPosition(to);

			EmitSignal(SignalName.PieceMoved, piece, from, to);

			// Check for pawn promotion
			if (piece is Pawn pawn && pawn.CanBePromoted())
			{
				EmitSignal(SignalName.PawnPromotion, pawn, to);
			}

			return true;
		}

		private void CapturePiece(Piece captured, Piece capturer)
		{
			pieces[captured.BoardPosition.X, captured.BoardPosition.Y] = null;
			EmitSignal(SignalName.PieceCaptured, captured, capturer);
			captured.OnCaptured(this);
		}

		public Piece? GetPieceAt(Vector2I position)
		{
			if (!IsValidPosition(position))
				return null;
			return pieces[position.X, position.Y];
		}

		public bool IsSquareEmpty(Vector2I position)
		{
			return IsValidPosition(position) && pieces[position.X, position.Y] == null;
		}

		public bool IsSquareOccupiedByEnemy(Vector2I position, PieceColor friendlyColor)
		{
			var piece = GetPieceAt(position);
			return piece != null && piece.Color != friendlyColor;
		}

		public bool IsValidPosition(Vector2I position)
		{
			return position.X >= 0 && position.X < 8 && position.Y >= 0 && position.Y < 8;
		}

		public Vector2 BoardToWorldPosition(Vector2I boardPos)
		{
			const float squareSize = 64.0f;
			return new Vector2(boardPos.X * squareSize, boardPos.Y * squareSize);
		}

		public Vector2I WorldToBoardPosition(Vector2 worldPos)
		{
			const float squareSize = 64.0f;
			return new Vector2I(
				Mathf.FloorToInt(worldPos.X / squareSize),
				Mathf.FloorToInt(worldPos.Y / squareSize)
			);
		}

		public Army GetArmy(PieceColor color)
		{
			return color == PieceColor.White ? whiteArmy : blackArmy;
		}

		public ChessPlusPlus.UI.BoardVisual GetBoardVisual()
		{
			return boardVisual;
		}

		public void HighlightPossibleMoves(Piece piece)
		{
			if (piece == null)
				return;

			boardVisual.ClearHighlights();
			boardVisual.HighlightSelectedSquare(piece.BoardPosition);

			var possibleMoves = piece.GetPossibleMoves(this);
			GD.Print($"{piece.Color} {piece.Type} at {piece.BoardPosition} has {possibleMoves.Count} possible moves: [{string.Join(", ", possibleMoves)}]");
			boardVisual.HighlightValidMoves(possibleMoves, this);
		}

		public void ClearHighlights()
		{
			boardVisual.ClearHighlights();
		}

		public void PromotePawn(Pawn pawn, PieceType newPieceType)
		{
			var position = pawn.BoardPosition;
			var color = pawn.Color;
			var army = GetArmy(color);

			// Remove the pawn
			pieces[position.X, position.Y] = null;
			pawn.QueueFree();

			// Create the new piece
			Piece newPiece = newPieceType switch
			{
				PieceType.Queen => new Queen(),
				PieceType.Rook => new Rook(),
				PieceType.Bishop => new Bishop(),
				PieceType.Knight => new Knight(),
				_ => new Queen() // Default to Queen
			};

			newPiece.Color = color;
			newPiece.HasMoved = true; // Promoted pieces count as having moved
			PlacePiece(newPiece, position);
		}

		private void HandleCastling(King king, Vector2I from, Vector2I to)
		{
			bool isKingsideCastle = to.X > from.X;
			int rookFromX = isKingsideCastle ? 7 : 0;
			int rookToX = isKingsideCastle ? 5 : 3;
			int row = from.Y;

			// Move the rook
			var rook = GetPieceAt(new Vector2I(rookFromX, row));
			if (rook != null)
			{
				pieces[rookFromX, row] = null;
				pieces[rookToX, row] = rook;
				rook.BoardPosition = new Vector2I(rookToX, row);
				rook.Position = BoardToWorldPosition(new Vector2I(rookToX, row));
				rook.HasMoved = true;
			}
		}

		private bool IsMoveLegal(Vector2I from, Vector2I to, PieceColor movingPieceColor)
		{
			// Simulate the move to check if it leaves the king in check
			var movingPiece = GetPieceAt(from);
			var capturedPiece = GetPieceAt(to);

			// Temporarily make the move
			pieces[from.X, from.Y] = null;
			pieces[to.X, to.Y] = movingPiece;
			if (movingPiece != null)
				movingPiece.BoardPosition = to;

			// Check if the king of the moving color is in check after this move
			bool wouldBeInCheck = IsKingInCheck(movingPieceColor);

			// Restore the original position
			pieces[from.X, from.Y] = movingPiece;
			pieces[to.X, to.Y] = capturedPiece;
			if (movingPiece != null)
				movingPiece.BoardPosition = from;

			return !wouldBeInCheck;
		}

		public bool IsKingInCheck(PieceColor kingColor)
		{
			// Find the king of the given color
			King? king = null;
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = GetPieceAt(new Vector2I(x, y));
					if (piece is King k && k.Color == kingColor)
					{
						king = k;
						break;
					}
				}
				if (king != null)
					break;
			}

			if (king == null)
				return false;

			// Check if any enemy piece can attack the king
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = GetPieceAt(new Vector2I(x, y));
					if (piece != null && piece.Color != kingColor)
					{
						var moves = piece.GetPossibleMoves(this);
						if (moves.Contains(king.BoardPosition))
						{
							return true;
						}
					}
				}
			}

			return false;
		}
	}
}

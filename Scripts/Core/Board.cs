namespace ChessPlusPlus.Core
{
	using System;
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

		public void UpdatePiecePositions()
		{
			// Update all piece positions when board size changes
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = pieces[x, y];
					if (piece != null)
					{
						piece.Position = BoardToWorldPosition(new Vector2I(x, y));
					}
				}
			}
		}

		private void InitializeBoard()
		{
			// Create board visual
			boardVisual = new ChessPlusPlus.UI.BoardVisual();
			boardVisual.Name = "BoardVisual";
			AddChild(boardVisual);
			MoveChild(boardVisual, 0); // Put visual behind pieces

			// Check for custom army from GameConfig
			if (GameConfig.Instance.HasCustomArmy())
			{
				var customArmy = GameConfig.Instance.GetCustomArmy();
				if (GameConfig.Instance.PlayerColor == PieceColor.White)
				{
					whiteArmy = customArmy!;
					blackArmy = new Army(PieceColor.Black);
				}
				else
				{
					blackArmy = customArmy!;
					whiteArmy = new Army(PieceColor.White);
				}
				GD.Print($"Using custom army for {GameConfig.Instance.PlayerColor}");
			}
			else
			{
				whiteArmy = new Army(PieceColor.White);
				blackArmy = new Army(PieceColor.Black);
				GD.Print("Using standard armies");
			}
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
		}

		/// <summary>
		/// Checks if a move is valid without actually executing it
		/// </summary>
		public bool IsValidMove(Vector2I from, Vector2I to)
		{
			var piece = GetPieceAt(from);
			if (piece == null || !piece.CanMoveTo(to, this))
				return false;

			if (!IsMoveLegal(from, to, piece.Color))
				return false;

			return true;
		}

		/// <summary>
		/// Attempts to move a piece from one position to another, validating legality and handling special moves
		/// </summary>
		public bool MovePiece(Vector2I from, Vector2I to)
		{
			if (!IsValidMove(from, to))
				return false;

			var piece = GetPieceAt(from);

			var targetPiece = GetPieceAt(to);
			if (targetPiece != null)
			{
				if (piece.IsEnemyPiece(targetPiece))
				{
					// Check if the target can be captured from this direction
					if (!CanBeCapturedFrom(targetPiece, from, to))
					{
						return false;
					}
					CapturePiece(targetPiece, piece);
				}
				else
				{
					return false;
				}
			}

			pieces[from.X, from.Y] = null;
			pieces[to.X, to.Y] = piece;

			if (piece is King king && king.IsCastlingMove(to))
			{
				HandleCastling(king, from, to);
			}

			piece.OnMoved(from, to, this);
			piece.Position = BoardToWorldPosition(to);

			EmitSignal(SignalName.PieceMoved, piece, from, to);

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

		/// <summary>
		/// Converts board coordinates to world pixel coordinates, accounting for board orientation
		/// </summary>
		public Vector2 BoardToWorldPosition(Vector2I boardPos)
		{
			if (boardVisual == null)
				return new Vector2(boardPos.X * 64.0f, boardPos.Y * 64.0f);

			float squareSize = boardVisual.SquareSize;
			var displayPos = GameConfig.Instance.ShouldFlipBoard() ? FlipBoardPosition(boardPos) : boardPos;

			// Add the BoardVisual's position offset since it's now centered in the viewport
			return boardVisual.Position + new Vector2(displayPos.X * squareSize, displayPos.Y * squareSize);
		}

		public Vector2I WorldToBoardPosition(Vector2 worldPos)
		{
			if (boardVisual == null)
				return Vector2I.Zero;

			float squareSize = boardVisual.SquareSize;

			// Subtract the BoardVisual's position offset
			var relativePos = worldPos - boardVisual.Position;

			var displayPos = new Vector2I(
				Mathf.FloorToInt(relativePos.X / squareSize),
				Mathf.FloorToInt(relativePos.Y / squareSize)
			);
			return GameConfig.Instance.ShouldFlipBoard() ? FlipBoardPosition(displayPos) : displayPos;
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

			var possibleMoves = GetValidMovesForHighlighting(piece);
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

		/// <summary>
		/// Handles the special rook movement during castling
		/// </summary>
		private void HandleCastling(King king, Vector2I from, Vector2I to)
		{
			bool isKingsideCastle = to.X > from.X;
			int rookFromX = isKingsideCastle ? 7 : 0;
			int rookToX = isKingsideCastle ? 5 : 3;
			int row = from.Y;

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

		/// <summary>
		/// Simulates a move to ensure it doesn't leave the player's king in check
		/// </summary>
		private bool IsMoveLegal(Vector2I from, Vector2I to, PieceColor movingPieceColor)
		{
			var movingPiece = GetPieceAt(from);
			var capturedPiece = GetPieceAt(to);

			pieces[from.X, from.Y] = null;
			pieces[to.X, to.Y] = movingPiece;
			if (movingPiece != null)
				movingPiece.BoardPosition = to;

			bool wouldBeInCheck = IsKingInCheck(movingPieceColor);

			pieces[from.X, from.Y] = movingPiece;
			pieces[to.X, to.Y] = capturedPiece;
			if (movingPiece != null)
				movingPiece.BoardPosition = from;

			return !wouldBeInCheck;
		}

		/// <summary>
		/// Determines if the specified color's king is currently under attack
		/// </summary>
		public bool IsKingInCheck(PieceColor kingColor)
		{
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
		/// <summary>
		/// Flips board position vertically for when player chooses to play as white
		/// </summary>
		private Vector2I FlipBoardPosition(Vector2I position)
		{
			return new Vector2I(position.X, 7 - position.Y);
		}

		public Vector2I GetDisplayPosition(Vector2I boardPos)
		{
			return GameConfig.Instance.ShouldFlipBoard() ? FlipBoardPosition(boardPos) : boardPos;
		}

		/// <summary>
		/// Checks if a piece can be captured from a specific direction
		/// Used for special capture immunity mechanics like Guard Pawns
		/// </summary>
		public bool CanBeCapturedFrom(Piece target, Vector2I attackerFrom, Vector2I targetPos)
		{
			// Guard Pawns can't be captured from horizontal or vertical directions
			if (target is GuardPawn)
			{
				var delta = targetPos - attackerFrom;
				var absX = Math.Abs(delta.X);
				var absY = Math.Abs(delta.Y);

				// Check if movement is purely horizontal or vertical
				// Horizontal: Y doesn't change (delta.Y == 0)
				// Vertical: X doesn't change (delta.X == 0)
				if (absX == 0 || absY == 0)
				{
					return false; // Guard Pawn is immune to horizontal/vertical captures
				}
				// Diagonal movements are allowed (both X and Y change)
			}
			return true; // Normal pieces can be captured from any direction
		}

		/// <summary>
		/// Gets valid moves for highlighting, filtering out illegal captures
		/// </summary>
		private List<Vector2I> GetValidMovesForHighlighting(Piece piece)
		{
			var possibleMoves = piece.GetPossibleMoves(this);
			var validMoves = new List<Vector2I>();

			foreach (var move in possibleMoves)
			{
				var targetPiece = GetPieceAt(move);

				// If there's an enemy piece, check if it can actually be captured
				if (targetPiece != null && piece.IsEnemyPiece(targetPiece))
				{
					if (CanBeCapturedFrom(targetPiece, piece.BoardPosition, move))
					{
						validMoves.Add(move);
					}
					// Skip this move if the capture would be blocked
				}
				else if (targetPiece == null)
				{
					// Empty square - always valid
					validMoves.Add(move);
				}
				// Friendly pieces are already filtered out by GetPossibleMoves
			}

			return validMoves;
		}
	}
}

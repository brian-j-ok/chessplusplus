namespace ChessPlusPlus.Core.Managers
{
	using ChessPlusPlus.Pieces;
	using Godot;

	/// <summary>
	/// Manages game state transitions and evaluates game ending conditions
	/// </summary>
	public partial class GameStateManager : Node
	{
		public GameState CurrentState { get; private set; } = GameState.Setup;

		[Signal]
		public delegate void GameStateChangedEventHandler(GameState newState);

		/// <summary>
		/// Initializes the game state manager for a new game
		/// </summary>
		public void Initialize()
		{
			SetState(GameState.Setup);
		}

		/// <summary>
		/// Transitions the game to the playing state
		/// </summary>
		public void StartPlaying()
		{
			SetState(GameState.Playing);
		}

		/// <summary>
		/// Sets a new game state and emits the appropriate signal
		/// </summary>
		public void SetState(GameState newState)
		{
			if (CurrentState != newState)
			{
				CurrentState = newState;
				EmitSignal(SignalName.GameStateChanged, (int)newState);
			}
		}

		/// <summary>
		/// Evaluates the current board state and updates the game state accordingly
		/// </summary>
		public void EvaluateGameState(Board board, PieceColor currentTurn)
		{
			if (IsInCheck(board, currentTurn))
			{
				if (IsCheckmate(board, currentTurn))
				{
					SetState(GameState.Checkmate);
				}
				else
				{
					SetState(GameState.Check);
				}
			}
			else if (IsStalemate(board, currentTurn))
			{
				SetState(GameState.Stalemate);
			}
			else if (IsDraw(board))
			{
				SetState(GameState.Draw);
			}
			else
			{
				SetState(GameState.Playing);
			}
		}

		/// <summary>
		/// Checks if the specified color's king is in check
		/// </summary>
		public bool IsInCheck(Board board, PieceColor color)
		{
			return board.IsKingInCheck(color);
		}

		/// <summary>
		/// Determines if the current position is checkmate
		/// </summary>
		private bool IsCheckmate(Board board, PieceColor color)
		{
			if (!IsInCheck(board, color))
				return false;

			// Check if any legal move exists that gets out of check
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = board.GetPieceAt(new Vector2I(x, y));
					if (piece != null && piece.Color == color)
					{
						var possibleMoves = piece.GetPossibleMoves(board);
						foreach (var move in possibleMoves)
						{
							if (board.IsValidMove(piece.BoardPosition, move))
							{
								// At least one legal move exists
								return false;
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Determines if the current position is stalemate
		/// </summary>
		private bool IsStalemate(Board board, PieceColor color)
		{
			if (IsInCheck(board, color))
				return false;

			// Check if any legal move exists
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = board.GetPieceAt(new Vector2I(x, y));
					if (piece != null && piece.Color == color)
					{
						var possibleMoves = piece.GetPossibleMoves(board);
						foreach (var move in possibleMoves)
						{
							if (board.IsValidMove(piece.BoardPosition, move))
							{
								// At least one legal move exists
								return false;
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Checks for draw conditions (insufficient material, etc.)
		/// </summary>
		private bool IsDraw(Board board)
		{
			// Check for insufficient material
			var whitePieces = new System.Collections.Generic.List<Piece>();
			var blackPieces = new System.Collections.Generic.List<Piece>();

			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = board.GetPieceAt(new Vector2I(x, y));
					if (piece != null)
					{
						if (piece.Color == PieceColor.White)
							whitePieces.Add(piece);
						else
							blackPieces.Add(piece);
					}
				}
			}

			// King vs King
			if (whitePieces.Count == 1 && blackPieces.Count == 1)
				return true;

			// King and Bishop vs King or King and Knight vs King
			if (
				(whitePieces.Count == 2 && blackPieces.Count == 1) || (whitePieces.Count == 1 && blackPieces.Count == 2)
			)
			{
				var twoP = whitePieces.Count == 2 ? whitePieces : blackPieces;
				var nonKing = twoP.Find(p => p.Type != PieceType.King);
				if (nonKing != null && (nonKing.Type == PieceType.Bishop || nonKing.Type == PieceType.Knight))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Handles a timeout event
		/// </summary>
		public void HandleTimeout(PieceColor timedOutColor)
		{
			SetState(GameState.Draw); // Or could be a win for the other player
		}

		/// <summary>
		/// Checks if the game is in a terminal state
		/// </summary>
		public bool IsGameOver()
		{
			return CurrentState == GameState.Checkmate
				|| CurrentState == GameState.Stalemate
				|| CurrentState == GameState.Draw;
		}
	}
}

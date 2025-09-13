namespace ChessPlusPlus.Core
{
	using ChessPlusPlus.Pieces;
	using Godot;
	public enum GameState
	{
		Setup,
		Playing,
		Check,
		Checkmate,
		Stalemate,
		Draw
	}

	public partial class GameManager : Node2D
	{
		[Export] public Board Board { get; set; } = null!;

		public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
		public GameState State { get; private set; } = GameState.Setup;

		private Piece? selectedPiece;
		private Vector2I selectedPosition;
		private ChessPlusPlus.UI.PromotionDialog? promotionDialog;

		// Timer properties
		private float whiteTimeRemaining = 600.0f; // 10 minutes in seconds
		private float blackTimeRemaining = 600.0f; // 10 minutes in seconds
		private bool timersRunning = false;

		[Signal]
		public delegate void TurnChangedEventHandler(PieceColor newTurn);

		[Signal]
		public delegate void GameStateChangedEventHandler(GameState newState);

		[Signal]
		public delegate void PieceSelectedEventHandler(Piece piece);

		[Signal]
		public delegate void TimerUpdatedEventHandler(float whiteTime, float blackTime);

		public override void _Ready()
		{
			GD.Print("GameManager _Ready called");
			if (Board == null)
			{
				Board = GetNode<Board>("Board");
				GD.Print($"Board found: {Board != null}");
			}

			Board.PieceMoved += OnPieceMoved;
			Board.PieceCaptured += OnPieceCaptured;
			Board.PawnPromotion += OnPawnPromotion;

			// Create promotion dialog
			promotionDialog = new ChessPlusPlus.UI.PromotionDialog();
			promotionDialog.PieceSelected += OnPromotionPieceSelected;
			AddChild(promotionDialog);

			StartNewGame();
		}

		public void StartNewGame()
		{
			State = GameState.Setup;
			CurrentTurn = PieceColor.White;

			// Reset timers
			whiteTimeRemaining = 600.0f;
			blackTimeRemaining = 600.0f;
			timersRunning = true;

			Board.SetupStandardBoard();

			// Refresh board orientation after player has chosen their color
			var boardVisual = Board.GetBoardVisual();
			if (boardVisual != null)
			{
				boardVisual.RefreshBoardOrientation();
			}

			// If player is playing as black and it's the start of the game,
			// let the AI make the first move or switch turns
			if (!GameConfig.Instance.IsPlayerWhite())
			{
				GD.Print("Player is playing as Black - White will move first");
			}

			State = GameState.Playing;
			EmitSignal(SignalName.GameStateChanged, (int)State);
			EmitSignal(SignalName.TurnChanged, (int)CurrentTurn);
			EmitSignal(SignalName.TimerUpdated, whiteTimeRemaining, blackTimeRemaining);
		}

		public void StartCustomGame(Army whiteArmy, Army blackArmy)
		{
			State = GameState.Setup;
			CurrentTurn = PieceColor.White;

			// Reset timers
			whiteTimeRemaining = 600.0f;
			blackTimeRemaining = 600.0f;
			timersRunning = true;

			State = GameState.Playing;
			EmitSignal(SignalName.GameStateChanged, (int)State);
			EmitSignal(SignalName.TurnChanged, (int)CurrentTurn);
			EmitSignal(SignalName.TimerUpdated, whiteTimeRemaining, blackTimeRemaining);
		}

		public override void _Process(double delta)
		{
			if (timersRunning && State == GameState.Playing)
			{
				// Update the current player's timer
				if (CurrentTurn == PieceColor.White)
				{
					whiteTimeRemaining -= (float)delta;
					if (whiteTimeRemaining <= 0)
					{
						whiteTimeRemaining = 0;
						timersRunning = false;
						State = GameState.Draw; // Time out results in loss/draw
						EmitSignal(SignalName.GameStateChanged, (int)State);
					}
				}
				else
				{
					blackTimeRemaining -= (float)delta;
					if (blackTimeRemaining <= 0)
					{
						blackTimeRemaining = 0;
						timersRunning = false;
						State = GameState.Draw; // Time out results in loss/draw
						EmitSignal(SignalName.GameStateChanged, (int)State);
					}
				}

				// Emit timer update signal
				EmitSignal(SignalName.TimerUpdated, whiteTimeRemaining, blackTimeRemaining);
			}
		}

		public override void _Input(InputEvent @event)
		{
			if (State != GameState.Playing && State != GameState.Check)
				return;

			if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
			{
				if (mouseButton.ButtonIndex == MouseButton.Left)
				{
					HandleBoardClick(mouseButton.Position);
				}
				else if (mouseButton.ButtonIndex == MouseButton.Right)
				{
					ClearSelection();
				}
			}
		}

		/// <summary>
		/// Handles player clicks on the chess board for piece selection and movement
		/// </summary>
		private void HandleBoardClick(Vector2 clickPosition)
		{
			var boardPos = Board.WorldToBoardPosition(Board.ToLocal(clickPosition));

			if (!Board.IsValidPosition(boardPos))
				return;

			var clickedPiece = Board.GetPieceAt(boardPos);

			if (selectedPiece == null)
			{
				// Allow selecting any piece that belongs to the current turn (both sides playable)
				if (clickedPiece != null && clickedPiece.Color == CurrentTurn)
				{
					SelectPiece(clickedPiece, boardPos);
				}
			}
			else
			{
				// Allow selecting another piece of the current turn
				if (clickedPiece != null && clickedPiece.Color == CurrentTurn)
				{
					SelectPiece(clickedPiece, boardPos);
				}
				else
				{
					TryMovePiece(boardPos);
				}
			}
		}

		private void SelectPiece(Piece piece, Vector2I position)
		{
			selectedPiece = piece;
			selectedPosition = position;
			EmitSignal(SignalName.PieceSelected, piece);

			HighlightPossibleMoves(piece);
		}

		private void HighlightPossibleMoves(Piece piece)
		{
			Board.HighlightPossibleMoves(piece);
		}

		private void TryMovePiece(Vector2I targetPosition)
		{
			if (Board.MovePiece(selectedPosition, targetPosition))
			{
				ClearSelection();
				EndTurn();
			}
		}

		private void ClearSelection()
		{
			selectedPiece = null;
			selectedPosition = new Vector2I(-1, -1);
			Board.ClearHighlights();
		}

		private void EndTurn()
		{
			CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
			EmitSignal(SignalName.TurnChanged, (int)CurrentTurn);

			CheckGameState();
		}

		private void CheckGameState()
		{
			if (IsInCheck(CurrentTurn))
			{
				if (IsCheckmate(CurrentTurn))
				{
					State = GameState.Checkmate;
				}
				else
				{
					State = GameState.Check;
				}
			}
			else if (IsStalemate(CurrentTurn))
			{
				State = GameState.Stalemate;
			}
			else
			{
				State = GameState.Playing;
			}

			EmitSignal(SignalName.GameStateChanged, (int)State);
		}

		private bool IsInCheck(PieceColor color)
		{
			return Board.IsKingInCheck(color);
		}

		/// <summary>
		/// TODO: Implement proper checkmate detection
		/// </summary>
		private bool IsCheckmate(PieceColor color)
		{
			return false;
		}

		/// <summary>
		/// TODO: Implement proper stalemate detection
		/// </summary>
		private bool IsStalemate(PieceColor color)
		{
			return false;
		}

		private void OnPieceMoved(Piece piece, Vector2I from, Vector2I to)
		{
			GD.Print($"{piece.Color} {piece.Type} moved from {from} to {to}");
		}

		private void OnPieceCaptured(Piece captured, Piece capturer)
		{
			GD.Print($"{capturer.Color} {capturer.Type} captured {captured.Color} {captured.Type}");
		}

		private Pawn? promotingPawn;

		private void OnPawnPromotion(Pawn pawn, Vector2I position)
		{
			promotingPawn = pawn;
			State = GameState.Setup; // Pause the game during promotion
			promotionDialog?.ShowPromotionDialog(pawn.Color);
		}

		private void OnPromotionPieceSelected(int pieceTypeInt)
		{
			if (promotingPawn == null)
				return;

			var pieceType = (PieceType)pieceTypeInt;
			Board.PromotePawn(promotingPawn, pieceType);

			promotingPawn = null;
			State = GameState.Playing;
			EndTurn();
		}
	}
}

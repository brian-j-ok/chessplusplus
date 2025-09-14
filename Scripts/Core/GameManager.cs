namespace ChessPlusPlus.Core
{
	using System.Threading.Tasks;
	using ChessPlusPlus.Network;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	public enum GameState
	{
		Setup,
		Playing,
		Check,
		Checkmate,
		Stalemate,
		Draw,
	}

	public partial class GameManager : Node2D
	{
		[Export]
		public Board Board { get; set; } = null!;

		public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
		public GameState State { get; private set; } = GameState.Setup;

		private PlayerController? whitePlayer;
		private PlayerController? blackPlayer;
		private PlayerController? currentPlayer;
		private HumanPlayerController? humanPlayer;
		private ChessPlusPlus.UI.PromotionDialog? promotionDialog;
		private bool isProcessingTurn = false;

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

			InitializePlayers();
			StartNewGame();
		}

		private void InitializePlayers()
		{
			// Clean up existing players
			if (whitePlayer != null)
			{
				whitePlayer.QueueFree();
				whitePlayer = null;
			}
			if (blackPlayer != null)
			{
				blackPlayer.QueueFree();
				blackPlayer = null;
			}

			var config = GameConfig.Instance;
			GD.Print($"Initializing players for mode: {config.Mode}");

			// Check if we're in network mode
			var networkManager = NetworkManager.Instance;
			if (networkManager.IsConnected)
			{
				GD.Print("Setting up network game");
				// Network game: one local, one remote
				if (config.PlayerColor == PieceColor.White)
				{
					whitePlayer = CreateNetworkPlayer(PieceColor.White, true); // Local
					blackPlayer = CreateNetworkPlayer(PieceColor.Black, false); // Remote
				}
				else
				{
					whitePlayer = CreateNetworkPlayer(PieceColor.White, false); // Remote
					blackPlayer = CreateNetworkPlayer(PieceColor.Black, true); // Local
				}
				// Initialize already called in CreateNetworkPlayer
				return;
			}

			switch (config.Mode)
			{
				case GameMode.PlayerVsPlayer:
					// Dev mode: both sides controlled by same human player
					whitePlayer = CreateHumanPlayer(PieceColor.White);
					blackPlayer = CreateHumanPlayer(PieceColor.Black);
					// Set humanPlayer to the current turn's controller
					humanPlayer = whitePlayer as HumanPlayerController;
					break;

				case GameMode.PlayerVsAI:
					if (config.PlayerColor == PieceColor.White)
					{
						whitePlayer = CreateHumanPlayer(PieceColor.White);
						blackPlayer = CreateAIPlayer(PieceColor.Black, config.AIDifficulty);
						humanPlayer = whitePlayer as HumanPlayerController;
					}
					else
					{
						whitePlayer = CreateAIPlayer(PieceColor.White, config.AIDifficulty);
						blackPlayer = CreateHumanPlayer(PieceColor.Black);
						humanPlayer = blackPlayer as HumanPlayerController;
					}
					break;

				case GameMode.AIVsAI:
					whitePlayer = CreateAIPlayer(PieceColor.White, config.AIDifficulty);
					blackPlayer = CreateAIPlayer(PieceColor.Black, config.AIDifficulty);
					humanPlayer = null;
					break;
			}

			whitePlayer?.Initialize(Board, this);
			blackPlayer?.Initialize(Board, this);
		}

		private HumanPlayerController CreateHumanPlayer(PieceColor color)
		{
			var player = new HumanPlayerController();
			player.PlayerColor = color;
			player.PlayerName = $"Human ({color})";
			AddChild(player);
			return player;
		}

		private AIPlayerController CreateAIPlayer(PieceColor color, AIDifficulty difficulty)
		{
			var player = new AIPlayerController();
			player.PlayerColor = color;
			player.Difficulty = difficulty;
			player.PlayerName = $"AI ({color}, {difficulty})";
			AddChild(player);
			return player;
		}

		private PlayerController CreateNetworkPlayer(PieceColor color, bool isLocal)
		{
			var player = new LocalNetworkPlayerController();
			player.PlayerColor = color;
			player.IsLocalPlayer = isLocal;
			player.PlayerName = isLocal ? $"You ({color})" : $"Opponent ({color})";
			AddChild(player);
			player.Initialize(Board, this);

			// For local network players, add to humanPlayer for input handling
			if (isLocal)
			{
				humanPlayer = player;
			}

			return player;
		}

		public async void StartNewGame()
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

			// Start the first turn
			await ProcessNextTurn();
		}

		public async void StartCustomGame(Army whiteArmy, Army blackArmy)
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

			// Start the first turn
			await ProcessNextTurn();
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
				// In dev mode, route to the current player
				if (GameConfig.Instance.Mode == GameMode.PlayerVsPlayer)
				{
					var currentHumanPlayer =
						CurrentTurn == PieceColor.White
							? whitePlayer as HumanPlayerController
							: blackPlayer as HumanPlayerController;

					if (mouseButton.ButtonIndex == MouseButton.Left)
					{
						currentHumanPlayer?.HandleBoardClick(mouseButton.Position);
					}
					else if (mouseButton.ButtonIndex == MouseButton.Right)
					{
						currentHumanPlayer?.ClearSelection();
					}
				}
				else
				{
					// In other modes, route to the single human player
					if (mouseButton.ButtonIndex == MouseButton.Left)
					{
						humanPlayer?.HandleBoardClick(mouseButton.Position);
					}
					else if (mouseButton.ButtonIndex == MouseButton.Right)
					{
						humanPlayer?.ClearSelection();
					}
				}
			}
		}

		private async Task ProcessNextTurn()
		{
			if (
				isProcessingTurn
				|| State == GameState.Checkmate
				|| State == GameState.Stalemate
				|| State == GameState.Draw
			)
				return;

			isProcessingTurn = true;

			currentPlayer = CurrentTurn == PieceColor.White ? whitePlayer : blackPlayer;

			if (currentPlayer == null)
			{
				GD.PrintErr($"No player controller for {CurrentTurn}");
				isProcessingTurn = false;
				return;
			}

			GD.Print($"Processing turn for {CurrentTurn}");
			currentPlayer.OnTurnStarted();

			// Get the move from the current player
			var move = await currentPlayer.GetNextMoveAsync();

			if (move != null)
			{
				GD.Print($"Executing move from {move.Value.From} to {move.Value.To}");
				// Execute the move
				if (Board.MovePiece(move.Value.From, move.Value.To))
				{
					isProcessingTurn = false; // Reset flag before ending turn
					EndTurn();
				}
				else
				{
					GD.PrintErr($"Failed to execute move from {move.Value.From} to {move.Value.To}");
					isProcessingTurn = false;
				}
			}
			else
			{
				GD.Print("No move returned from player");
				isProcessingTurn = false;
			}
		}

		private async void EndTurn()
		{
			currentPlayer?.OnTurnEnded();

			CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
			EmitSignal(SignalName.TurnChanged, (int)CurrentTurn);

			CheckGameState();

			// Continue to next turn if game is still active
			if (State == GameState.Playing || State == GameState.Check)
			{
				await ProcessNextTurn();
			}
			else
			{
				// Notify players that the game has ended
				whitePlayer?.OnGameEnded(State);
				blackPlayer?.OnGameEnded(State);
			}
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

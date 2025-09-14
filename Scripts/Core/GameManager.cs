namespace ChessPlusPlus.Core
{
	using System.Threading.Tasks;
	using ChessPlusPlus.Core.Managers;
	using ChessPlusPlus.Network;
	using ChessPlusPlus.Pieces;
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

	/// <summary>
	/// Coordinates between different game managers and handles overall game flow
	/// </summary>
	public partial class GameManager : Node2D
	{
		[Export]
		public Board Board { get; set; } = null!;

		// Managers
		private TurnManager turnManager = null!;
		private TimerManager timerManager = null!;
		private GameStateManager gameStateManager = null!;
		private PlayerManager playerManager = null!;
		private InputRouter inputRouter = null!;
		private NetworkStateManager? networkStateManager;
		private NetworkManager? networkManager;

		// Network sync timer
		private float networkSyncTimer = 0.0f;
		private const float NetworkSyncInterval = 0.5f; // Sync every 500ms

		// UI Components
		private ChessPlusPlus.UI.PromotionDialog? promotionDialog;

		// Public access to managers for components that need them
		public TurnManager TurnManager => turnManager;
		public TimerManager TimerManager => timerManager;
		public GameStateManager GameStateManager => gameStateManager;
		public PlayerManager PlayerManager => playerManager;

		[Signal]
		public delegate void PieceSelectedEventHandler(Piece piece);

		public override void _Ready()
		{
			GD.Print("GameManager _Ready called");
			if (Board == null)
			{
				Board = GetNode<Board>("Board");
				GD.Print($"Board found: {Board != null}");
			}

			// Initialize managers
			InitializeManagers();

			// Setup board signals
			Board.PieceMoved += OnPieceMoved;
			Board.PieceCaptured += OnPieceCaptured;
			Board.PawnPromotion += OnPawnPromotion;

			// Create promotion dialog
			promotionDialog = new ChessPlusPlus.UI.PromotionDialog();
			promotionDialog.PieceSelected += OnPromotionPieceSelected;
			AddChild(promotionDialog);

			// Initialize players
			playerManager.InitializePlayers(Board, this);

			// Check if this is a network game that should wait for synchronization
			if (networkManager != null && networkManager.IsConnected)
			{
				GD.Print("Network game detected - checking for LAN armies");
				// Check if armies have already been set (game started from LANSetupScreen)
				if (GameConfig.Instance.HasLANArmies())
				{
					var whiteArmy = GameConfig.Instance.GetLANWhiteArmy();
					var blackArmy = GameConfig.Instance.GetLANBlackArmy();

					// Validate armies are not null
					if (whiteArmy != null && blackArmy != null)
					{
						GD.Print("LAN armies found - starting game with custom armies");
						StartCustomGame(whiteArmy, blackArmy);
					}
					else
					{
						GD.PrintErr("LAN armies were null! Using standard armies as fallback");
						StartNewGame();
					}
				}
				else
				{
					GD.PrintErr("No LAN armies configured! Using standard armies as fallback");
					// Start with standard armies instead of waiting
					StartNewGame();
				}
			}
			else
			{
				// Single player or local game - start immediately
				StartNewGame();
			}
		}

		/// <summary>
		/// Initializes all manager components
		/// </summary>
		private void InitializeManagers()
		{
			// Create and add managers as children
			turnManager = new TurnManager();
			AddChild(turnManager);
			turnManager.TurnChanged += OnTurnChanged;

			timerManager = new TimerManager();
			AddChild(timerManager);
			timerManager.TimerUpdated += OnTimerUpdated;
			timerManager.TimeExpired += OnTimeExpired;

			gameStateManager = new GameStateManager();
			AddChild(gameStateManager);
			gameStateManager.GameStateChanged += OnGameStateChanged;

			playerManager = new PlayerManager();
			AddChild(playerManager);

			inputRouter = new InputRouter();
			AddChild(inputRouter);
			inputRouter.Initialize(playerManager, gameStateManager, turnManager);

			// Initialize network components if in network mode
			InitializeNetworking();
		}

		/// <summary>
		/// Initializes networking components if in a network game
		/// </summary>
		private void InitializeNetworking()
		{
			networkManager = NetworkManager.Instance;

			// Check if we're in a network game
			if (networkManager != null && networkManager.IsConnected)
			{
				GD.Print($"Initializing network state manager. IsHost: {networkManager.IsHost}");

				// Create network state manager
				networkStateManager = new NetworkStateManager();
				AddChild(networkStateManager);
				networkStateManager.Initialize(Board, this, networkManager.IsHost);

				// Connect network events
				networkManager.GameStateReceived += OnNetworkStateReceived;
				networkManager.MoveValidationRequested += OnMoveValidationRequested;
				networkManager.MoveValidationReceived += OnMoveValidationReceived;

				// If we're the host, send initial state after a short delay
				if (networkManager.IsHost)
				{
					GetTree().CreateTimer(0.5).Timeout += () =>
					{
						networkStateManager?.BroadcastState();
					};
				}
			}
		}

		public async void StartNewGame()
		{
			// Initialize managers for new game
			gameStateManager.Initialize();
			turnManager.Initialize();
			timerManager.Initialize();

			// Setup board
			Board.SetupStandardBoard();

			// Refresh board orientation
			var boardVisual = Board.GetBoardVisual();
			if (boardVisual != null)
			{
				boardVisual.RefreshBoardOrientation();
			}

			if (!GameConfig.Instance.IsPlayerWhite())
			{
				GD.Print("Player is playing as Black - White will move first");
			}

			// Start playing
			gameStateManager.StartPlaying();

			// Start the first turn
			await ProcessNextTurn();
		}

		public async void StartCustomGame(Army whiteArmy, Army blackArmy)
		{
			// Initialize managers for new game
			gameStateManager.Initialize();
			turnManager.Initialize();
			timerManager.Initialize();

			// Setup custom armies on board
			Board.SetupCustomBoard(whiteArmy, blackArmy);

			// Refresh board orientation
			var boardVisual = Board.GetBoardVisual();
			if (boardVisual != null)
			{
				boardVisual.RefreshBoardOrientation();
			}

			// Start playing
			gameStateManager.StartPlaying();

			// Start the first turn
			await ProcessNextTurn();
		}

		public override void _Process(double delta)
		{
			// Update timers
			if (timerManager != null && turnManager != null && gameStateManager != null)
			{
				timerManager.Update((float)delta, turnManager.CurrentTurn, gameStateManager.CurrentState);
			}

			// Handle network state sync for host
			if (networkManager != null && networkManager.IsHost && networkStateManager != null)
			{
				networkSyncTimer += (float)delta;
				if (networkSyncTimer >= NetworkSyncInterval)
				{
					networkSyncTimer = 0.0f;
					networkStateManager.BroadcastState();
				}
			}
		}

		public override void _Input(InputEvent @event)
		{
			// Route input through the input router
			inputRouter.HandleInput(@event);

			// Handle keyboard input for additional controls
			if (@event is InputEventKey keyEvent)
			{
				inputRouter.HandleKeyboardInput(keyEvent);
			}
		}

		private async Task ProcessNextTurn()
		{
			if (gameStateManager.IsGameOver())
				return;

			var moveExecuted = await turnManager.ProcessNextTurn(
				playerManager.WhitePlayer,
				playerManager.BlackPlayer,
				Board,
				gameStateManager.CurrentState
			);

			if (moveExecuted)
			{
				// Check if the piece that moved needs more moves this turn
				var stateManager = Board.GetStateManager();
				if (stateManager != null)
				{
					var lastMovedPiece = stateManager.GetLastMovedPiece(turnManager.CurrentTurn);
					if (lastMovedPiece != null && stateManager.NeedsAnotherMove(lastMovedPiece))
					{
						GD.Print($"{lastMovedPiece.Color} {lastMovedPiece.Type} needs to move again!");
						// Continue with the same player's turn
						await ProcessNextTurn();
						return;
					}
				}

				// All moves complete for this turn - check for auto-captures before ending turn
				Board.ProcessAutoCaptures();

				EndTurn();
			}
		}

		private async void EndTurn()
		{
			// End the current turn
			turnManager.EndTurn(Board);

			// Add timer increment if applicable
			var previousTurn = turnManager.CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
			timerManager.AddIncrement(previousTurn);

			// Evaluate game state
			gameStateManager.EvaluateGameState(Board, turnManager.CurrentTurn);

			// Continue to next turn if game is still active
			if (!gameStateManager.IsGameOver())
			{
				await ProcessNextTurn();
			}
			else
			{
				// Notify players that the game has ended
				turnManager.NotifyGameEnded(
					playerManager.WhitePlayer,
					playerManager.BlackPlayer,
					gameStateManager.CurrentState
				);
				timerManager.StopTimers();
			}
		}

		// Signal handlers for managers
		private void OnTurnChanged(PieceColor newTurn)
		{
			GD.Print($"Turn changed to {newTurn}");
		}

		private void OnTimerUpdated(float whiteTime, float blackTime)
		{
			// Timer updates are handled by UI connecting directly to TimerManager
		}

		private void OnTimeExpired(PieceColor color)
		{
			GD.Print($"{color} has run out of time");
			gameStateManager.HandleTimeout(color);
		}

		private void OnGameStateChanged(GameState newState)
		{
			GD.Print($"Game state changed to {newState}");
		}

		private void OnPieceMoved(Piece piece, Vector2I from, Vector2I to)
		{
			GD.Print($"{piece.Color} {piece.Type} moved from {from} to {to}");

			// Handle network sync if in a network game
			OnPieceMovedNetwork(piece, from, to);
		}

		private void OnPieceCaptured(Piece captured, Piece capturer)
		{
			GD.Print($"{capturer.Color} {capturer.Type} captured {captured.Color} {captured.Type}");
		}

		private Pawn? promotingPawn;

		private void OnPawnPromotion(Pawn pawn, Vector2I position)
		{
			promotingPawn = pawn;
			gameStateManager.SetState(GameState.Setup); // Pause the game during promotion
			timerManager.PauseTimers();
			promotionDialog?.ShowPromotionDialog(pawn.Color);
		}

		private void OnPromotionPieceSelected(int pieceTypeInt)
		{
			if (promotingPawn == null)
				return;

			var pieceType = (PieceType)pieceTypeInt;
			Board.PromotePawn(promotingPawn, pieceType);

			promotingPawn = null;
			gameStateManager.SetState(GameState.Playing);
			timerManager.ResumeTimers();
			EndTurn();
		}

		// Network event handlers
		private void OnNetworkStateReceived(string serializedState)
		{
			if (networkStateManager != null && !networkManager!.IsHost)
			{
				// Clients apply the state from host
				networkStateManager.OnNetworkStateReceived(serializedState);
			}
		}

		private void OnMoveValidationRequested(Vector2I from, Vector2I to, int peerId)
		{
			if (networkManager != null && networkManager.IsHost && networkStateManager != null)
			{
				// Host validates the move
				var playerColor = peerId == 1 ? PieceColor.White : PieceColor.Black; // Simplified - need proper mapping
				bool isValid = networkStateManager.ValidateMove(from, to, playerColor);
				networkManager.SendMoveValidationResult(peerId, isValid, from, to);

				if (isValid)
				{
					// Execute the move if valid
					Board.MovePiece(from, to);
				}
			}
		}

		private void OnMoveValidationReceived(bool isValid, Vector2I from, Vector2I to)
		{
			if (!isValid)
			{
				GD.Print($"Move from {from} to {to} was rejected by host");
				// TODO: Rollback the move or prevent it from executing
			}
		}

		/// <summary>
		/// Called after a piece moves to handle network sync
		/// </summary>
		private void OnPieceMovedNetwork(Piece piece, Vector2I from, Vector2I to)
		{
			// If we're the host, broadcast the new state after the move
			if (networkManager != null && networkManager.IsHost && networkStateManager != null)
			{
				networkStateManager.BroadcastState();
			}
		}
	}
}

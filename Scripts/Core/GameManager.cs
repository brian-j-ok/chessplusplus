namespace ChessPlusPlus.Core
{
	using System.Threading.Tasks;
	using ChessPlusPlus.Core.Managers;
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

			// Initialize players and start game
			playerManager.InitializePlayers(Board, this);
			StartNewGame();
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

			// TODO: Setup custom armies on board

			// Start playing
			gameStateManager.StartPlaying();

			// Start the first turn
			await ProcessNextTurn();
		}

		public override void _Process(double delta)
		{
			// Update timers
			timerManager.Update((float)delta, turnManager.CurrentTurn, gameStateManager.CurrentState);
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
				EndTurn();
			}
		}

		private async void EndTurn()
		{
			// End the current turn
			turnManager.EndTurn();

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
	}
}

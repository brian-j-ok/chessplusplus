namespace ChessPlusPlus.Network
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.Json;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Core.Abilities;
	using ChessPlusPlus.Pieces;
	using Godot;

	/// <summary>
	/// Manages the complete game state for network synchronization
	/// </summary>
	public partial class NetworkStateManager : Node
	{
		/// <summary>
		/// Represents a complete snapshot of the game state
		/// </summary>
		public class GameStateSnapshot
		{
			public class PieceInfo
			{
				public string Type { get; set; } = "";
				public string ClassName { get; set; } = "";
				public int Color { get; set; }
				public int X { get; set; }
				public int Y { get; set; }
				public bool HasMoved { get; set; }
				public int MovesThisTurn { get; set; }
				public bool IsFrozen { get; set; }
				public Dictionary<string, object> CustomData { get; set; } = new();
			}

			public class TimerInfo
			{
				public float WhiteTime { get; set; }
				public float BlackTime { get; set; }
				public bool IsWhiteTicking { get; set; }
				public bool IsBlackTicking { get; set; }
			}

			// Core game state
			public List<PieceInfo> Pieces { get; set; } = new();
			public int CurrentTurn { get; set; } // PieceColor as int
			public int GameState { get; set; } // GameState enum as int
			public TimerInfo Timers { get; set; } = new();

			// Move tracking
			public int MoveNumber { get; set; }
			public string? LastMove { get; set; } // Serialized move info

			// Special states
			public bool IsCheck { get; set; }
			public bool IsCheckmate { get; set; }
			public bool IsStalemate { get; set; }
			public bool IsDraw { get; set; }

			// Multi-move tracking
			public string? ActiveMultiMovePiece { get; set; }
			public int MultiMoveCount { get; set; }
			public int MultiMoveTotal { get; set; }

			// Army configuration (for initial sync)
			public string? WhiteArmyConfig { get; set; }
			public string? BlackArmyConfig { get; set; }

			// Timestamp for synchronization
			public long Timestamp { get; set; }
		}

		private Board board = null!;
		private GameManager gameManager = null!;
		private NetworkManager networkManager = null!;
		private GameStateSnapshot? lastSentSnapshot;
		private GameStateSnapshot? lastReceivedSnapshot;

		// Authority tracking
		private bool isAuthoritative = false;
		private Queue<GameStateSnapshot> pendingStates = new();

		[Signal]
		public delegate void StateReceivedEventHandler();

		[Signal]
		public delegate void StateConflictEventHandler();

		public void Initialize(Board gameBoard, GameManager manager, bool isHost)
		{
			board = gameBoard;
			gameManager = manager;
			networkManager = NetworkManager.Instance;
			isAuthoritative = isHost;

			GD.Print($"NetworkStateManager initialized. Authoritative: {isAuthoritative}");
		}

		/// <summary>
		/// Creates a complete snapshot of the current game state
		/// </summary>
		public GameStateSnapshot CaptureSnapshot()
		{
			var snapshot = new GameStateSnapshot
			{
				Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
				CurrentTurn = (int)gameManager.TurnManager.CurrentTurn,
				GameState = (int)gameManager.GameStateManager.CurrentState,
				MoveNumber = GetMoveNumber(),
			};

			// Capture piece positions and states
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var piece = board.GetPieceAt(new Vector2I(x, y));
					if (piece != null)
					{
						var pieceInfo = new GameStateSnapshot.PieceInfo
						{
							Type = piece.Type.ToString(),
							ClassName = piece.ClassName,
							Color = (int)piece.Color,
							X = x,
							Y = y,
							HasMoved = piece.HasMoved,
						};

						// Capture piece-specific state
						var stateManager = board.GetBoardStateManager();
						if (stateManager != null)
						{
							var pieceState = stateManager.GetPieceState(piece);
							pieceInfo.MovesThisTurn = pieceState.MovesThisTurn;
							pieceInfo.IsFrozen = pieceState.IsFrozen;

							// Store custom states for special pieces
							if (piece is IMultiMoveAbility multiMove)
							{
								pieceInfo.CustomData["RequiresDoubleMove"] = pieceState.RequiresDoubleMove;
								pieceInfo.CustomData["MovesPerTurn"] = multiMove.MovesPerTurn;
							}

							if (piece is GlassQueen glassQueen)
							{
								pieceInfo.CustomData["PendingAutoCapture"] = pieceState.PendingAutoCapture;
							}
						}

						snapshot.Pieces.Add(pieceInfo);
					}
				}
			}

			// Capture timer state
			var timerManager = gameManager.TimerManager;
			if (timerManager != null)
			{
				snapshot.Timers = new GameStateSnapshot.TimerInfo
				{
					WhiteTime = timerManager.GetRemainingTime(PieceColor.White),
					BlackTime = timerManager.GetRemainingTime(PieceColor.Black),
					IsWhiteTicking = timerManager.IsTimerActive(PieceColor.White),
					IsBlackTicking = timerManager.IsTimerActive(PieceColor.Black),
				};
			}

			// Capture game state flags
			var gameStateManager = gameManager.GameStateManager;
			if (gameStateManager != null)
			{
				snapshot.IsCheck = gameStateManager.CurrentState == GameState.Check;
				snapshot.IsCheckmate = gameStateManager.CurrentState == GameState.Checkmate;
				snapshot.IsStalemate = gameStateManager.CurrentState == GameState.Stalemate;
				snapshot.IsDraw = gameStateManager.CurrentState == GameState.Draw;
			}

			// Track multi-move state
			var boardStateManager = board.GetBoardStateManager();
			if (boardStateManager != null)
			{
				var lastMovedPiece = boardStateManager.GetLastMovedPiece(gameManager.TurnManager.CurrentTurn);
				if (lastMovedPiece != null && lastMovedPiece is IMultiMoveAbility multiMovePiece)
				{
					var pieceState = boardStateManager.GetPieceState(lastMovedPiece);
					if (!multiMovePiece.CanEndTurn(pieceState.MovesThisTurn))
					{
						snapshot.ActiveMultiMovePiece =
							$"{lastMovedPiece.BoardPosition.X},{lastMovedPiece.BoardPosition.Y}";
						snapshot.MultiMoveCount = pieceState.MovesThisTurn;
						snapshot.MultiMoveTotal = multiMovePiece.MovesPerTurn;
					}
				}
			}

			return snapshot;
		}

		/// <summary>
		/// Applies a received snapshot to the local game state (for non-authoritative clients)
		/// </summary>
		public void ApplySnapshot(GameStateSnapshot snapshot)
		{
			if (isAuthoritative)
			{
				GD.PrintErr("Authoritative host should not apply remote snapshots!");
				return;
			}

			lastReceivedSnapshot = snapshot;

			// Apply timer state
			var timerManager = gameManager.TimerManager;
			if (timerManager != null && snapshot.Timers != null)
			{
				timerManager.SetTime(PieceColor.White, snapshot.Timers.WhiteTime);
				timerManager.SetTime(PieceColor.Black, snapshot.Timers.BlackTime);

				if (snapshot.Timers.IsWhiteTicking)
					timerManager.StartTimer(PieceColor.White);
				else
					timerManager.StopTimer(PieceColor.White);

				if (snapshot.Timers.IsBlackTicking)
					timerManager.StartTimer(PieceColor.Black);
				else
					timerManager.StopTimer(PieceColor.Black);
			}

			// Apply piece positions
			ReconcilePiecePositions(snapshot);

			// Apply game state
			ApplyGameState(snapshot);

			// Apply turn state
			// TODO: Need to make TurnManager.CurrentTurn settable for network sync
			// if (gameManager.TurnManager.CurrentTurn != (PieceColor)snapshot.CurrentTurn)
			// {
			//     gameManager.TurnManager.CurrentTurn = (PieceColor)snapshot.CurrentTurn;
			// }

			EmitSignal(SignalName.StateReceived);
		}

		/// <summary>
		/// Reconciles piece positions with the authoritative snapshot
		/// </summary>
		private void ReconcilePiecePositions(GameStateSnapshot snapshot)
		{
			// Create a map of current pieces
			var currentPieces = new Dictionary<Vector2I, Piece>();
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var pos = new Vector2I(x, y);
					var piece = board.GetPieceAt(pos);
					if (piece != null)
					{
						currentPieces[pos] = piece;
					}
				}
			}

			// Apply snapshot pieces
			var snapshotPositions = new HashSet<Vector2I>();
			foreach (var pieceInfo in snapshot.Pieces)
			{
				var pos = new Vector2I(pieceInfo.X, pieceInfo.Y);
				snapshotPositions.Add(pos);

				var currentPiece = board.GetPieceAt(pos);

				// Check if piece matches
				if (
					currentPiece == null
					|| currentPiece.Type.ToString() != pieceInfo.Type
					|| currentPiece.ClassName != pieceInfo.ClassName
					|| (int)currentPiece.Color != pieceInfo.Color
				)
				{
					// Need to reconcile - remove current and place correct piece
					if (currentPiece != null)
					{
						board.RemovePiece(pos);
						currentPiece.QueueFree();
					}

					// Create the correct piece
					var pieceType = Enum.Parse<PieceType>(pieceInfo.Type);
					var army = pieceInfo.Color == (int)PieceColor.White ? board.GetWhiteArmy() : board.GetBlackArmy();

					if (army != null)
					{
						// This is a simplified recreation - may need army position index
						var newPiece = army.CreatePiece(pieceType, 0); // TODO: Track position index
						newPiece.BoardPosition = pos;
						newPiece.HasMoved = pieceInfo.HasMoved;
						board.PlacePiece(newPiece, pos);
					}
				}
				else
				{
					// Update piece state
					currentPiece.HasMoved = pieceInfo.HasMoved;

					// Update board state manager info
					var stateManager = board.GetBoardStateManager();
					if (stateManager != null)
					{
						var pieceState = stateManager.GetPieceState(currentPiece);
						pieceState.MovesThisTurn = pieceInfo.MovesThisTurn;
						pieceState.IsFrozen = pieceInfo.IsFrozen;
					}
				}
			}

			// Remove pieces not in snapshot
			foreach (var kvp in currentPieces)
			{
				if (!snapshotPositions.Contains(kvp.Key))
				{
					board.RemovePiece(kvp.Key);
					kvp.Value.QueueFree();
				}
			}
		}

		/// <summary>
		/// Applies game state flags from snapshot
		/// </summary>
		private void ApplyGameState(GameStateSnapshot snapshot)
		{
			var gameStateManager = gameManager.GameStateManager;
			if (gameStateManager != null)
			{
				var newState = (GameState)snapshot.GameState;
				if (gameStateManager.CurrentState != newState)
				{
					// TODO: Need to make GameStateManager.CurrentState settable for network sync
					// gameStateManager.CurrentState = newState;

					// Trigger appropriate UI updates
					if (snapshot.IsCheckmate)
					{
						GD.Print($"Checkmate! Game Over.");
					}
					else if (snapshot.IsStalemate)
					{
						GD.Print($"Stalemate! Game is a draw.");
					}
					else if (snapshot.IsDraw)
					{
						GD.Print($"Game is a draw.");
					}
					else if (snapshot.IsCheck)
					{
						GD.Print($"Check!");
					}
				}
			}
		}

		/// <summary>
		/// Sends the current game state to all clients (called by host)
		/// </summary>
		public void BroadcastState()
		{
			if (!isAuthoritative || !networkManager.IsConnected)
				return;

			var snapshot = CaptureSnapshot();
			lastSentSnapshot = snapshot;

			var json = JsonSerializer.Serialize(snapshot);
			networkManager.SendGameState(json);
		}

		/// <summary>
		/// Validates if a move is legal according to the current authoritative state
		/// </summary>
		public bool ValidateMove(Vector2I from, Vector2I to, PieceColor playerColor)
		{
			if (!isAuthoritative)
				return true; // Non-authoritative clients always accept

			var piece = board.GetPieceAt(from);
			if (piece == null || piece.Color != playerColor)
				return false;

			// Check if it's the player's turn
			if (gameManager.TurnManager.CurrentTurn != playerColor)
				return false;

			// Validate the move is legal
			var possibleMoves = piece.GetPossibleMoves(board);
			return possibleMoves.Contains(to);
		}

		/// <summary>
		/// Gets the current move number
		/// </summary>
		private int GetMoveNumber()
		{
			// This is a simplified implementation
			// In a real game, you'd track this properly
			return 0;
		}

		/// <summary>
		/// Handles network state updates
		/// </summary>
		public void OnNetworkStateReceived(string jsonState)
		{
			try
			{
				var snapshot = JsonSerializer.Deserialize<GameStateSnapshot>(jsonState);
				if (snapshot != null)
				{
					ApplySnapshot(snapshot);
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"Failed to deserialize network state: {e.Message}");
			}
		}

		/// <summary>
		/// Checks if states are synchronized
		/// </summary>
		public bool IsStateSynchronized()
		{
			if (lastSentSnapshot == null || lastReceivedSnapshot == null)
				return true; // No comparison possible

			// Compare key state elements
			return lastSentSnapshot.CurrentTurn == lastReceivedSnapshot.CurrentTurn
				&& lastSentSnapshot.GameState == lastReceivedSnapshot.GameState
				&& lastSentSnapshot.Pieces.Count == lastReceivedSnapshot.Pieces.Count;
		}
	}
}

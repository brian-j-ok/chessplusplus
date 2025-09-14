namespace ChessPlusPlus.Core.Managers
{
	using System.Threading.Tasks;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	/// <summary>
	/// Manages turn progression and player turn states in the chess game
	/// </summary>
	public partial class TurnManager : Node
	{
		public PieceColor CurrentTurn { get; private set; } = PieceColor.White;

		private PlayerController? currentPlayer;
		private bool isProcessingTurn = false;

		[Signal]
		public delegate void TurnChangedEventHandler(PieceColor newTurn);

		[Signal]
		public delegate void TurnEndedEventHandler(PieceColor endedTurn);

		/// <summary>
		/// Initializes the turn manager for a new game
		/// </summary>
		public void Initialize()
		{
			CurrentTurn = PieceColor.White;
			isProcessingTurn = false;
		}

		/// <summary>
		/// Processes the next turn for the current player
		/// </summary>
		public async Task<bool> ProcessNextTurn(
			PlayerController? whitePlayer,
			PlayerController? blackPlayer,
			Board board,
			GameState gameState
		)
		{
			if (
				isProcessingTurn
				|| gameState == GameState.Checkmate
				|| gameState == GameState.Stalemate
				|| gameState == GameState.Draw
			)
				return false;

			isProcessingTurn = true;

			currentPlayer = CurrentTurn == PieceColor.White ? whitePlayer : blackPlayer;

			if (currentPlayer == null)
			{
				GD.PrintErr($"No player controller for {CurrentTurn}");
				isProcessingTurn = false;
				return false;
			}

			GD.Print($"Processing turn for {CurrentTurn}");
			currentPlayer.OnTurnStarted();

			var move = await currentPlayer.GetNextMoveAsync();

			if (move != null)
			{
				GD.Print($"Executing move from {move.Value.From} to {move.Value.To}");

				if (board.MovePiece(move.Value.From, move.Value.To))
				{
					isProcessingTurn = false;
					return true;
				}
				else
				{
					GD.PrintErr($"Failed to execute move from {move.Value.From} to {move.Value.To}");
					isProcessingTurn = false;
					return false;
				}
			}
			else
			{
				GD.Print("No move returned from player");
				isProcessingTurn = false;
				return false;
			}
		}

		/// <summary>
		/// Ends the current turn and switches to the next player
		/// </summary>
		public void EndTurn(Board? board = null)
		{
			currentPlayer?.OnTurnEnded();

			var previousTurn = CurrentTurn;
			CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

			// Notify board of turn change for ability processing
			board?.OnTurnStart(CurrentTurn);

			EmitSignal(SignalName.TurnEnded, (int)previousTurn);
			EmitSignal(SignalName.TurnChanged, (int)CurrentTurn);
		}

		/// <summary>
		/// Notifies players that the game has ended
		/// </summary>
		public void NotifyGameEnded(PlayerController? whitePlayer, PlayerController? blackPlayer, GameState finalState)
		{
			whitePlayer?.OnGameEnded(finalState);
			blackPlayer?.OnGameEnded(finalState);
		}

		/// <summary>
		/// Gets whether a turn is currently being processed
		/// </summary>
		public bool IsProcessingTurn()
		{
			return isProcessingTurn;
		}
	}
}

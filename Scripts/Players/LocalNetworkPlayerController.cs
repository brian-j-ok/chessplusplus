namespace ChessPlusPlus.Players
{
	using System.Threading.Tasks;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Network;
	using ChessPlusPlus.Pieces;
	using Godot;

	/// <summary>
	/// A simplified network player controller that handles both local and remote players
	/// Local players use mouse input and send moves over network
	/// Remote players receive moves from the network
	/// </summary>
	public partial class LocalNetworkPlayerController : HumanPlayerController
	{
		[Export]
		public bool IsLocalPlayer { get; set; } = true;
		private NetworkManager? networkManager;
		private int currentMoveNumber = 0;
		private int totalMovesRequired = 1;

		public override void _Ready()
		{
			base._Ready();
			PlayerName = IsLocalPlayer ? $"You ({PlayerColor})" : $"Opponent ({PlayerColor})";

			// Get NetworkManager instance
			networkManager = NetworkManager.Instance;
			if (!IsLocalPlayer && networkManager != null)
			{
				// Listen for moves from the network
				networkManager.MoveReceived += OnNetworkMoveReceived;
			}
		}

		public override async Task<Move?> GetNextMoveAsync()
		{
			if (IsLocalPlayer)
			{
				// Local player uses normal human input
				var move = await base.GetNextMoveAsync();

				// Check if this is a multi-move piece
				if (move != null && move.Value.Piece != null)
				{
					bool isMultiMove = false;
					int moveNumber = 1;
					int totalMoves = 1;

					// Check if the piece has multi-move ability
					if (move.Value.Piece is ChessPlusPlus.Core.Abilities.IMultiMoveAbility multiMoveAbility)
					{
						isMultiMove = true;
						totalMoves = multiMoveAbility.MovesPerTurn;

						// Track current move number for this piece
						var boardStateManager = board.GetBoardStateManager();
						if (boardStateManager != null)
						{
							var pieceState = boardStateManager.GetPieceState(move.Value.Piece);
							moveNumber = pieceState.MovesThisTurn + 1;
						}
					}

					// Send the move over the network with multi-move info
					if (networkManager != null && networkManager.IsConnected)
					{
						GD.Print(
							$"Sending move to network: {move.Value.From} to {move.Value.To} (Move {moveNumber}/{totalMoves})"
						);
						networkManager.SendMove(move.Value.From, move.Value.To, isMultiMove, moveNumber, totalMoves);
					}
				}

				return move;
			}
			else
			{
				// Remote player waits for network move
				if (currentMoveTask != null)
				{
					// Already waiting - this can happen with multi-moves
					GD.Print($"Still waiting for opponent's move ({PlayerColor})...");
					var existingMove = await currentMoveTask.Task;
					currentMoveTask = null;
					return existingMove;
				}

				GD.Print($"Waiting for opponent's move ({PlayerColor})...");
				currentMoveTask = new TaskCompletionSource<Move?>();

				var move = await currentMoveTask.Task;
				currentMoveTask = null;
				return move;
			}
		}

		private void OnNetworkMoveReceived(Vector2I from, Vector2I to)
		{
			if (IsLocalPlayer)
			{
				// Ignore network moves if we're the local player
				return;
			}

			if (currentMoveTask == null)
			{
				GD.PrintErr("Received network move but not waiting for one!");
				return;
			}

			var piece = board.GetPieceAt(from);
			if (piece == null)
			{
				GD.PrintErr($"No piece at {from} for network move!");
				currentMoveTask.SetResult(null);
				return;
			}

			var capturedPiece = board.GetPieceAt(to);
			var move = new Move(from, to, piece, capturedPiece);
			GD.Print($"Received opponent's move: {from} to {to}");
			currentMoveTask.SetResult(move);
		}

		public override void OnTurnStarted()
		{
			base.OnTurnStarted();
			if (!IsLocalPlayer)
			{
				GD.Print($"Opponent's turn ({PlayerColor})");
			}
		}
	}
}

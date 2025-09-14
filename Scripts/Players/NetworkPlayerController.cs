namespace ChessPlusPlus.Players
{
	using System.Threading.Tasks;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Network;
	using ChessPlusPlus.Pieces;
	using Godot;

	public partial class NetworkPlayerController : PlayerController
	{
		[Export]
		public bool IsLocalPlayer { get; set; } = false;

		private TaskCompletionSource<Move?>? currentMoveTask;
		private NetworkManager? networkManager;

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
			if (currentMoveTask != null)
			{
				GD.PrintErr("Already waiting for a network move!");
				return null;
			}

			currentMoveTask = new TaskCompletionSource<Move?>();

			if (IsLocalPlayer)
			{
				// Local player uses mouse input
				GD.Print($"Local network player {PlayerColor} waiting for input...");
				// The input will come through HandleBoardClick (like HumanPlayerController)
			}
			else
			{
				// Remote player waits for network move
				GD.Print($"Waiting for opponent's move ({PlayerColor})...");
			}

			var move = await currentMoveTask.Task;
			currentMoveTask = null;
			return move;
		}

		/// <summary>
		/// Handle mouse input for local network player
		/// </summary>
		public void HandleBoardClick(Vector2 clickPosition)
		{
			if (!IsLocalPlayer || currentMoveTask == null)
				return;

			var boardPos = board.WorldToBoardPosition(board.ToLocal(clickPosition));
			if (!board.IsValidPosition(boardPos))
				return;

			// This is simplified - in a real implementation you'd have piece selection
			// For now, we'll just detect valid moves
			var piece = board.GetPieceAt(boardPos);
			if (piece != null && piece.Color == PlayerColor)
			{
				// TODO: Implement piece selection UI
				GD.Print($"Selected piece at {boardPos}");
			}
		}

		/// <summary>
		/// Make a move as the local player and send it over the network
		/// </summary>
		public void MakeLocalMove(Vector2I from, Vector2I to)
		{
			if (!IsLocalPlayer || currentMoveTask == null)
				return;

			var piece = board.GetPieceAt(from);
			if (piece == null || piece.Color != PlayerColor)
				return;

			if (board.IsValidMove(from, to))
			{
				var capturedPiece = board.GetPieceAt(to);
				var move = new Move(from, to, piece, capturedPiece);

				// Send move to the other player
				networkManager?.SendMove(from, to);

				// Complete the move locally
				currentMoveTask.SetResult(move);
			}
		}

		/// <summary>
		/// Called when a move is received from the network
		/// </summary>
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
			if (IsLocalPlayer)
			{
				GD.Print("Your turn (network game)");
			}
		}

		public override void OnGameEnded(GameState finalState)
		{
			base.OnGameEnded(finalState);
			// Notify network of game end
			GD.Print($"Network game ended: {finalState}");
		}
	}
}

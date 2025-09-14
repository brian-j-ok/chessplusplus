namespace ChessPlusPlus.Players
{
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using Godot;
	using System.Threading.Tasks;

	public partial class NetworkPlayerController : PlayerController
	{
		[Export] public string NetworkId { get; set; } = "";
		[Export] public bool IsLocalPlayer { get; set; } = false;

		private TaskCompletionSource<Move?>? currentMoveTask;

		public override void _Ready()
		{
			PlayerName = $"Network Player ({PlayerColor})";
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
				// If this is the local player in a network game,
				// they would use input similar to HumanPlayerController
				// For now, this is just a stub
				GD.Print("Waiting for local player network move...");
			}
			else
			{
				// Wait for move from network
				GD.Print($"Waiting for network move from {NetworkId}...");
			}

			var move = await currentMoveTask.Task;
			currentMoveTask = null;
			return move;
		}

		/// <summary>
		/// Called when a move is received from the network
		/// </summary>
		public void OnNetworkMoveReceived(Vector2I from, Vector2I to)
		{
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
			currentMoveTask.SetResult(move);
		}

		/// <summary>
		/// Sends a move to the network
		/// </summary>
		public void SendMoveToNetwork(Move move)
		{
			// This would send the move to other players via network
			// For now, this is just a stub
			GD.Print($"Sending move to network: {move.From} -> {move.To}");
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

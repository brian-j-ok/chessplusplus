namespace ChessPlusPlus.Players
{
	using System.Threading.Tasks;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using Godot;

	public partial class HumanPlayerController : PlayerController
	{
		protected TaskCompletionSource<Move?>? currentMoveTask;
		private Piece? selectedPiece;
		private Vector2I selectedPosition;
		private bool isWaitingForInput = false;

		public override void _Ready()
		{
			PlayerName = "Human Player";
		}

		public override async Task<Move?> GetNextMoveAsync()
		{
			if (currentMoveTask != null)
			{
				GD.PrintErr("Already waiting for a move!");
				return null;
			}

			GD.Print($"HumanPlayer {PlayerColor} waiting for move...");
			currentMoveTask = new TaskCompletionSource<Move?>();
			isWaitingForInput = true;

			var move = await currentMoveTask.Task;

			currentMoveTask = null;
			isWaitingForInput = false;
			GD.Print($"HumanPlayer {PlayerColor} move complete");
			return move;
		}

		public void HandleBoardClick(Vector2 clickPosition)
		{
			if (!isWaitingForInput)
			{
				GD.Print($"Not waiting for input (Player: {PlayerColor}, Waiting: {isWaitingForInput})");
				return;
			}

			if (gameManager.CurrentTurn != PlayerColor)
			{
				GD.Print($"Not this player's turn (Current: {gameManager.CurrentTurn}, Player: {PlayerColor})");
				return;
			}

			var boardPos = board.WorldToBoardPosition(board.ToLocal(clickPosition));

			if (!board.IsValidPosition(boardPos))
				return;

			var clickedPiece = board.GetPieceAt(boardPos);

			if (selectedPiece == null)
			{
				if (clickedPiece != null && clickedPiece.Color == PlayerColor)
				{
					SelectPiece(clickedPiece, boardPos);
				}
			}
			else
			{
				if (clickedPiece != null && clickedPiece.Color == PlayerColor)
				{
					SelectPiece(clickedPiece, boardPos);
				}
				else
				{
					TryMakeMove(boardPos);
				}
			}
		}

		private void SelectPiece(Piece piece, Vector2I position)
		{
			selectedPiece = piece;
			selectedPosition = position;
			EmitSignal(SignalName.PieceSelected, piece, position);
			board.HighlightPossibleMoves(piece);
		}

		private void TryMakeMove(Vector2I targetPosition)
		{
			if (selectedPiece == null)
				return;

			GD.Print($"Trying move from {selectedPosition} to {targetPosition}");
			if (IsValidMove(selectedPosition, targetPosition))
			{
				var capturedPiece = board.GetPieceAt(targetPosition);
				var move = new Move(selectedPosition, targetPosition, selectedPiece, capturedPiece);

				GD.Print($"Valid move! Completing task...");
				ClearSelection();
				currentMoveTask?.SetResult(move);
			}
			else
			{
				GD.Print($"Invalid move from {selectedPosition} to {targetPosition}");
			}
		}

		public void ClearSelection()
		{
			selectedPiece = null;
			selectedPosition = new Vector2I(-1, -1);
			board.ClearHighlights();
		}

		public void CancelMove()
		{
			ClearSelection();
			currentMoveTask?.SetResult(null);
		}

		public override void OnTurnStarted()
		{
			base.OnTurnStarted();
			// isWaitingForInput is set in GetNextMoveAsync
		}

		public override void OnTurnEnded()
		{
			base.OnTurnEnded();
			isWaitingForInput = false;
			ClearSelection();
		}
	}
}

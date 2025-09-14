namespace ChessPlusPlus.Players
{
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using Godot;
	using System.Threading.Tasks;

	public struct Move
	{
		public Vector2I From { get; set; }
		public Vector2I To { get; set; }
		public Piece Piece { get; set; }
		public Piece? CapturedPiece { get; set; }
		public float Score { get; set; }

		public Move(Vector2I from, Vector2I to, Piece piece, Piece? capturedPiece = null, float score = 0f)
		{
			From = from;
			To = to;
			Piece = piece;
			CapturedPiece = capturedPiece;
			Score = score;
		}
	}

	public abstract partial class PlayerController : Node
	{
		[Export] public PieceColor PlayerColor { get; set; }
		[Export] public string PlayerName { get; set; } = "Player";

		protected Board board = null!;
		protected GameManager gameManager = null!;

		[Signal]
		public delegate void MoveSelectedEventHandler(Vector2I from, Vector2I to);

		[Signal]
		public delegate void PieceSelectedEventHandler(Piece piece, Vector2I position);

		public virtual void Initialize(Board board, GameManager gameManager)
		{
			this.board = board;
			this.gameManager = gameManager;
		}

		public abstract Task<Move?> GetNextMoveAsync();

		public virtual void OnTurnStarted()
		{
			GD.Print($"{PlayerName} ({PlayerColor}) turn started");
		}

		public virtual void OnTurnEnded()
		{
			GD.Print($"{PlayerName} ({PlayerColor}) turn ended");
		}

		public virtual void OnGameEnded(GameState finalState)
		{
			GD.Print($"Game ended with state: {finalState}");
		}

		protected bool IsValidMove(Vector2I from, Vector2I to)
		{
			var piece = board.GetPieceAt(from);
			if (piece == null || piece.Color != PlayerColor)
				return false;

			return board.IsValidMove(from, to);
		}
	}
}

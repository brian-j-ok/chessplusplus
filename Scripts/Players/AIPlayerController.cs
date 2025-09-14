namespace ChessPlusPlus.Players
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using Godot;

	public enum AIDifficulty
	{
		Easy = 1,
		Medium = 2,
		Hard = 3,
	}

	public partial class AIPlayerController : PlayerController
	{
		[Export]
		public AIDifficulty Difficulty { get; set; } = AIDifficulty.Medium;

		[Export]
		public float ThinkingTimeMin { get; set; } = 0.5f;

		[Export]
		public float ThinkingTimeMax { get; set; } = 2.0f;

		private readonly Dictionary<PieceType, int> pieceValues =
			new()
			{
				{ PieceType.Pawn, 100 },
				{ PieceType.Knight, 320 },
				{ PieceType.Bishop, 330 },
				{ PieceType.Rook, 500 },
				{ PieceType.Queen, 900 },
				{ PieceType.King, 20000 },
			};

		private readonly float[,] pawnPositionBonus = new float[8, 8]
		{
			{ 0, 0, 0, 0, 0, 0, 0, 0 },
			{ 50, 50, 50, 50, 50, 50, 50, 50 },
			{ 10, 10, 20, 30, 30, 20, 10, 10 },
			{ 5, 5, 10, 25, 25, 10, 5, 5 },
			{ 0, 0, 0, 20, 20, 0, 0, 0 },
			{ 5, -5, -10, 0, 0, -10, -5, 5 },
			{ 5, 10, 10, -20, -20, 10, 10, 5 },
			{ 0, 0, 0, 0, 0, 0, 0, 0 },
		};

		private readonly float[,] knightPositionBonus = new float[8, 8]
		{
			{ -50, -40, -30, -30, -30, -30, -40, -50 },
			{ -40, -20, 0, 0, 0, 0, -20, -40 },
			{ -30, 0, 10, 15, 15, 10, 0, -30 },
			{ -30, 5, 15, 20, 20, 15, 5, -30 },
			{ -30, 0, 15, 20, 20, 15, 0, -30 },
			{ -30, 5, 10, 15, 15, 10, 5, -30 },
			{ -40, -20, 0, 5, 5, 0, -20, -40 },
			{ -50, -40, -30, -30, -30, -30, -40, -50 },
		};

		private readonly float[,] centerControlBonus = new float[8, 8]
		{
			{ -20, -10, -10, -10, -10, -10, -10, -20 },
			{ -10, 0, 0, 0, 0, 0, 0, -10 },
			{ -10, 0, 5, 5, 5, 5, 0, -10 },
			{ -10, 0, 5, 10, 10, 5, 0, -10 },
			{ -10, 0, 5, 10, 10, 5, 0, -10 },
			{ -10, 0, 5, 5, 5, 5, 0, -10 },
			{ -10, 0, 0, 0, 0, 0, 0, -10 },
			{ -20, -10, -10, -10, -10, -10, -10, -20 },
		};

		public override void _Ready()
		{
			PlayerName = $"AI ({Difficulty})";
		}

		public override async Task<Move?> GetNextMoveAsync()
		{
			GD.Print($"AI {PlayerColor} thinking (Difficulty: {Difficulty})...");
			await Task.Delay((int)(GD.RandRange(ThinkingTimeMin, ThinkingTimeMax) * 1000));

			var possibleMoves = GetAllPossibleMoves(PlayerColor);
			GD.Print($"AI found {possibleMoves.Count} possible moves");

			if (possibleMoves.Count == 0)
			{
				GD.Print("AI has no valid moves!");
				return null;
			}

			Move bestMove;
			switch (Difficulty)
			{
				case AIDifficulty.Easy:
					bestMove = GetRandomMove(possibleMoves);
					break;
				case AIDifficulty.Medium:
					bestMove = GetBestMoveSimple(possibleMoves);
					break;
				case AIDifficulty.Hard:
					bestMove = GetBestMoveMinimax(possibleMoves, 3);
					break;
				default:
					bestMove = possibleMoves[0];
					break;
			}

			GD.Print($"AI selected move: {bestMove.From} to {bestMove.To}");
			return bestMove;
		}

		private List<Move> GetAllPossibleMoves(PieceColor color)
		{
			var moves = new List<Move>();

			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var from = new Vector2I(x, y);
					var piece = board.GetPieceAt(from);

					if (piece != null && piece.Color == color)
					{
						var possibleTargets = GetPossibleMovesForPiece(piece, from);
						foreach (var to in possibleTargets)
						{
							var capturedPiece = board.GetPieceAt(to);
							moves.Add(new Move(from, to, piece, capturedPiece));
						}
					}
				}
			}

			return moves;
		}

		private List<Vector2I> GetPossibleMovesForPiece(Piece piece, Vector2I from)
		{
			var moves = new List<Vector2I>();

			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var to = new Vector2I(x, y);
					if (board.IsValidMove(from, to))
					{
						moves.Add(to);
					}
				}
			}

			return moves;
		}

		private Move GetRandomMove(List<Move> moves)
		{
			return moves[GD.RandRange(0, moves.Count - 1)];
		}

		private Move GetBestMoveSimple(List<Move> moves)
		{
			for (int i = 0; i < moves.Count; i++)
			{
				var move = moves[i];
				move.Score = EvaluateMove(move);
				moves[i] = move;
			}

			var sortedMoves = moves.OrderByDescending(m => m.Score).ToList();

			var topMoves = sortedMoves.Take(3).ToList();
			return topMoves[GD.RandRange(0, topMoves.Count - 1)];
		}

		private Move GetBestMoveMinimax(List<Move> moves, int depth)
		{
			float bestScore = float.MinValue;
			Move bestMove = moves[0];

			foreach (var move in moves)
			{
				float score = Minimax(move, depth - 1, float.MinValue, float.MaxValue, false);
				if (score > bestScore)
				{
					bestScore = score;
					bestMove = move;
				}
			}

			return bestMove;
		}

		private float Minimax(Move move, int depth, float alpha, float beta, bool isMaximizing)
		{
			if (depth == 0)
			{
				return EvaluateMove(move);
			}

			var opponentColor = PlayerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
			var nextMoves = isMaximizing ? GetAllPossibleMoves(PlayerColor) : GetAllPossibleMoves(opponentColor);

			if (nextMoves.Count == 0)
			{
				return EvaluateMove(move) * (depth + 1);
			}

			if (isMaximizing)
			{
				float maxEval = float.MinValue;
				foreach (var nextMove in nextMoves)
				{
					float eval = Minimax(nextMove, depth - 1, alpha, beta, false);
					maxEval = Mathf.Max(maxEval, eval);
					alpha = Mathf.Max(alpha, eval);
					if (beta <= alpha)
						break;
				}
				return maxEval;
			}
			else
			{
				float minEval = float.MaxValue;
				foreach (var nextMove in nextMoves)
				{
					float eval = Minimax(nextMove, depth - 1, alpha, beta, true);
					minEval = Mathf.Min(minEval, eval);
					beta = Mathf.Min(beta, eval);
					if (beta <= alpha)
						break;
				}
				return minEval;
			}
		}

		private float EvaluateMove(Move move)
		{
			float score = 0;

			if (move.CapturedPiece != null)
			{
				score += pieceValues[move.CapturedPiece.Type];
			}

			score += GetPositionBonus(move.Piece, move.To);

			if (IsCheckMove(move))
			{
				score += 50;
			}

			if (IsCenterControl(move.To))
			{
				score += 30;
			}

			if (IsPieceDevelopment(move))
			{
				score += 20;
			}

			if (IsCastlingMove(move))
			{
				score += 60;
			}

			return score;
		}

		private float GetPositionBonus(Piece piece, Vector2I position)
		{
			int x = position.X;
			int y = piece.Color == PieceColor.White ? position.Y : 7 - position.Y;

			switch (piece.Type)
			{
				case PieceType.Pawn:
					return pawnPositionBonus[y, x];
				case PieceType.Knight:
					return knightPositionBonus[y, x];
				default:
					return centerControlBonus[y, x];
			}
		}

		private bool IsCheckMove(Move move)
		{
			return false;
		}

		private bool IsCenterControl(Vector2I position)
		{
			return (position.X >= 3 && position.X <= 4) && (position.Y >= 3 && position.Y <= 4);
		}

		private bool IsPieceDevelopment(Move move)
		{
			if (move.Piece.Type == PieceType.Knight || move.Piece.Type == PieceType.Bishop)
			{
				int homeRow = move.Piece.Color == PieceColor.White ? 0 : 7;
				return move.From.Y == homeRow;
			}
			return false;
		}

		private bool IsCastlingMove(Move move)
		{
			return move.Piece.Type == PieceType.King && Mathf.Abs(move.To.X - move.From.X) == 2;
		}

		public override void OnTurnStarted()
		{
			base.OnTurnStarted();
			GD.Print($"AI thinking... (Difficulty: {Difficulty})");
		}
	}
}

namespace ChessPlusPlus.Players
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using ChessPlusPlus.AI;
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

		[Export]
		public bool UseLearning { get; set; } = true;

		[Export]
		public int SearchDepth { get; set; } = 3;

		private IPieceEvaluator evaluator = null!;
		private string learnedValuesPath = "user://ai_learned_values.json";

		public override void _Ready()
		{
			PlayerName = $"AI ({Difficulty})";

			// Initialize the dynamic evaluator
			evaluator = new DynamicPieceEvaluator();
			evaluator.SetDifficulty(Difficulty);

			// Load learned values if enabled
			if (UseLearning && evaluator is DynamicPieceEvaluator dynamicEval)
			{
				dynamicEval.LoadLearnedValues(learnedValuesPath);
			}

			// Adjust search depth based on difficulty
			SearchDepth = Difficulty switch
			{
				AIDifficulty.Easy => 1,
				AIDifficulty.Medium => 2,
				AIDifficulty.Hard => 3,
				_ => 2,
			};
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
					bestMove = GetBestMoveMinimax(possibleMoves, SearchDepth);
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
				move.Score = evaluator.EvaluateMove(move, board);
				moves[i] = move;
			}

			var sortedMoves = moves.OrderByDescending(m => m.Score).ToList();

			// Add some randomness for variety
			int topMovesCount = Difficulty switch
			{
				AIDifficulty.Easy => 5,
				AIDifficulty.Medium => 3,
				AIDifficulty.Hard => 2,
				_ => 3,
			};

			var topMoves = sortedMoves.Take(Mathf.Min(topMovesCount, sortedMoves.Count)).ToList();
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
				return evaluator.EvaluateMove(move, board, SearchDepth - depth);
			}

			var opponentColor = PlayerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
			var nextMoves = isMaximizing ? GetAllPossibleMoves(PlayerColor) : GetAllPossibleMoves(opponentColor);

			if (nextMoves.Count == 0)
			{
				// Check for checkmate or stalemate
				float endgameBonus = isMaximizing ? -10000 : 10000;
				return evaluator.EvaluateMove(move, board, SearchDepth - depth) + endgameBonus;
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

		public void SaveLearnedValues()
		{
			if (UseLearning && evaluator is DynamicPieceEvaluator dynamicEval)
			{
				dynamicEval.SaveLearnedValues(learnedValuesPath);
			}
		}

		public override void _ExitTree()
		{
			// Save learned values when AI is destroyed
			SaveLearnedValues();
			base._ExitTree();
		}

		public override void OnTurnStarted()
		{
			base.OnTurnStarted();
			GD.Print($"AI thinking... (Difficulty: {Difficulty})");
		}
	}
}

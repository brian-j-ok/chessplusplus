namespace ChessPlusPlus.AI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Core.Abilities;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	/// <summary>
	/// Dynamically evaluates pieces based on their capabilities and board state
	/// </summary>
	public partial class DynamicPieceEvaluator : Node, IPieceEvaluator
	{
		private AIDifficulty difficulty = AIDifficulty.Medium;
		private Dictionary<string, float> pieceValueCache = new();
		private Dictionary<string, float> learnedValues = new();

		// Base values for standard pieces (will be adjusted based on abilities)
		private readonly Dictionary<PieceType, float> baseValues =
			new()
			{
				{ PieceType.Pawn, 100 },
				{ PieceType.Knight, 320 },
				{ PieceType.Bishop, 330 },
				{ PieceType.Rook, 500 },
				{ PieceType.Queen, 900 },
				{ PieceType.King, 20000 },
			};

		// Ability value modifiers
		private readonly Dictionary<Type, float> abilityValues =
			new()
			{
				{ typeof(IPassiveAbility), 150 },
				{ typeof(ICaptureModifier), 100 },
				{ typeof(IDefensiveAbility), 120 },
				{ typeof(IBoardEffect), 180 },
				{ typeof(IMovementModifier), 80 },
				{ typeof(IPersistentEffect), 140 },
				{ typeof(IMultiMoveAbility), 200 },
			};

		public void SetDifficulty(AIDifficulty newDifficulty)
		{
			difficulty = newDifficulty;
		}

		public float GetPieceValue(Piece piece)
		{
			// Cache key includes piece type and class name
			string cacheKey = $"{piece.Color}_{piece.Type}_{piece.ClassName}";

			// Check learned values first (from past games)
			if (learnedValues.TryGetValue(cacheKey, out float learnedValue))
			{
				return learnedValue;
			}

			// Check cache
			if (pieceValueCache.TryGetValue(cacheKey, out float cachedValue))
			{
				return cachedValue;
			}

			// Calculate dynamic value
			float value = CalculateDynamicPieceValue(piece);
			pieceValueCache[cacheKey] = value;
			return value;
		}

		private float CalculateDynamicPieceValue(Piece piece)
		{
			// Start with base value
			float value = baseValues[piece.Type];

			// Add ability bonuses
			value += GetAbilityValue(piece, null!);

			// Adjust based on piece class name
			if (piece.ClassName != "Standard")
			{
				// Non-standard pieces get a complexity bonus
				value *= 1.1f;
			}

			// Special adjustments for specific known pieces
			switch (piece.ClassName)
			{
				case "Freezing":
					value += 200; // Freeze is very powerful
					break;
				case "Bombing":
					value += 150; // Extended capture range
					break;
				case "Ranger":
					value += 50; // Better pawn
					break;
				case "Guard":
					value += 80; // Defensive pawn
					break;
				case "Charge":
					value += 100; // Mobile knight
					break;
				case "Glass":
					value -= 100; // Fragile queen
					break;
				case "Resurrecting":
					value += 500; // Can come back
					break;
			}

			return value;
		}

		public float GetAbilityValue(Piece piece, Board board)
		{
			float abilityScore = 0;

			// Check which ability interfaces the piece implements
			foreach (var abilityType in abilityValues.Keys)
			{
				if (abilityType.IsAssignableFrom(piece.GetType()))
				{
					abilityScore += abilityValues[abilityType];

					// Additional context-sensitive scoring
					if (board != null && difficulty >= AIDifficulty.Hard)
					{
						abilityScore += GetContextualAbilityBonus(piece, abilityType, board);
					}
				}
			}

			return abilityScore;
		}

		private float GetContextualAbilityBonus(Piece piece, Type abilityType, Board board)
		{
			float bonus = 0;

			// Freezing abilities are more valuable when enemies are nearby
			if (abilityType == typeof(IBoardEffect) && piece is FreezingBishop freezer)
			{
				var affected = freezer.GetAffectedPieces(piece, board);
				bonus += affected.Count * 50;
			}

			// Capture modifiers are more valuable in crowded boards
			if (abilityType == typeof(ICaptureModifier))
			{
				int pieceCount = CountPiecesOnBoard(board);
				bonus += pieceCount * 2;
			}

			return bonus;
		}

		public float GetPositionalBonus(Piece piece, Vector2I position, Board board)
		{
			float bonus = 0;

			// Center control is universally good
			bonus += GetCenterControlBonus(position) * GetCenterControlWeight(piece);

			// Piece-specific positioning
			switch (piece.Type)
			{
				case PieceType.Pawn:
					bonus += GetPawnPositionBonus(piece, position);
					break;
				case PieceType.Knight:
					bonus += GetKnightPositionBonus(position);
					break;
				case PieceType.Bishop:
					bonus += GetDiagonalControlBonus(position);
					break;
				case PieceType.Rook:
					bonus += GetFileControlBonus(position, board);
					break;
				case PieceType.Queen:
					bonus += GetQueenPositionBonus(position);
					break;
				case PieceType.King:
					bonus += GetKingSafetyBonus(piece, position, board);
					break;
			}

			// Adjust based on difficulty
			if (difficulty == AIDifficulty.Easy)
			{
				bonus *= 0.5f; // Less emphasis on position
			}
			else if (difficulty == AIDifficulty.Hard)
			{
				bonus *= 1.2f; // More emphasis on position
			}

			return bonus;
		}

		private float GetCenterControlBonus(Vector2I position)
		{
			// Distance from center
			float centerX = 3.5f;
			float centerY = 3.5f;
			float distance = Mathf.Sqrt(Mathf.Pow(position.X - centerX, 2) + Mathf.Pow(position.Y - centerY, 2));

			// Closer to center = higher bonus
			return Mathf.Max(0, 20 - distance * 5);
		}

		private float GetCenterControlWeight(Piece piece)
		{
			// Different pieces benefit differently from center control
			return piece.Type switch
			{
				PieceType.Knight => 1.5f,
				PieceType.Bishop => 1.2f,
				PieceType.Pawn => 0.8f,
				PieceType.Queen => 1.0f,
				PieceType.Rook => 0.6f,
				PieceType.King => 0.3f,
				_ => 1.0f,
			};
		}

		private float GetPawnPositionBonus(Piece pawn, Vector2I position)
		{
			float bonus = 0;
			int rank = pawn.Color == PieceColor.White ? position.Y : 7 - position.Y;

			// Advancement bonus
			bonus += rank * 10;

			// Promotion threat
			if (rank >= 5)
			{
				bonus += (rank - 4) * 30;
			}

			// Center pawns are valuable
			if (position.X >= 3 && position.X <= 4)
			{
				bonus += 15;
			}

			return bonus;
		}

		private float GetKnightPositionBonus(Vector2I position)
		{
			// Knights are terrible on edges
			if (position.X == 0 || position.X == 7 || position.Y == 0 || position.Y == 7)
			{
				return -30;
			}

			// Knights love being in the center
			if (position.X >= 2 && position.X <= 5 && position.Y >= 2 && position.Y <= 5)
			{
				return 20;
			}

			return 0;
		}

		private float GetDiagonalControlBonus(Vector2I position)
		{
			// Bishops control more squares from center
			int squaresControlled = 0;

			for (int i = 1; i < 8; i++)
			{
				if (IsValidPosition(position + new Vector2I(i, i)))
					squaresControlled++;
				if (IsValidPosition(position + new Vector2I(i, -i)))
					squaresControlled++;
				if (IsValidPosition(position + new Vector2I(-i, i)))
					squaresControlled++;
				if (IsValidPosition(position + new Vector2I(-i, -i)))
					squaresControlled++;
			}

			return squaresControlled * 2;
		}

		private float GetFileControlBonus(Vector2I position, Board board)
		{
			float bonus = 0;

			// Open file bonus
			bool hasOwnPawn = false;
			bool hasEnemyPawn = false;

			for (int y = 0; y < 8; y++)
			{
				var piece = board?.GetPieceAt(new Vector2I(position.X, y));
				if (piece?.Type == PieceType.Pawn)
				{
					if (piece.Color == board.GetPieceAt(position)?.Color)
						hasOwnPawn = true;
					else
						hasEnemyPawn = true;
				}
			}

			if (!hasOwnPawn)
			{
				bonus += 20; // Semi-open file
				if (!hasEnemyPawn)
				{
					bonus += 15; // Fully open file
				}
			}

			// 7th rank bonus for rooks
			var rook = board?.GetPieceAt(position);
			if (rook != null)
			{
				int seventhRank = rook.Color == PieceColor.White ? 6 : 1;
				if (position.Y == seventhRank)
				{
					bonus += 30;
				}
			}

			return bonus;
		}

		private float GetQueenPositionBonus(Vector2I position)
		{
			// Queens should stay back early but be active
			return GetCenterControlBonus(position) * 0.5f;
		}

		private float GetKingSafetyBonus(Piece king, Vector2I position, Board board)
		{
			float safety = 0;

			// Castled position bonus
			if (king.HasMoved)
			{
				// Penalize king that has moved (can't castle)
				safety -= 20;
			}

			// Corner safety in non-endgame
			if (CountPiecesOnBoard(board) > 10)
			{
				if ((position.X <= 1 || position.X >= 6) && (position.Y <= 1 || position.Y >= 6))
				{
					safety += 30;
				}
			}
			else
			{
				// In endgame, king should be active
				safety += GetCenterControlBonus(position);
			}

			// Pawn shield
			int pawnShieldCount = CountPawnShield(king, position, board);
			safety += pawnShieldCount * 15;

			return safety;
		}

		private int CountPawnShield(Piece king, Vector2I position, Board board)
		{
			int count = 0;
			int direction = king.Color == PieceColor.White ? 1 : -1;

			for (int dx = -1; dx <= 1; dx++)
			{
				var checkPos = position + new Vector2I(dx, direction);
				if (IsValidPosition(checkPos))
				{
					var piece = board?.GetPieceAt(checkPos);
					if (piece?.Type == PieceType.Pawn && piece.Color == king.Color)
					{
						count++;
					}
				}
			}

			return count;
		}

		public float GetMobilityScore(Piece piece, Board board)
		{
			if (board == null || difficulty == AIDifficulty.Easy)
				return 0;

			// Count possible moves
			var moves = piece.GetPossibleMoves(board);
			float mobilityScore = moves.Count * GetMobilityWeight(piece.Type);

			// Penalize blocked pieces
			if (moves.Count == 0)
			{
				mobilityScore = -50;
			}

			return mobilityScore;
		}

		private float GetMobilityWeight(PieceType type)
		{
			return type switch
			{
				PieceType.Knight => 4,
				PieceType.Bishop => 3,
				PieceType.Rook => 2,
				PieceType.Queen => 1,
				PieceType.Pawn => 1,
				PieceType.King => 0.5f,
				_ => 1,
			};
		}

		public float GetThreatScore(Piece piece, Vector2I position, Board board)
		{
			if (board == null || difficulty <= AIDifficulty.Easy)
				return 0;

			float threatScore = 0;

			// Check if piece is under attack
			if (IsPieceUnderAttack(piece, position, board))
			{
				threatScore -= GetPieceValue(piece) * 0.5f;
			}

			// Check what this piece threatens
			var possibleMoves = piece.GetPossibleMoves(board);
			foreach (var move in possibleMoves)
			{
				var target = board.GetPieceAt(move);
				if (target != null && target.Color != piece.Color)
				{
					threatScore += GetPieceValue(target) * 0.3f;

					// Threatening the king is especially valuable
					if (target.Type == PieceType.King)
					{
						threatScore += 100;
					}
				}
			}

			return threatScore;
		}

		private bool IsPieceUnderAttack(Piece piece, Vector2I position, Board board)
		{
			// Check all enemy pieces to see if they can capture this piece
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var attacker = board.GetPieceAt(new Vector2I(x, y));
					if (attacker != null && attacker.Color != piece.Color)
					{
						var attackerMoves = attacker.GetPossibleMoves(board);
						if (attackerMoves.Contains(position))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public float EvaluatePosition(Board board, PieceColor perspective)
		{
			float evaluation = 0;

			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					var position = new Vector2I(x, y);
					var piece = board.GetPieceAt(position);

					if (piece != null)
					{
						float pieceEval = GetPieceValue(piece);
						pieceEval += GetPositionalBonus(piece, position, board);

						if (difficulty >= AIDifficulty.Medium)
						{
							pieceEval += GetMobilityScore(piece, board);
							pieceEval += GetThreatScore(piece, position, board);
						}

						// Negate for opponent pieces
						if (piece.Color != perspective)
						{
							pieceEval = -pieceEval;
						}

						evaluation += pieceEval;
					}
				}
			}

			return evaluation;
		}

		public float EvaluateMove(Move move, Board board, int depth = 0)
		{
			float score = 0;

			// Material gain from capture
			if (move.CapturedPiece != null)
			{
				score += GetPieceValue(move.CapturedPiece);

				// Capturing with a less valuable piece is better
				score += (GetPieceValue(move.CapturedPiece) - GetPieceValue(move.Piece)) * 0.1f;
			}

			// Positional improvement
			float oldPositionScore = GetPositionalBonus(move.Piece, move.From, board);
			float newPositionScore = GetPositionalBonus(move.Piece, move.To, board);
			score += newPositionScore - oldPositionScore;

			// Special move bonuses
			if (IsCastlingMove(move))
			{
				score += 60;
			}

			if (IsCheckMove(move, board))
			{
				score += 50;
			}

			if (IsPawnPromotion(move))
			{
				score += 800;
			}

			// Ability activation bonus
			if (move.Piece is IPassiveAbility passive)
			{
				score += 30; // Bonus for pieces with abilities making moves
			}

			// Depth bonus (prefer immediate gains)
			score *= (1.0f - depth * 0.05f);

			return score;
		}

		private bool IsCastlingMove(Move move)
		{
			return move.Piece.Type == PieceType.King && Mathf.Abs(move.To.X - move.From.X) == 2;
		}

		private bool IsCheckMove(Move move, Board board)
		{
			// Would need to simulate the move and check if king is in check
			// For now, return false (can be implemented later)
			return false;
		}

		private bool IsPawnPromotion(Move move)
		{
			if (move.Piece.Type != PieceType.Pawn)
				return false;

			int promotionRank = move.Piece.Color == PieceColor.White ? 7 : 0;
			return move.To.Y == promotionRank;
		}

		private int CountPiecesOnBoard(Board board)
		{
			int count = 0;
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					if (board?.GetPieceAt(new Vector2I(x, y)) != null)
						count++;
				}
			}
			return count;
		}

		private bool IsValidPosition(Vector2I pos)
		{
			return pos.X >= 0 && pos.X < 8 && pos.Y >= 0 && pos.Y < 8;
		}

		/// <summary>
		/// Learn from game outcomes to adjust piece values
		/// </summary>
		public void LearnFromGame(string pieceKey, float performanceScore)
		{
			if (!learnedValues.ContainsKey(pieceKey))
			{
				learnedValues[pieceKey] = baseValues.Values.Average();
			}

			// Adjust learned value based on performance
			learnedValues[pieceKey] = Mathf.Lerp(learnedValues[pieceKey], performanceScore, 0.1f);
		}

		/// <summary>
		/// Save learned values to file for persistence
		/// </summary>
		public void SaveLearnedValues(string filepath)
		{
			var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Write);
			if (file != null)
			{
				var data = new Godot.Collections.Dictionary();
				foreach (var kvp in learnedValues)
				{
					data[kvp.Key] = kvp.Value;
				}
				file.StoreString(Json.Stringify(data));
				file.Close();
			}
		}

		/// <summary>
		/// Load learned values from file
		/// </summary>
		public void LoadLearnedValues(string filepath)
		{
			if (FileAccess.FileExists(filepath))
			{
				var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
				if (file != null)
				{
					string jsonString = file.GetAsText();
					file.Close();

					var json = new Json();
					if (json.Parse(jsonString) == Error.Ok)
					{
						var data = json.Data.AsGodotDictionary();
						learnedValues.Clear();
						foreach (var key in data.Keys)
						{
							learnedValues[key.ToString()] = (float)data[key].AsDouble();
						}
					}
				}
			}
		}
	}
}

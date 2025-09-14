using System.Collections.Generic;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class BombingRook : Rook, ICaptureModifier
	{
		public BombingRook()
		{
			Type = PieceType.Rook;
			ClassName = "Bombing";
		}

		// ICaptureModifier implementation
		public string AbilityName => "Artillery Strike";
		public string Description => "Can capture enemies one square beyond normal range by throwing bombs over pieces";

		public List<Vector2I> GetAdditionalCaptures(Piece piece, Board board)
		{
			var additionalCaptures = new List<Vector2I>();

			Vector2I[] directions = new Vector2I[]
			{
				new Vector2I(0, 1),
				new Vector2I(0, -1),
				new Vector2I(1, 0),
				new Vector2I(-1, 0),
			};

			foreach (var direction in directions)
			{
				// Find the furthest point we can move to
				int maxDistance = 0;
				for (int distance = 1; distance < 8; distance++)
				{
					var checkPos = BoardPosition + (direction * distance);
					if (!IsValidPosition(checkPos))
						break;

					var pieceAtPos = board.GetPieceAt(checkPos);
					if (pieceAtPos != null)
					{
						// Hit a piece - this is where normal movement would stop
						maxDistance = distance;
						break;
					}
					maxDistance = distance;
				}

				// Now check one square beyond for bombing targets
				if (maxDistance > 0)
				{
					var bombPos = BoardPosition + (direction * (maxDistance + 1));
					if (IsValidPosition(bombPos))
					{
						var targetPiece = board.GetPieceAt(bombPos);
						if (targetPiece != null && IsEnemyPiece(targetPiece))
						{
							// Can bomb this enemy piece!
							additionalCaptures.Add(bombPos);
							GD.Print($"BombingRook can bomb {targetPiece.Color} {targetPiece.Type} at {bombPos}");
						}
					}
				}
			}

			return additionalCaptures;
		}

		public bool CanCapture(Piece piece, Vector2I targetPos, Board board)
		{
			// Check if this is a bombing capture (beyond normal range)
			var normalMoves = base.GetPossibleMoves(board);
			if (normalMoves.Contains(targetPos))
			{
				// Normal capture
				return true;
			}

			// Check if it's a valid bombing target
			var additionalCaptures = GetAdditionalCaptures(piece, board);
			return additionalCaptures.Contains(targetPos);
		}

		public override List<Vector2I> GetPossibleMoves(Board board)
		{
			// Get normal rook moves
			var moves = base.GetPossibleMoves(board);

			// Add bombing captures
			var bombingTargets = GetAdditionalCaptures(this, board);
			moves.AddRange(bombingTargets);

			return moves;
		}
	}
}

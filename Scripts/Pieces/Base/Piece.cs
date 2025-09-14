namespace ChessPlusPlus.Pieces
{
	using System.Collections.Generic;
	using ChessPlusPlus.Core;
	using Godot;

	public enum PieceColor
	{
		White,
		Black,
	}

	public enum PieceType
	{
		Pawn,
		Knight,
		Bishop,
		Rook,
		Queen,
		King,
	}

	/// <summary>
	/// Base class for all chess pieces, handling common functionality like positioning and rendering
	/// </summary>
	public abstract partial class Piece : Node2D
	{
		[Export]
		public PieceColor Color { get; set; }

		[Export]
		public PieceType Type { get; protected set; }

		[Export]
		public string ClassName { get; protected set; } = "Standard";

		public Vector2I BoardPosition { get; set; }
		public bool HasMoved { get; set; } = false;

		protected Sprite2D sprite = null!;

		public override void _Ready()
		{
			AddToGroup("chess_pieces");
			sprite = new Sprite2D();
			sprite.Name = "Sprite2D";
			sprite.Centered = true;
			AddChild(sprite);
			InitializePiece();
		}

		protected virtual void InitializePiece()
		{
			LoadSprite();
			ScaleToFitSquare();
		}

		/// <summary>
		/// Scales the piece sprite to fit within the chess square with padding
		/// </summary>
		public virtual void ScaleToFitSquare()
		{
			ScaleToFitSquare(GetDynamicSquareSize());
		}

		public virtual void ScaleToFitSquare(float squareSize)
		{
			if (sprite.Texture == null)
				return;

			float padding = squareSize * 0.1f; // 10% padding scales with square size
			float targetSize = squareSize - padding;

			var textureSize = sprite.Texture.GetSize();
			float scaleX = targetSize / textureSize.X;
			float scaleY = targetSize / textureSize.Y;
			float scale = Mathf.Min(scaleX, scaleY);

			sprite.Scale = new Vector2(scale, scale);

			// Center the sprite in the square (since sprite.Centered = true, position is the center point)
			sprite.Position = new Vector2(squareSize * 0.5f, squareSize * 0.5f);
		}

		private float GetDynamicSquareSize()
		{
			// Try to find BoardVisual to get current square size
			var boardVisual = GetTree().GetFirstNodeInGroup("board_visual") as ChessPlusPlus.UI.BoardVisual;
			return boardVisual?.SquareSize ?? 64.0f; // Fallback to 64 if not found
		}

		public abstract List<Vector2I> GetPossibleMoves(Board board);

		public virtual bool CanMoveTo(Vector2I targetPosition, Board board)
		{
			var possibleMoves = GetPossibleMoves(board);
			return possibleMoves.Contains(targetPosition);
		}

		public virtual void OnMoved(Vector2I from, Vector2I to, Board board)
		{
			HasMoved = true;
			BoardPosition = to;
		}

		public virtual void OnCaptured(Board board)
		{
			QueueFree();
		}

		protected virtual void LoadSprite()
		{
			string colorPrefix = Color == PieceColor.White ? "white" : "black";
			string pieceName = Type.ToString().ToLower();

			// Try class-specific sprite first (e.g., white_pawn_ranger.png)
			if (ClassName != "Standard")
			{
				string classSpecificPath =
					$"res://Resources/Textures/{colorPrefix}_{pieceName}_{ClassName.ToLower()}.png";
				if (ResourceLoader.Exists(classSpecificPath))
				{
					sprite.Texture = GD.Load<Texture2D>(classSpecificPath);
					return;
				}
			}

			// Fall back to standard sprite (e.g., white_pawn.png)
			string standardPath = $"res://Resources/Textures/{colorPrefix}_{pieceName}.png";
			if (ResourceLoader.Exists(standardPath))
			{
				sprite.Texture = GD.Load<Texture2D>(standardPath);
			}
			else
			{
				GD.PrintErr($"No sprite found for {colorPrefix} {pieceName} (class: {ClassName})");
			}
		}

		protected bool IsValidPosition(Vector2I pos)
		{
			return pos.X >= 0 && pos.X < 8 && pos.Y >= 0 && pos.Y < 8;
		}

		public bool IsEnemyPiece(Piece other)
		{
			return other != null && other.Color != this.Color;
		}

		public bool IsFriendlyPiece(Piece other)
		{
			return other != null && other.Color == this.Color;
		}
	}
}

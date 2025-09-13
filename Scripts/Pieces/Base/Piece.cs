namespace ChessPlusPlus.Pieces
{
	using System.Collections.Generic;
	using ChessPlusPlus.Core;
	using Godot;
	public enum PieceColor
	{
		White,
		Black
	}

	public enum PieceType
	{
		Pawn,
		Knight,
		Bishop,
		Rook,
		Queen,
		King
	}

	/// <summary>
	/// Base class for all chess pieces, handling common functionality like positioning and rendering
	/// </summary>
	public abstract partial class Piece : Node2D
	{
		[Export] public PieceColor Color { get; set; }
		[Export] public PieceType Type { get; protected set; }
		[Export] public string ClassName { get; protected set; } = "Standard";

		public Vector2I BoardPosition { get; set; }
		public bool HasMoved { get; set; } = false;

		protected Sprite2D sprite = null!;

		public override void _Ready()
		{
			sprite = new Sprite2D();
			sprite.Name = "Sprite2D";
			sprite.Centered = true;
			sprite.Position = new Vector2(32, 32); // Center in 64x64 square
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
		protected virtual void ScaleToFitSquare()
		{
			if (sprite.Texture == null)
				return;

			float squareSize = 64.0f;
			float padding = 8.0f;
			float targetSize = squareSize - padding;

			var textureSize = sprite.Texture.GetSize();
			float scaleX = targetSize / textureSize.X;
			float scaleY = targetSize / textureSize.Y;
			float scale = Mathf.Min(scaleX, scaleY);

			sprite.Scale = new Vector2(scale, scale);
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
			string texturePath = $"res://Resources/Textures/{colorPrefix}_{pieceName}.png";

			if (ResourceLoader.Exists(texturePath))
			{
				sprite.Texture = GD.Load<Texture2D>(texturePath);
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

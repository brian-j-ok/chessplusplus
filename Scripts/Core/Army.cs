namespace ChessPlusPlus.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.Json;
	using ChessPlusPlus.Pieces;
	using Godot;

	/// <summary>
	/// Represents a piece assignment linking position, type, and class variant
	/// </summary>
	public class PieceClassAssignment
	{
		public PieceType Type { get; set; }
		public int Position { get; set; }
		public string ClassName { get; set; } = string.Empty;

		public PieceClassAssignment(PieceType type, int position, string className = "Standard")
		{
			Type = type;
			Position = position;
			ClassName = className;
		}
	}

	/// <summary>
	/// Manages piece creation and army composition for one side of the chess board
	/// </summary>
	public partial class Army
	{
		public PieceColor Color { get; private set; }
		private Dictionary<(PieceType, int), string> pieceClasses = new();

		public Army(PieceColor color)
		{
			Color = color;
			InitializeStandardArmy();
		}

		private void InitializeStandardArmy()
		{
			for (int i = 0; i < 8; i++)
			{
				SetPieceClass(PieceType.Pawn, i, "Standard");
			}

			SetPieceClass(PieceType.Rook, 0, "Standard");
			SetPieceClass(PieceType.Knight, 1, "Standard");
			SetPieceClass(PieceType.Bishop, 2, "Standard");
			SetPieceClass(PieceType.Queen, 3, "Standard");
			SetPieceClass(PieceType.King, 4, "Standard");
			SetPieceClass(PieceType.Bishop, 5, "Standard");
			SetPieceClass(PieceType.Knight, 6, "Standard");
			SetPieceClass(PieceType.Rook, 7, "Standard");
		}

		public void SetPieceClass(PieceType type, int position, string className)
		{
			pieceClasses[(type, position)] = className;
		}

		public string GetPieceClass(PieceType type, int position)
		{
			return pieceClasses.TryGetValue((type, position), out string? className) ? className : "Standard";
		}

		/// <summary>
		/// Creates a piece instance based on type, position, and assigned class variant
		/// </summary>
		public Piece CreatePiece(PieceType type, int position)
		{
			string className = GetPieceClass(type, position);
			Piece piece = CreatePieceByClass(type, className);
			piece.Color = Color;
			return piece;
		}

		private Piece CreatePieceByClass(PieceType type, string className)
		{
			return type switch
			{
				PieceType.Pawn => CreatePawn(className),
				PieceType.Knight => CreateKnight(className),
				PieceType.Bishop => CreateBishop(className),
				PieceType.Rook => CreateRook(className),
				PieceType.Queen => CreateQueen(className),
				PieceType.King => CreateKing(className),
				_ => throw new ArgumentException($"Unknown piece type: {type}"),
			};
		}

		private Piece CreatePawn(string className)
		{
			return className switch
			{
				"Ranger" => new RangerPawn(),
				"Guard" => new GuardPawn(),
				_ => new Pawn(),
			};
		}

		private Piece CreateKnight(string className)
		{
			return className switch
			{
				"Charge" => new ChargeKnight(),
				_ => new Knight(),
			};
		}

		private Piece CreateBishop(string className)
		{
			return className switch
			{
				"Freezing" => new FreezingBishop(),
				_ => new Bishop(),
			};
		}

		private Piece CreateRook(string className)
		{
			return className switch
			{
				"Bombing" => new BombingRook(),
				_ => new Rook(),
			};
		}

		private Piece CreateQueen(string className)
		{
			return className switch
			{
				"Glass" => new GlassQueen(),
				_ => new Queen(),
			};
		}

		private Piece CreateKing(string className)
		{
			return className switch
			{
				"Resurrecting" => new ResurrectingKing(),
				_ => new King(),
			};
		}

		public List<PieceClassAssignment> GetArmyComposition()
		{
			var composition = new List<PieceClassAssignment>();
			foreach (var kvp in pieceClasses)
			{
				composition.Add(new PieceClassAssignment(kvp.Key.Item1, kvp.Key.Item2, kvp.Value));
			}
			return composition;
		}

		public void LoadArmyComposition(List<PieceClassAssignment> composition)
		{
			pieceClasses.Clear();
			foreach (var assignment in composition)
			{
				SetPieceClass(assignment.Type, assignment.Position, assignment.ClassName);
			}
		}

		/// <summary>
		/// Serializes the army configuration to a JSON string for network transmission
		/// </summary>
		public string Serialize()
		{
			var composition = GetArmyComposition();
			return JsonSerializer.Serialize(composition);
		}

		/// <summary>
		/// Deserializes army configuration from a JSON string
		/// </summary>
		public static Army Deserialize(string json, PieceColor color)
		{
			var army = new Army(color);
			var composition = JsonSerializer.Deserialize<List<PieceClassAssignment>>(json);
			if (composition != null)
			{
				army.LoadArmyComposition(composition);
			}
			return army;
		}
	}
}

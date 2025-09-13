using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChessPlusPlus.Pieces;
using Godot;

namespace ChessPlusPlus.Core
{
	public class PieceClassInfo
	{
		public string ClassName { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public Type ClassType { get; set; } = null!;
		public PieceType PieceType { get; set; }

		public PieceClassInfo(string className, string displayName, string description, Type classType, PieceType pieceType)
		{
			ClassName = className;
			DisplayName = displayName;
			Description = description;
			ClassType = classType;
			PieceType = pieceType;
		}
	}

	public static class PieceRegistry
	{
		private static Dictionary<PieceType, List<PieceClassInfo>> registeredClasses = new();
		private static bool initialized = false;

		public static void Initialize()
		{
			if (initialized)
				return;

			registeredClasses.Clear();

			// Get all types in the current assembly
			var assembly = Assembly.GetExecutingAssembly();
			var pieceTypes = assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(Piece)) && !t.IsAbstract);

			foreach (var type in pieceTypes)
			{
				try
				{
					// Create a temporary instance to get piece information
					var piece = (Piece)Activator.CreateInstance(type)!;
					var pieceType = piece.Type;
					var className = piece.ClassName;

					// Generate display name and description
					var displayName = GenerateDisplayName(className, type.Name);
					var description = GenerateDescription(className, type.Name, pieceType);

					var classInfo = new PieceClassInfo(className, displayName, description, type, pieceType);

					if (!registeredClasses.ContainsKey(pieceType))
					{
						registeredClasses[pieceType] = new List<PieceClassInfo>();
					}

					registeredClasses[pieceType].Add(classInfo);
					GD.Print($"Registered piece class: {pieceType} - {className} ({type.Name})");
				}
				catch (Exception ex)
				{
					GD.PrintErr($"Failed to register piece class {type.Name}: {ex.Message}");
				}
			}

			// Sort classes by name for consistent ordering
			foreach (var list in registeredClasses.Values)
			{
				list.Sort((a, b) => a.ClassName.CompareTo(b.ClassName));
			}

			initialized = true;
			GD.Print($"PieceRegistry initialized with {registeredClasses.Sum(kvp => kvp.Value.Count)} piece classes");
		}

		public static List<PieceClassInfo> GetAvailableClasses(PieceType pieceType)
		{
			if (!initialized)
				Initialize();

			return registeredClasses.TryGetValue(pieceType, out var classes)
				? new List<PieceClassInfo>(classes)
				: new List<PieceClassInfo>();
		}

		public static List<string> GetAvailableClassNames(PieceType pieceType)
		{
			return GetAvailableClasses(pieceType)
				.Select(info => info.ClassName)
				.ToList();
		}

		public static PieceClassInfo? GetClassInfo(PieceType pieceType, string className)
		{
			if (!initialized)
				Initialize();

			return registeredClasses.TryGetValue(pieceType, out var classes)
				? classes.FirstOrDefault(info => info.ClassName == className)
				: null;
		}

		public static bool IsValidClass(PieceType pieceType, string className)
		{
			return GetClassInfo(pieceType, className) != null;
		}

		private static string GenerateDisplayName(string className, string typeName)
		{
			// If className is "Standard", use a friendly name based on the type
			if (className == "Standard")
			{
				return "Standard";
			}

			// For custom classes, use the className directly
			return className;
		}

		private static string GenerateDescription(string className, string typeName, PieceType pieceType)
		{
			// Generate descriptions based on known classes
			return className switch
			{
				"Standard" => $"Standard {pieceType.ToString().ToLower()} with traditional movement",
				"Ranger" when pieceType == PieceType.Pawn => "Can move 2 squares forward at any time and capture backwards",
				"Guard" when pieceType == PieceType.Pawn => "Enhanced defensive capabilities",
				"Charge" when pieceType == PieceType.Knight => "Aggressive knight variant with special charge ability",
				_ => $"{className} variant of {pieceType.ToString().ToLower()}"
			};
		}
	}
}

namespace ChessPlusPlus.AI
{
	using ChessPlusPlus.Players;
	using Godot;

	/// <summary>
	/// Configuration settings for AI difficulty tuning
	/// </summary>
	[GlobalClass]
	public partial class AIConfig : Resource
	{
		[ExportGroup("Easy Difficulty")]
		[Export]
		public int EasySearchDepth { get; set; } = 1;

		[Export]
		public float EasyRandomnessFactor { get; set; } = 0.3f;

		[Export]
		public int EasyTopMovesPool { get; set; } = 5;

		[Export]
		public float EasyPositionWeight { get; set; } = 0.5f;

		[Export]
		public float EasyAbilityWeight { get; set; } = 0.7f;

		[Export]
		public bool EasyConsiderThreats { get; set; } = false;

		[ExportGroup("Medium Difficulty")]
		[Export]
		public int MediumSearchDepth { get; set; } = 2;

		[Export]
		public float MediumRandomnessFactor { get; set; } = 0.15f;

		[Export]
		public int MediumTopMovesPool { get; set; } = 3;

		[Export]
		public float MediumPositionWeight { get; set; } = 1.0f;

		[Export]
		public float MediumAbilityWeight { get; set; } = 1.0f;

		[Export]
		public bool MediumConsiderThreats { get; set; } = true;

		[ExportGroup("Hard Difficulty")]
		[Export]
		public int HardSearchDepth { get; set; } = 3;

		[Export]
		public float HardRandomnessFactor { get; set; } = 0.05f;

		[Export]
		public int HardTopMovesPool { get; set; } = 2;

		[Export]
		public float HardPositionWeight { get; set; } = 1.2f;

		[Export]
		public float HardAbilityWeight { get; set; } = 1.3f;

		[Export]
		public bool HardConsiderThreats { get; set; } = true;

		[ExportGroup("Expert Difficulty (Future)")]
		[Export]
		public int ExpertSearchDepth { get; set; } = 5;

		[Export]
		public float ExpertRandomnessFactor { get; set; } = 0.0f;

		[Export]
		public int ExpertTopMovesPool { get; set; } = 1;

		[Export]
		public float ExpertPositionWeight { get; set; } = 1.5f;

		[Export]
		public float ExpertAbilityWeight { get; set; } = 1.5f;

		[Export]
		public bool ExpertConsiderThreats { get; set; } = true;

		[Export]
		public bool ExpertUseOpeningBook { get; set; } = true;

		[Export]
		public bool ExpertUseEndgameTablebase { get; set; } = true;

		[ExportGroup("Learning")]
		[Export]
		public bool EnableLearning { get; set; } = true;

		[Export]
		public float LearningRate { get; set; } = 0.1f;

		[Export]
		public string LearnedValuesPath { get; set; } = "user://ai_learned_values.json";

		[ExportGroup("Performance")]
		[Export]
		public int MaxThinkingTime { get; set; } = 5000; // milliseconds

		[Export]
		public int TranspositionTableSize { get; set; } = 1000000;

		[Export]
		public bool UseParallelSearch { get; set; } = false;

		/// <summary>
		/// Get settings for a specific difficulty level
		/// </summary>
		public DifficultySettings GetDifficultySettings(AIDifficulty difficulty)
		{
			return difficulty switch
			{
				AIDifficulty.Easy => new DifficultySettings
				{
					SearchDepth = EasySearchDepth,
					RandomnessFactor = EasyRandomnessFactor,
					TopMovesPool = EasyTopMovesPool,
					PositionWeight = EasyPositionWeight,
					AbilityWeight = EasyAbilityWeight,
					ConsiderThreats = EasyConsiderThreats,
				},
				AIDifficulty.Medium => new DifficultySettings
				{
					SearchDepth = MediumSearchDepth,
					RandomnessFactor = MediumRandomnessFactor,
					TopMovesPool = MediumTopMovesPool,
					PositionWeight = MediumPositionWeight,
					AbilityWeight = MediumAbilityWeight,
					ConsiderThreats = MediumConsiderThreats,
				},
				AIDifficulty.Hard => new DifficultySettings
				{
					SearchDepth = HardSearchDepth,
					RandomnessFactor = HardRandomnessFactor,
					TopMovesPool = HardTopMovesPool,
					PositionWeight = HardPositionWeight,
					AbilityWeight = HardAbilityWeight,
					ConsiderThreats = HardConsiderThreats,
				},
				_ => new DifficultySettings
				{
					SearchDepth = MediumSearchDepth,
					RandomnessFactor = MediumRandomnessFactor,
					TopMovesPool = MediumTopMovesPool,
					PositionWeight = MediumPositionWeight,
					AbilityWeight = MediumAbilityWeight,
					ConsiderThreats = MediumConsiderThreats,
				},
			};
		}
	}

	/// <summary>
	/// Settings for a specific difficulty level
	/// </summary>
	public struct DifficultySettings
	{
		public int SearchDepth { get; set; }
		public float RandomnessFactor { get; set; }
		public int TopMovesPool { get; set; }
		public float PositionWeight { get; set; }
		public float AbilityWeight { get; set; }
		public bool ConsiderThreats { get; set; }
	}
}

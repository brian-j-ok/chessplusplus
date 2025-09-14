namespace ChessPlusPlus.Core
{
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	public enum GameMode
	{
		PlayerVsPlayer,
		PlayerVsAI,
		AIVsAI,
	}

	public partial class GameConfig : Resource
	{
		[Export]
		public PieceColor PlayerColor { get; set; } = PieceColor.White;

		[Export]
		public bool FlipBoardForBlack { get; set; } = true;

		[Export]
		public GameMode Mode { get; set; } = GameMode.PlayerVsPlayer; // Default to dev mode

		[Export]
		public AIDifficulty AIDifficulty { get; set; } = AIDifficulty.Medium;

		private Army? customPlayerArmy = null;

		public static GameConfig Instance { get; private set; } = new GameConfig();

		public void SetPlayerColor(PieceColor color)
		{
			PlayerColor = color;
		}

		public bool IsPlayerWhite()
		{
			return PlayerColor == PieceColor.White;
		}

		/// <summary>
		/// Determines if the board should be flipped to show the player's pieces at the bottom
		/// </summary>
		public bool ShouldFlipBoard()
		{
			return FlipBoardForBlack && PlayerColor == PieceColor.White;
		}

		public void SetCustomArmy(Army army)
		{
			customPlayerArmy = army;
		}

		public Army? GetCustomArmy()
		{
			return customPlayerArmy;
		}

		public bool HasCustomArmy()
		{
			return customPlayerArmy != null;
		}

		public void ClearCustomArmy()
		{
			customPlayerArmy = null;
		}
	}
}

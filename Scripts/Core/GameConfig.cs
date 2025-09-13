namespace ChessPlusPlus.Core
{
	using ChessPlusPlus.Pieces;
	using Godot;

	public partial class GameConfig : Resource
	{
		[Export] public PieceColor PlayerColor { get; set; } = PieceColor.White;
		[Export] public bool FlipBoardForBlack { get; set; } = true;
		[Export] public string GameMode { get; set; } = "Standard";

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
	}
}

namespace ChessPlusPlus.Core.Managers
{
	using ChessPlusPlus.Network;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	/// <summary>
	/// Manages player lifecycle, creation, and initialization
	/// </summary>
	public partial class PlayerManager : Node
	{
		public PlayerController? WhitePlayer { get; private set; }
		public PlayerController? BlackPlayer { get; private set; }
		public HumanPlayerController? HumanPlayer { get; private set; }

		/// <summary>
		/// Initializes players based on game configuration
		/// </summary>
		public void InitializePlayers(Board board, GameManager gameManager)
		{
			CleanupExistingPlayers();

			var config = GameConfig.Instance;
			GD.Print($"Initializing players for mode: {config.Mode}");

			// Check if we're in network mode
			var networkManager = NetworkManager.Instance;
			if (networkManager.IsConnected)
			{
				InitializeNetworkPlayers(board, gameManager, config);
				return;
			}

			// Initialize local players based on game mode
			switch (config.Mode)
			{
				case GameMode.PlayerVsPlayer:
					InitializePlayerVsPlayer(board, gameManager);
					break;

				case GameMode.PlayerVsAI:
					InitializePlayerVsAI(board, gameManager, config);
					break;

				case GameMode.AIVsAI:
					InitializeAIVsAI(board, gameManager, config);
					break;
			}
		}

		/// <summary>
		/// Cleans up existing players
		/// </summary>
		private void CleanupExistingPlayers()
		{
			if (WhitePlayer != null)
			{
				WhitePlayer.QueueFree();
				WhitePlayer = null;
			}
			if (BlackPlayer != null)
			{
				BlackPlayer.QueueFree();
				BlackPlayer = null;
			}
			HumanPlayer = null;
		}

		/// <summary>
		/// Initializes players for network game
		/// </summary>
		private void InitializeNetworkPlayers(Board board, GameManager gameManager, GameConfig config)
		{
			GD.Print("Setting up network game");

			if (config.PlayerColor == PieceColor.White)
			{
				WhitePlayer = CreateNetworkPlayer(PieceColor.White, true, board, gameManager);
				BlackPlayer = CreateNetworkPlayer(PieceColor.Black, false, board, gameManager);
			}
			else
			{
				WhitePlayer = CreateNetworkPlayer(PieceColor.White, false, board, gameManager);
				BlackPlayer = CreateNetworkPlayer(PieceColor.Black, true, board, gameManager);
			}
		}

		/// <summary>
		/// Initializes players for Player vs Player mode
		/// </summary>
		private void InitializePlayerVsPlayer(Board board, GameManager gameManager)
		{
			WhitePlayer = CreateHumanPlayer(PieceColor.White);
			BlackPlayer = CreateHumanPlayer(PieceColor.Black);
			HumanPlayer = WhitePlayer as HumanPlayerController;

			WhitePlayer.Initialize(board, gameManager);
			BlackPlayer.Initialize(board, gameManager);
		}

		/// <summary>
		/// Initializes players for Player vs AI mode
		/// </summary>
		private void InitializePlayerVsAI(Board board, GameManager gameManager, GameConfig config)
		{
			if (config.PlayerColor == PieceColor.White)
			{
				WhitePlayer = CreateHumanPlayer(PieceColor.White);
				BlackPlayer = CreateAIPlayer(PieceColor.Black, config.AIDifficulty);
				HumanPlayer = WhitePlayer as HumanPlayerController;
			}
			else
			{
				WhitePlayer = CreateAIPlayer(PieceColor.White, config.AIDifficulty);
				BlackPlayer = CreateHumanPlayer(PieceColor.Black);
				HumanPlayer = BlackPlayer as HumanPlayerController;
			}

			WhitePlayer.Initialize(board, gameManager);
			BlackPlayer.Initialize(board, gameManager);
		}

		/// <summary>
		/// Initializes players for AI vs AI mode
		/// </summary>
		private void InitializeAIVsAI(Board board, GameManager gameManager, GameConfig config)
		{
			WhitePlayer = CreateAIPlayer(PieceColor.White, config.AIDifficulty);
			BlackPlayer = CreateAIPlayer(PieceColor.Black, config.AIDifficulty);
			HumanPlayer = null;

			WhitePlayer.Initialize(board, gameManager);
			BlackPlayer.Initialize(board, gameManager);
		}

		/// <summary>
		/// Creates a human player controller
		/// </summary>
		private HumanPlayerController CreateHumanPlayer(PieceColor color)
		{
			var player = new HumanPlayerController();
			player.PlayerColor = color;
			player.PlayerName = $"Human ({color})";
			AddChild(player);
			return player;
		}

		/// <summary>
		/// Creates an AI player controller
		/// </summary>
		private AIPlayerController CreateAIPlayer(PieceColor color, AIDifficulty difficulty)
		{
			var player = new AIPlayerController();
			player.PlayerColor = color;
			player.Difficulty = difficulty;
			player.PlayerName = $"AI ({color}, {difficulty})";
			AddChild(player);
			return player;
		}

		/// <summary>
		/// Creates a network player controller
		/// </summary>
		private PlayerController CreateNetworkPlayer(
			PieceColor color,
			bool isLocal,
			Board board,
			GameManager gameManager
		)
		{
			var player = new LocalNetworkPlayerController();
			player.PlayerColor = color;
			player.IsLocalPlayer = isLocal;
			player.PlayerName = isLocal ? $"You ({color})" : $"Opponent ({color})";
			AddChild(player);
			player.Initialize(board, gameManager);

			if (isLocal)
			{
				HumanPlayer = player;
			}

			return player;
		}

		/// <summary>
		/// Gets the player controller for the specified color
		/// </summary>
		public PlayerController? GetPlayer(PieceColor color)
		{
			return color == PieceColor.White ? WhitePlayer : BlackPlayer;
		}

		/// <summary>
		/// Gets the current human player for the current turn in dev mode
		/// </summary>
		public HumanPlayerController? GetCurrentHumanPlayer(PieceColor currentTurn)
		{
			if (GameConfig.Instance.Mode == GameMode.PlayerVsPlayer)
			{
				return currentTurn == PieceColor.White
					? WhitePlayer as HumanPlayerController
					: BlackPlayer as HumanPlayerController;
			}
			return HumanPlayer;
		}
	}
}

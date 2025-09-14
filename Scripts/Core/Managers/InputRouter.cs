namespace ChessPlusPlus.Core.Managers
{
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	/// <summary>
	/// Routes input events to the appropriate player controllers
	/// </summary>
	public partial class InputRouter : Node
	{
		private PlayerManager playerManager = null!;
		private GameStateManager gameStateManager = null!;
		private TurnManager turnManager = null!;

		/// <summary>
		/// Initializes the input router with required managers
		/// </summary>
		public void Initialize(PlayerManager playerManager, GameStateManager gameStateManager, TurnManager turnManager)
		{
			this.playerManager = playerManager;
			this.gameStateManager = gameStateManager;
			this.turnManager = turnManager;
		}

		/// <summary>
		/// Handles input events and routes them to the appropriate player
		/// </summary>
		public void HandleInput(InputEvent @event)
		{
			// Only process input during active game states
			if (!IsInputAllowed())
				return;

			if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
			{
				RouteMouseInput(mouseButton);
			}
		}

		/// <summary>
		/// Determines if input should be processed based on game state
		/// </summary>
		private bool IsInputAllowed()
		{
			var state = gameStateManager.CurrentState;
			return state == GameState.Playing || state == GameState.Check;
		}

		/// <summary>
		/// Routes mouse input to the appropriate player controller
		/// </summary>
		private void RouteMouseInput(InputEventMouseButton mouseButton)
		{
			var config = GameConfig.Instance;
			HumanPlayerController? targetPlayer = null;

			// Determine which player should receive the input
			if (config.Mode == GameMode.PlayerVsPlayer)
			{
				// In dev mode, route to the current player
				targetPlayer = playerManager.GetCurrentHumanPlayer(turnManager.CurrentTurn);
			}
			else
			{
				// In other modes, route to the single human player
				targetPlayer = playerManager.HumanPlayer;
			}

			if (targetPlayer == null)
				return;

			// Route the input based on button type
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				targetPlayer.HandleBoardClick(mouseButton.Position);
			}
			else if (mouseButton.ButtonIndex == MouseButton.Right)
			{
				targetPlayer.ClearSelection();
			}
		}

		/// <summary>
		/// Handles keyboard input for game controls
		/// </summary>
		public void HandleKeyboardInput(InputEventKey keyEvent)
		{
			if (!keyEvent.Pressed)
				return;

			switch (keyEvent.Keycode)
			{
				case Key.Escape:
					HandleEscapeKey();
					break;
				case Key.R:
					if (keyEvent.CtrlPressed)
						HandleResetRequest();
					break;
			}
		}

		/// <summary>
		/// Handles the escape key press
		/// </summary>
		private void HandleEscapeKey()
		{
			// Clear selection for the active player
			var humanPlayer = playerManager.GetCurrentHumanPlayer(turnManager.CurrentTurn);
			humanPlayer?.ClearSelection();
		}

		/// <summary>
		/// Handles reset game request
		/// </summary>
		private void HandleResetRequest()
		{
			GD.Print("Reset game requested");
			// This would trigger a game reset through the GameManager
		}
	}
}

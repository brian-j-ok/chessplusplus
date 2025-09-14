using ChessPlusPlus.Core;
using ChessPlusPlus.Pieces;
using Godot;

namespace ChessPlusPlus.UI
{
	public partial class GameUI : Control
	{
		private Label? whiteTimerLabel;
		private Label? blackTimerLabel;
		private Label? turnIndicatorLabel;
		private GameManager? gameManager;
		private ChessPlusPlus.Core.Managers.TimerManager? timerManager;
		private ChessPlusPlus.Core.Managers.TurnManager? turnManager;

		public override void _Ready()
		{
			// Create UI container
			var vbox = new VBoxContainer();
			vbox.Position = new Vector2(10, 10);
			AddChild(vbox);

			// Create turn indicator
			turnIndicatorLabel = new Label();
			turnIndicatorLabel.Text = "Current Turn: White";
			turnIndicatorLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
			turnIndicatorLabel.AddThemeFontSizeOverride("font_size", 24);
			vbox.AddChild(turnIndicatorLabel);

			// Add spacing
			vbox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });

			// Create timer container
			var timerContainer = new HBoxContainer();
			timerContainer.AddThemeConstantOverride("separation", 50);
			vbox.AddChild(timerContainer);

			// White timer
			var whiteContainer = new VBoxContainer();
			timerContainer.AddChild(whiteContainer);

			var whiteLabel = new Label();
			whiteLabel.Text = "White";
			whiteLabel.AddThemeFontSizeOverride("font_size", 18);
			whiteContainer.AddChild(whiteLabel);

			whiteTimerLabel = new Label();
			whiteTimerLabel.Text = "10:00";
			whiteTimerLabel.AddThemeFontSizeOverride("font_size", 28);
			whiteTimerLabel.AddThemeColorOverride("font_color", Colors.White);
			whiteContainer.AddChild(whiteTimerLabel);

			// Black timer
			var blackContainer = new VBoxContainer();
			timerContainer.AddChild(blackContainer);

			var blackLabel = new Label();
			blackLabel.Text = "Black";
			blackLabel.AddThemeFontSizeOverride("font_size", 18);
			blackContainer.AddChild(blackLabel);

			blackTimerLabel = new Label();
			blackTimerLabel.Text = "10:00";
			blackTimerLabel.AddThemeFontSizeOverride("font_size", 28);
			blackTimerLabel.AddThemeColorOverride("font_color", Colors.DarkGray);
			blackContainer.AddChild(blackTimerLabel);

			// Find and connect to GameManager and its managers
			gameManager = GetNode<GameManager>("/root/Game");
			if (gameManager != null)
			{
				timerManager = gameManager.TimerManager;
				turnManager = gameManager.TurnManager;

				if (timerManager != null)
				{
					timerManager.TimerUpdated += OnTimerUpdated;
				}

				if (turnManager != null)
				{
					turnManager.TurnChanged += (turnInt) => OnTurnChanged((PieceColor)turnInt);
				}
			}
		}

		private void OnTimerUpdated(float whiteTime, float blackTime)
		{
			if (whiteTimerLabel != null)
			{
				whiteTimerLabel.Text = FormatTime(whiteTime);
			}

			if (blackTimerLabel != null)
			{
				blackTimerLabel.Text = FormatTime(blackTime);
			}
		}

		private void OnTurnChanged(PieceColor turn)
		{
			if (turnIndicatorLabel != null)
			{
				turnIndicatorLabel.Text = $"Current Turn: {turn}";

				// Highlight active timer
				if (whiteTimerLabel != null && blackTimerLabel != null)
				{
					if (turn == PieceColor.White)
					{
						whiteTimerLabel.AddThemeColorOverride("font_color", Colors.Yellow);
						blackTimerLabel.AddThemeColorOverride("font_color", Colors.DarkGray);
					}
					else
					{
						whiteTimerLabel.AddThemeColorOverride("font_color", Colors.White);
						blackTimerLabel.AddThemeColorOverride("font_color", Colors.Yellow);
					}
				}
			}
		}

		private string FormatTime(float seconds)
		{
			int minutes = (int)(seconds / 60);
			int secs = (int)(seconds % 60);
			return $"{minutes:D2}:{secs:D2}";
		}

		public override void _ExitTree()
		{
			if (timerManager != null)
			{
				timerManager.TimerUpdated -= OnTimerUpdated;
			}

			if (turnManager != null)
			{
				// TurnChanged unsubscribe is handled by Godot's lifecycle
			}
		}
	}
}

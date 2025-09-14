using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Managers;
using ChessPlusPlus.Pieces;
using ChessPlusPlus.UI.Builders;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;
using StylePresets = ChessPlusPlus.UI.Styles.StylePresets;
using UIBuilder = ChessPlusPlus.UI.Builders.UI;

namespace ChessPlusPlus.UI.Screens
{
	public partial class GameUIScreen : Control
	{
		private Label? whiteTimerLabel;
		private Label? blackTimerLabel;
		private Label? turnIndicatorLabel;
		private GameManager? gameManager;
		private TimerManager? timerManager;
		private TurnManager? turnManager;

		public override void _Ready()
		{
			BuildUI();
			ConnectToManagers();
		}

		private void BuildUI()
		{
			var mainContainer = UIBuilder
				.VBox()
				.Position(10, 10)
				.Spacing(StylePresets.Spacing.Medium)
				.Children(BuildTurnIndicator(), UIBuilder.Spacer(0, StylePresets.Spacing.Large), BuildTimerPanel())
				.AddTo(this);
		}

		private Control BuildTurnIndicator()
		{
			turnIndicatorLabel = UIBuilder
				.Label("Current Turn: White")
				.FontSize(StylePresets.FontSizes.Large)
				.FontColor(ColorPalette.TextPrimary)
				.Build();

			return turnIndicatorLabel;
		}

		private Control BuildTimerPanel()
		{
			return UIBuilder
				.HBox()
				.Spacing(StylePresets.Spacing.ExtraLarge)
				.Children(
					BuildPlayerTimer("White", ColorPalette.WhitePlayer, out whiteTimerLabel),
					BuildPlayerTimer("Black", ColorPalette.BlackPlayer, out blackTimerLabel)
				)
				.Build();
		}

		private Control BuildPlayerTimer(string playerName, Color timerColor, out Label timerLabel)
		{
			var nameLabel = UIBuilder
				.Label(playerName)
				.FontSize(StylePresets.FontSizes.Medium)
				.FontColor(ColorPalette.TextSecondary)
				.Build();

			timerLabel = UIBuilder.Label("10:00").FontSize(StylePresets.FontSizes.Huge).FontColor(timerColor).Build();

			return UIBuilder.VBox().Spacing(StylePresets.Spacing.Small).Children(nameLabel, timerLabel).Build();
		}

		private void ConnectToManagers()
		{
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
						whiteTimerLabel.AddThemeColorOverride("font_color", ColorPalette.TextAccent);
						blackTimerLabel.AddThemeColorOverride("font_color", ColorPalette.BlackPlayer);
					}
					else
					{
						whiteTimerLabel.AddThemeColorOverride("font_color", ColorPalette.WhitePlayer);
						blackTimerLabel.AddThemeColorOverride("font_color", ColorPalette.TextAccent);
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

using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Managers;
using ChessPlusPlus.Pieces;
using ChessPlusPlus.UI.Builders;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;
using StylePresets = ChessPlusPlus.UI.Styles.StylePresets;
using UIBuilder = ChessPlusPlus.UI.Builders.UI;

namespace ChessPlusPlus.UI.Components
{
	public partial class GameInfoPanel : VBoxContainer
	{
		private Label? whiteTimerLabel;
		private Label? blackTimerLabel;
		private Label? turnIndicatorLabel;
		private Panel? whiteTimerPanel;
		private Panel? blackTimerPanel;
		private GameManager? gameManager;
		private TimerManager? timerManager;
		private TurnManager? turnManager;

		public override void _Ready()
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter; // Don't expand vertically
			AddThemeConstantOverride("separation", StylePresets.Spacing.Medium);
			BuildUI();

			// Defer connection to ensure GameManager is fully initialized
			CallDeferred(nameof(ConnectToManagers));
		}

		private void BuildUI()
		{
			AddChild(BuildTurnIndicatorSection());
			AddChild(new HSeparator());
			AddChild(BuildTimersSection());
		}

		private Control BuildTurnIndicatorSection()
		{
			var section = new VBoxContainer();
			section.AddThemeConstantOverride("separation", 8);
			section.CustomMinimumSize = new Vector2(0, 80);
			section.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			var headerLabel = UIBuilder
				.Label("CURRENT TURN")
				.FontSize(StylePresets.FontSizes.Small)
				.FontColor(ColorPalette.TextSecondary)
				.Build();
			headerLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			headerLabel.SetHorizontalAlignment(HorizontalAlignment.Center);
			section.AddChild(headerLabel);

			turnIndicatorLabel = UIBuilder
				.Label("White")
				.FontSize(StylePresets.FontSizes.ExtraLarge)
				.FontColor(ColorPalette.TextPrimary)
				.Build();
			turnIndicatorLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			turnIndicatorLabel.SetHorizontalAlignment(HorizontalAlignment.Center);
			section.AddChild(turnIndicatorLabel);

			return section;
		}

		private Control BuildTimersSection()
		{
			var section = new VBoxContainer();
			section.AddThemeConstantOverride("separation", 12);
			section.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			var headerLabel = UIBuilder
				.Label("TIMERS")
				.FontSize(StylePresets.FontSizes.Small)
				.FontColor(ColorPalette.TextSecondary)
				.Build();
			headerLabel.CustomMinimumSize = new Vector2(0, 25);
			headerLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			headerLabel.SetHorizontalAlignment(HorizontalAlignment.Center);
			section.AddChild(headerLabel);

			whiteTimerPanel = BuildTimerPanel("White", ColorPalette.WhitePlayer, out whiteTimerLabel);
			section.AddChild(whiteTimerPanel);

			blackTimerPanel = BuildTimerPanel("Black", ColorPalette.BlackPlayer, out blackTimerLabel);
			section.AddChild(blackTimerPanel);

			return section;
		}

		private Panel BuildTimerPanel(string playerName, Color playerColor, out Label timerLabel)
		{
			var panel = new Panel();
			panel.CustomMinimumSize = new Vector2(0, 85);
			panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			var styleBox = new StyleBoxFlat
			{
				BgColor = ColorPalette.BackgroundDark,
				BorderWidthLeft = 4,
				BorderColor = playerColor,
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6,
				CornerRadiusBottomRight = 6,
				ContentMarginLeft = 20,
				ContentMarginRight = 20,
				ContentMarginTop = 14,
				ContentMarginBottom = 14,
			};
			panel.AddThemeStyleboxOverride("panel", styleBox);

			var container = new VBoxContainer();
			container.AddThemeConstantOverride("separation", 4);
			container.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			container.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			// Create horizontal container for centered content
			var nameContainer = new HBoxContainer();
			nameContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			nameContainer.AddThemeConstantOverride("separation", 0);

			// Add spacer before
			var spacer1 = new Control();
			spacer1.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			nameContainer.AddChild(spacer1);

			var nameLabel = UIBuilder
				.Label(playerName.ToUpper())
				.FontSize(StylePresets.FontSizes.Small)
				.FontColor(ColorPalette.TextSecondary)
				.Build();
			nameLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			nameContainer.AddChild(nameLabel);

			// Add spacer after
			var spacer2 = new Control();
			spacer2.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			nameContainer.AddChild(spacer2);

			container.AddChild(nameContainer);

			// Timer label - also centered
			var timerContainer = new HBoxContainer();
			timerContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			timerContainer.AddThemeConstantOverride("separation", 0);

			var spacer3 = new Control();
			spacer3.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			timerContainer.AddChild(spacer3);

			timerLabel = UIBuilder.Label("10:00").FontSize(StylePresets.FontSizes.Huge).FontColor(playerColor).Build();
			timerLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			timerContainer.AddChild(timerLabel);

			var spacer4 = new Control();
			spacer4.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			timerContainer.AddChild(spacer4);

			container.AddChild(timerContainer);

			panel.AddChild(container);
			return panel;
		}

		private void ConnectToManagers()
		{
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
				turnIndicatorLabel.Text = turn.ToString();
				turnIndicatorLabel.AddThemeColorOverride(
					"font_color",
					turn == PieceColor.White ? ColorPalette.WhitePlayer : ColorPalette.BlackPlayer
				);
			}

			if (whiteTimerPanel != null && blackTimerPanel != null)
			{
				var whiteStyle = whiteTimerPanel.GetThemeStylebox("panel") as StyleBoxFlat;
				var blackStyle = blackTimerPanel.GetThemeStylebox("panel") as StyleBoxFlat;

				if (whiteStyle != null && blackStyle != null)
				{
					if (turn == PieceColor.White)
					{
						whiteStyle.BorderWidthLeft = 4;
						whiteStyle.BorderColor = ColorPalette.TextAccent;
						whiteStyle.BgColor = new Color(
							ColorPalette.BackgroundDark.R,
							ColorPalette.BackgroundDark.G,
							ColorPalette.BackgroundDark.B,
							1.0f
						);
						blackStyle.BorderWidthLeft = 2;
						blackStyle.BorderColor = ColorPalette.BlackPlayer;
						blackStyle.BgColor = new Color(
							ColorPalette.BackgroundDark.R,
							ColorPalette.BackgroundDark.G,
							ColorPalette.BackgroundDark.B,
							0.5f
						);
					}
					else
					{
						blackStyle.BorderWidthLeft = 4;
						blackStyle.BorderColor = ColorPalette.TextAccent;
						blackStyle.BgColor = new Color(
							ColorPalette.BackgroundDark.R,
							ColorPalette.BackgroundDark.G,
							ColorPalette.BackgroundDark.B,
							1.0f
						);
						whiteStyle.BorderWidthLeft = 2;
						whiteStyle.BorderColor = ColorPalette.WhitePlayer;
						whiteStyle.BgColor = new Color(
							ColorPalette.BackgroundDark.R,
							ColorPalette.BackgroundDark.G,
							ColorPalette.BackgroundDark.B,
							0.5f
						);
					}

					whiteTimerPanel.AddThemeStyleboxOverride("panel", whiteStyle);
					blackTimerPanel.AddThemeStyleboxOverride("panel", blackStyle);
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
		}
	}
}

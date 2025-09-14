using ChessPlusPlus.Pieces;
using ChessPlusPlus.UI.Builders;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;
using StylePresets = ChessPlusPlus.UI.Styles.StylePresets;
using UIBuilder = ChessPlusPlus.UI.Builders.UI;

namespace ChessPlusPlus.UI.Screens
{
	public partial class PromotionDialogScreen : Control
	{
		[Signal]
		public delegate void PieceSelectedEventHandler(int selectedType);

		private PieceColor promotingColor;
		private Panel? dialogPanel;

		public override void _Ready()
		{
			Visible = false;
			MouseFilter = MouseFilterEnum.Stop; // Block input to underlying elements
			BuildUI();
		}

		private void BuildUI()
		{
			// Full-screen overlay
			var overlayColor = new Color(
				ColorPalette.BackgroundDark.R,
				ColorPalette.BackgroundDark.G,
				ColorPalette.BackgroundDark.B,
				0.7f
			);
			var overlay = UIBuilder.ColorRect(overlayColor).FullRect().AddTo(this);

			// Dialog panel
			dialogPanel = UIBuilder
				.Panel()
				.Size(400, 200)
				.Center()
				.Style(ColorPalette.BackgroundMedium, ColorPalette.BorderAccent, 3, 10)
				.Visible(false)
				.AddTo(this);
		}

		public void ShowPromotionDialog(PieceColor color)
		{
			promotingColor = color;
			Visible = true;

			// Clear existing content
			if (dialogPanel != null)
			{
				foreach (Node child in dialogPanel.GetChildren())
				{
					child.QueueFree();
				}

				// Create dialog content
				CreatePromotionContent();
				dialogPanel.Visible = true;
			}
		}

		private void CreatePromotionContent()
		{
			if (dialogPanel == null)
				return;

			var content = UIBuilder
				.VBox()
				.ExpandFill()
				.Spacing(StylePresets.Spacing.Large)
				.Children(BuildTitle(), BuildPromotionButtons())
				.Build();

			// Add padding around content
			var marginBuilder = UIBuilder.Margins();
			marginBuilder.ExpandFill();
			marginBuilder.Margins(StylePresets.Spacing.Large);
			marginBuilder.Child(content);
			marginBuilder.AddTo(dialogPanel);
		}

		private Control BuildTitle()
		{
			return UIBuilder.Label($"Promote {promotingColor} Pawn").Title().Centered().Build();
		}

		private Control BuildPromotionButtons()
		{
			var buttonContainer = UIBuilder.HBox().CenterAlign().Spacing(StylePresets.Spacing.Medium).Build();

			// Create promotion buttons with icons if available
			CreateStyledPromotionButton(buttonContainer, PieceType.Queen, "Queen", "♕");
			CreateStyledPromotionButton(buttonContainer, PieceType.Rook, "Rook", "♜");
			CreateStyledPromotionButton(buttonContainer, PieceType.Bishop, "Bishop", "♗");
			CreateStyledPromotionButton(buttonContainer, PieceType.Knight, "Knight", "♘");

			return UIBuilder.CenterContainer().ExpandFill().Child(buttonContainer).Build();
		}

		private void CreateStyledPromotionButton(Container parent, PieceType pieceType, string name, string symbol)
		{
			var button = UIBuilder
				.Button($"{symbol}\n{name}")
				.Size(80, 80)
				.FontSize(StylePresets.FontSizes.Medium)
				.OnPress(() => OnPieceSelected(pieceType))
				.Build();

			// Style based on piece color
			if (promotingColor == PieceColor.White)
			{
				button.AddThemeColorOverride("font_color", ColorPalette.WhitePlayer);
			}
			else
			{
				button.AddThemeColorOverride("font_color", ColorPalette.BlackPlayer);
			}

			parent.AddChild(button);
		}

		private void OnPieceSelected(PieceType pieceType)
		{
			EmitSignal(SignalName.PieceSelected, (int)pieceType);
			Visible = false;
			dialogPanel!.Visible = false;
		}
	}
}

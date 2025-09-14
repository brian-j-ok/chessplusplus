using ChessPlusPlus.UI.Builders;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;
using StylePresets = ChessPlusPlus.UI.Styles.StylePresets;
using UIBuilder = ChessPlusPlus.UI.Builders.UI;

namespace ChessPlusPlus.UI.Components
{
	public partial class LeftSidebarPanel : Panel
	{
		private VBoxContainer? contentContainer;

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(280, 0);
			SizeFlagsHorizontal = Control.SizeFlags.Fill;
			SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			SetAnchorsPreset(Control.LayoutPreset.FullRect);
			MouseFilter = MouseFilterEnum.Pass; // Ensure panel can receive mouse events

			var styleBox = new StyleBoxFlat
			{
				BgColor = ColorPalette.BackgroundPanel,
				BorderWidthRight = 2,
				BorderColor = ColorPalette.BorderPrimary,
				ContentMarginLeft = StylePresets.Spacing.Medium,
				ContentMarginRight = StylePresets.Spacing.Medium,
				ContentMarginTop = StylePresets.Spacing.Medium,
				ContentMarginBottom = StylePresets.Spacing.Medium,
			};
			AddThemeStyleboxOverride("panel", styleBox);

			contentContainer = new VBoxContainer();
			contentContainer.AddThemeConstantOverride("separation", StylePresets.Spacing.Medium);
			contentContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			contentContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			AddChild(contentContainer);
		}

		public void AddSection(Control section)
		{
			contentContainer?.AddChild(section);
		}

		public void AddSeparator()
		{
			var separator = new HSeparator();
			separator.AddThemeColorOverride("separator", ColorPalette.BorderSecondary);
			contentContainer?.AddChild(separator);
		}

		public void Clear()
		{
			if (contentContainer != null)
			{
				foreach (Node child in contentContainer.GetChildren())
				{
					child.QueueFree();
				}
			}
		}
	}
}

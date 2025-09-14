using ChessPlusPlus.Core;
using ChessPlusPlus.UI.Builders;
using ChessPlusPlus.UI.Components;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;
using StylePresets = ChessPlusPlus.UI.Styles.StylePresets;
using UIBuilder = ChessPlusPlus.UI.Builders.UI;

namespace ChessPlusPlus.UI.Screens
{
	public partial class GameUIScreen : Control
	{
		private HBoxContainer? mainContainer;
		private LeftSidebarPanel? leftSidebar;
		private GameInfoPanel? gameInfoPanel;
		private Control? boardContainer;
		private const float SIDEBAR_WIDTH = 280f;

		public override void _Ready()
		{
			BuildUI();
		}

		private void BuildUI()
		{
			// Set up full screen layout
			SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

			// Create horizontal box container for layout
			mainContainer = UIBuilder.HBox().Spacing(0).Build();
			mainContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			AddChild(mainContainer);

			// Create and add left sidebar
			leftSidebar = new LeftSidebarPanel();
			leftSidebar.CustomMinimumSize = new Vector2(SIDEBAR_WIDTH, 0);
			leftSidebar.SizeFlagsHorizontal = Control.SizeFlags.Fill;
			leftSidebar.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			mainContainer.AddChild(leftSidebar);

			// Add game info panel to sidebar
			gameInfoPanel = new GameInfoPanel();
			leftSidebar.AddSection(gameInfoPanel);

			// Add separator
			leftSidebar.AddSeparator();

			// Create board container
			boardContainer = UIBuilder.Panel().Background(ColorPalette.BackgroundMedium).Build();
			boardContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			boardContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			mainContainer.AddChild(boardContainer);
		}

		public void AddSidebarSection(Control section)
		{
			leftSidebar?.AddSection(section);
		}

		public void AddSidebarSeparator()
		{
			leftSidebar?.AddSeparator();
		}

		public Control? GetBoardContainer()
		{
			return boardContainer;
		}

		public override void _ExitTree()
		{
			// Cleanup is handled by child components
		}
	}
}

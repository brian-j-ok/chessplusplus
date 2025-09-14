using ChessPlusPlus.UI.Components;
using Godot;

namespace ChessPlusPlus.UI
{
	public partial class GameUI : CanvasLayer
	{
		private Control? uiRoot;
		private HBoxContainer? mainContainer;
		private LeftSidebarPanel? leftSidebar;
		private GameInfoPanel? gameInfoPanel;
		private const float SIDEBAR_WIDTH = 280f;

		public override void _Ready()
		{
			// Create root control for UI (following Godot best practice)
			uiRoot = new Control();
			uiRoot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			AddChild(uiRoot);

			// Create horizontal container for layout
			mainContainer = new HBoxContainer();
			mainContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			mainContainer.AddThemeConstantOverride("separation", 0);
			uiRoot.AddChild(mainContainer);

			// Create and add left sidebar
			leftSidebar = new LeftSidebarPanel();
			leftSidebar.CustomMinimumSize = new Vector2(SIDEBAR_WIDTH, 0);
			leftSidebar.SizeFlagsHorizontal = Control.SizeFlags.Fill;
			leftSidebar.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			mainContainer.AddChild(leftSidebar);

			// Create and add game info panel to sidebar
			gameInfoPanel = new GameInfoPanel();
			leftSidebar.AddSection(gameInfoPanel);

			// Add separator after game info
			leftSidebar.AddSeparator();

			// Create a transparent panel for the board area
			// This reserves space in the layout but doesn't render anything
			var boardAreaSpacer = new Panel();
			boardAreaSpacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			boardAreaSpacer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			boardAreaSpacer.MouseFilter = Control.MouseFilterEnum.Ignore;
			boardAreaSpacer.Modulate = new Color(1, 1, 1, 0); // Fully transparent
			mainContainer.AddChild(boardAreaSpacer);

			// The Board (Node2D) will handle its own positioning via BoardVisual
			// which now knows about the sidebar offset
		}

		public void AddSidebarSection(Control section)
		{
			leftSidebar?.AddSection(section);
		}

		public void AddSidebarSeparator()
		{
			leftSidebar?.AddSeparator();
		}

		public override void _ExitTree()
		{
			// Cleanup is handled by child components
		}
	}
}

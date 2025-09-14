using Godot;

namespace ChessPlusPlus.UI.Builders
{
	/// <summary>
	/// Builder for ScrollContainer controls
	/// </summary>
	public class ScrollContainerBuilder : UIBuilder<ScrollContainer, ScrollContainerBuilder>
	{
		public ScrollContainerBuilder(ScrollContainer container)
			: base(container) { }

		/// <summary>
		/// Sets horizontal scroll enabled
		/// </summary>
		public ScrollContainerBuilder HorizontalScroll(bool enabled = true)
		{
			control.HorizontalScrollMode = enabled
				? ScrollContainer.ScrollMode.Auto
				: ScrollContainer.ScrollMode.Disabled;
			return this;
		}

		/// <summary>
		/// Sets vertical scroll enabled
		/// </summary>
		public ScrollContainerBuilder VerticalScroll(bool enabled = true)
		{
			control.VerticalScrollMode = enabled
				? ScrollContainer.ScrollMode.Auto
				: ScrollContainer.ScrollMode.Disabled;
			return this;
		}

		/// <summary>
		/// Adds a child control to the scroll container
		/// </summary>
		public ScrollContainerBuilder Child(Control child)
		{
			control.AddChild(child);
			return this;
		}
	}
}

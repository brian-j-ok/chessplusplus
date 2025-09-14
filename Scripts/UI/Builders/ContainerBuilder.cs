using System;
using System.Collections.Generic;
using Godot;

namespace ChessPlusPlus.UI.Builders
{
	/// <summary>
	/// Builder for container controls with fluent interface
	/// </summary>
	public class ContainerBuilder<TContainer> : UIBuilder<TContainer, ContainerBuilder<TContainer>>
		where TContainer : Container
	{
		public ContainerBuilder(TContainer container)
			: base(container) { }

		/// <summary>
		/// Sets the position of the container
		/// </summary>
		public ContainerBuilder<TContainer> Position(float x, float y)
		{
			control.Position = new Vector2(x, y);
			return this;
		}

		/// <summary>
		/// Adds a single child to the container
		/// </summary>
		public ContainerBuilder<TContainer> Child(Control child)
		{
			control.AddChild(child);
			return this;
		}

		/// <summary>
		/// Adds multiple children to the container
		/// </summary>
		public ContainerBuilder<TContainer> Children(params Control[] children)
		{
			foreach (var child in children)
			{
				control.AddChild(child);
			}
			return this;
		}

		/// <summary>
		/// Adds multiple children from a collection
		/// </summary>
		public ContainerBuilder<TContainer> Children(IEnumerable<Control> children)
		{
			foreach (var child in children)
			{
				control.AddChild(child);
			}
			return this;
		}
	}

	/// <summary>
	/// Extension methods for all Container types
	/// </summary>
	public static class ContainerExtensions
	{
		/// <summary>
		/// Adds padding to any container by wrapping it in a MarginContainer
		/// </summary>
		public static ContainerBuilder<T> Padding<T>(this ContainerBuilder<T> builder, int pixels)
			where T : Container
		{
			var container = builder.Build();
			if (container is MarginContainer marginContainer)
			{
				marginContainer.AddThemeConstantOverride("margin_left", pixels);
				marginContainer.AddThemeConstantOverride("margin_right", pixels);
				marginContainer.AddThemeConstantOverride("margin_top", pixels);
				marginContainer.AddThemeConstantOverride("margin_bottom", pixels);
			}
			return builder;
		}
	}

	/// <summary>
	/// Specialized builder for BoxContainer types
	/// </summary>
	public static class BoxContainerExtensions
	{
		public static ContainerBuilder<T> Spacing<T>(this ContainerBuilder<T> builder, int pixels)
			where T : BoxContainer
		{
			builder.Build().AddThemeConstantOverride("separation", pixels);
			return builder;
		}

		public static ContainerBuilder<T> Alignment<T>(
			this ContainerBuilder<T> builder,
			BoxContainer.AlignmentMode alignment
		)
			where T : BoxContainer
		{
			builder.Build().Alignment = alignment;
			return builder;
		}

		public static ContainerBuilder<T> CenterAlign<T>(this ContainerBuilder<T> builder)
			where T : BoxContainer
		{
			builder.Build().Alignment = BoxContainer.AlignmentMode.Center;
			return builder;
		}
	}

	/// <summary>
	/// Builder for MarginContainer with margin helpers
	/// </summary>
	public class MarginContainerBuilder : ContainerBuilder<MarginContainer>
	{
		public MarginContainerBuilder(MarginContainer container)
			: base(container) { }

		/// <summary>
		/// Sets all margins to the same value
		/// </summary>
		public MarginContainerBuilder Margins(int pixels)
		{
			control.AddThemeConstantOverride("margin_left", pixels);
			control.AddThemeConstantOverride("margin_right", pixels);
			control.AddThemeConstantOverride("margin_top", pixels);
			control.AddThemeConstantOverride("margin_bottom", pixels);
			return this;
		}

		/// <summary>
		/// Sets horizontal and vertical margins
		/// </summary>
		public MarginContainerBuilder Margins(int horizontal, int vertical)
		{
			control.AddThemeConstantOverride("margin_left", horizontal);
			control.AddThemeConstantOverride("margin_right", horizontal);
			control.AddThemeConstantOverride("margin_top", vertical);
			control.AddThemeConstantOverride("margin_bottom", vertical);
			return this;
		}

		/// <summary>
		/// Sets individual margins
		/// </summary>
		public MarginContainerBuilder Margins(int left, int top, int right, int bottom)
		{
			control.AddThemeConstantOverride("margin_left", left);
			control.AddThemeConstantOverride("margin_top", top);
			control.AddThemeConstantOverride("margin_right", right);
			control.AddThemeConstantOverride("margin_bottom", bottom);
			return this;
		}

		/// <summary>
		/// Sets only the left margin
		/// </summary>
		public MarginContainerBuilder MarginLeft(int pixels)
		{
			control.AddThemeConstantOverride("margin_left", pixels);
			return this;
		}

		/// <summary>
		/// Sets only the right margin
		/// </summary>
		public MarginContainerBuilder MarginRight(int pixels)
		{
			control.AddThemeConstantOverride("margin_right", pixels);
			return this;
		}

		/// <summary>
		/// Sets only the top margin
		/// </summary>
		public MarginContainerBuilder MarginTop(int pixels)
		{
			control.AddThemeConstantOverride("margin_top", pixels);
			return this;
		}

		/// <summary>
		/// Sets only the bottom margin
		/// </summary>
		public MarginContainerBuilder MarginBottom(int pixels)
		{
			control.AddThemeConstantOverride("margin_bottom", pixels);
			return this;
		}
	}

	/// <summary>
	/// Builder for GridContainer with column helpers
	/// </summary>
	public class GridContainerBuilder : ContainerBuilder<GridContainer>
	{
		public GridContainerBuilder(GridContainer container)
			: base(container) { }

		/// <summary>
		/// Sets the number of columns
		/// </summary>
		public GridContainerBuilder Columns(int count)
		{
			control.Columns = count;
			return this;
		}

		/// <summary>
		/// Sets horizontal spacing between cells
		/// </summary>
		public GridContainerBuilder HSpacing(int pixels)
		{
			control.AddThemeConstantOverride("h_separation", pixels);
			return this;
		}

		/// <summary>
		/// Sets vertical spacing between cells
		/// </summary>
		public GridContainerBuilder VSpacing(int pixels)
		{
			control.AddThemeConstantOverride("v_separation", pixels);
			return this;
		}

		/// <summary>
		/// Sets both horizontal and vertical spacing
		/// </summary>
		public GridContainerBuilder Spacing(int pixels)
		{
			control.AddThemeConstantOverride("h_separation", pixels);
			control.AddThemeConstantOverride("v_separation", pixels);
			return this;
		}

		/// <summary>
		/// Sets both horizontal and vertical spacing individually
		/// </summary>
		public GridContainerBuilder Spacing(int horizontal, int vertical)
		{
			control.AddThemeConstantOverride("h_separation", horizontal);
			control.AddThemeConstantOverride("v_separation", vertical);
			return this;
		}
	}
}

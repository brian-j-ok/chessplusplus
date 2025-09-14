using System;
using Godot;

namespace ChessPlusPlus.UI.Builders
{
	/// <summary>
	/// Base class for all UI builders providing fluent interface pattern
	/// </summary>
	public abstract class UIBuilder<TControl, TBuilder>
		where TControl : Control
		where TBuilder : UIBuilder<TControl, TBuilder>
	{
		protected TControl control;

		protected UIBuilder(TControl control)
		{
			this.control = control;
		}

		/// <summary>
		/// Sets the name of the control
		/// </summary>
		public TBuilder Name(string name)
		{
			control.Name = name;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets the minimum size of the control
		/// </summary>
		public TBuilder Size(float width, float height)
		{
			control.CustomMinimumSize = new Vector2(width, height);
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets the minimum size of the control
		/// </summary>
		public TBuilder Size(Vector2 size)
		{
			control.CustomMinimumSize = size;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets visibility of the control
		/// </summary>
		public TBuilder Visible(bool visible)
		{
			control.Visible = visible;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets the modulate color
		/// </summary>
		public TBuilder Color(Color color)
		{
			control.Modulate = color;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets horizontal size flags
		/// </summary>
		public TBuilder HorizontalFlags(Control.SizeFlags flags)
		{
			control.SizeFlagsHorizontal = flags;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets vertical size flags
		/// </summary>
		public TBuilder VerticalFlags(Control.SizeFlags flags)
		{
			control.SizeFlagsVertical = flags;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets both size flags to expand and fill
		/// </summary>
		public TBuilder ExpandFill()
		{
			control.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			control.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets anchors for full rect
		/// </summary>
		public TBuilder FullRect()
		{
			control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets anchors to center
		/// </summary>
		public TBuilder Center()
		{
			control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets custom anchors
		/// </summary>
		public TBuilder Anchors(float left, float top, float right, float bottom)
		{
			control.AnchorLeft = left;
			control.AnchorTop = top;
			control.AnchorRight = right;
			control.AnchorBottom = bottom;
			return (TBuilder)this;
		}

		/// <summary>
		/// Sets tooltip text
		/// </summary>
		public TBuilder Tooltip(string text)
		{
			control.TooltipText = text;
			return (TBuilder)this;
		}

		/// <summary>
		/// Adds the control to a parent and returns the control
		/// </summary>
		public TControl AddTo(Node parent)
		{
			parent.AddChild(control);
			return control;
		}

		/// <summary>
		/// Returns the built control
		/// </summary>
		public TControl Build()
		{
			return control;
		}

		/// <summary>
		/// Implicit conversion to the control type
		/// </summary>
		public static implicit operator TControl(UIBuilder<TControl, TBuilder> builder)
		{
			return builder.control;
		}
	}

	/// <summary>
	/// Main UI factory class for creating builders
	/// </summary>
	public static class UI
	{
		/// <summary>
		/// Creates a new button builder
		/// </summary>
		public static ButtonBuilder Button(string text = "")
		{
			return new ButtonBuilder(new Button { Text = text });
		}

		/// <summary>
		/// Creates a new label builder
		/// </summary>
		public static LabelBuilder Label(string text = "")
		{
			return new LabelBuilder(new Label { Text = text });
		}

		/// <summary>
		/// Creates a vertical box container
		/// </summary>
		public static ContainerBuilder<VBoxContainer> VBox()
		{
			return new ContainerBuilder<VBoxContainer>(new VBoxContainer());
		}

		/// <summary>
		/// Creates a horizontal box container
		/// </summary>
		public static ContainerBuilder<HBoxContainer> HBox()
		{
			return new ContainerBuilder<HBoxContainer>(new HBoxContainer());
		}

		/// <summary>
		/// Creates a margin container
		/// </summary>
		public static MarginContainerBuilder Margins()
		{
			return new MarginContainerBuilder(new MarginContainer());
		}

		/// <summary>
		/// Creates a panel container
		/// </summary>
		public static PanelBuilder Panel()
		{
			return new PanelBuilder(new Panel());
		}

		/// <summary>
		/// Creates a center container
		/// </summary>
		public static ContainerBuilder<CenterContainer> CenterContainer()
		{
			return new ContainerBuilder<CenterContainer>(new CenterContainer());
		}

		/// <summary>
		/// Creates a grid container
		/// </summary>
		public static GridContainerBuilder Grid(int columns = 2)
		{
			return new GridContainerBuilder(new GridContainer { Columns = columns });
		}

		/// <summary>
		/// Creates a spacer control
		/// </summary>
		public static Control Spacer(float width = 0, float height = 0)
		{
			return new Control { CustomMinimumSize = new Vector2(width, height) };
		}

		/// <summary>
		/// Creates a horizontal separator
		/// </summary>
		public static HSeparator HSeparator()
		{
			return new HSeparator();
		}

		/// <summary>
		/// Creates a vertical separator
		/// </summary>
		public static VSeparator VSeparator()
		{
			return new VSeparator();
		}

		/// <summary>
		/// Creates a line edit
		/// </summary>
		public static LineEditBuilder LineEdit(string placeholder = "")
		{
			return new LineEditBuilder(new LineEdit { PlaceholderText = placeholder });
		}

		/// <summary>
		/// Creates a color rect
		/// </summary>
		public static ColorRectBuilder ColorRect(Color color)
		{
			return new ColorRectBuilder(new ColorRect { Color = color });
		}

		/// <summary>
		/// Creates a scroll container
		/// </summary>
		public static ScrollContainerBuilder ScrollContainer()
		{
			return new ScrollContainerBuilder(new ScrollContainer());
		}

		/// <summary>
		/// Creates a basic control
		/// </summary>
		public static ControlBuilder Control()
		{
			return new ControlBuilder(new Control());
		}
	}
}

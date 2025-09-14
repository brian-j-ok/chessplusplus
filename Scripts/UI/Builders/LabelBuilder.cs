using System;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;

namespace ChessPlusPlus.UI.Builders
{
	/// <summary>
	/// Builder for Label controls with fluent interface
	/// </summary>
	public class LabelBuilder : UIBuilder<Label, LabelBuilder>
	{
		public LabelBuilder(Label label)
			: base(label) { }

		/// <summary>
		/// Sets the label text
		/// </summary>
		public LabelBuilder Text(string text)
		{
			control.Text = text;
			return this;
		}

		/// <summary>
		/// Sets the horizontal alignment
		/// </summary>
		public LabelBuilder Align(HorizontalAlignment alignment)
		{
			control.HorizontalAlignment = alignment;
			return this;
		}

		/// <summary>
		/// Centers the text horizontally
		/// </summary>
		public LabelBuilder CenterH()
		{
			control.HorizontalAlignment = HorizontalAlignment.Center;
			return this;
		}

		/// <summary>
		/// Sets the vertical alignment
		/// </summary>
		public LabelBuilder VAlign(VerticalAlignment alignment)
		{
			control.VerticalAlignment = alignment;
			return this;
		}

		/// <summary>
		/// Centers the text vertically
		/// </summary>
		public LabelBuilder CenterV()
		{
			control.VerticalAlignment = VerticalAlignment.Center;
			return this;
		}

		/// <summary>
		/// Centers the text both horizontally and vertically
		/// </summary>
		public LabelBuilder Centered()
		{
			control.HorizontalAlignment = HorizontalAlignment.Center;
			control.VerticalAlignment = VerticalAlignment.Center;
			return this;
		}

		/// <summary>
		/// Sets the font size
		/// </summary>
		public LabelBuilder FontSize(int size)
		{
			control.AddThemeFontSizeOverride("font_size", size);
			return this;
		}

		/// <summary>
		/// Sets the font color
		/// </summary>
		public LabelBuilder FontColor(Color color)
		{
			control.AddThemeColorOverride("font_color", color);
			return this;
		}

		/// <summary>
		/// Applies title styling (large, centered)
		/// </summary>
		public LabelBuilder Title()
		{
			control.HorizontalAlignment = HorizontalAlignment.Center;
			control.AddThemeFontSizeOverride("font_size", 32);
			control.AddThemeColorOverride("font_color", ColorPalette.TextPrimary);
			return this;
		}

		/// <summary>
		/// Applies subtitle styling (medium, centered)
		/// </summary>
		public LabelBuilder Subtitle()
		{
			control.HorizontalAlignment = HorizontalAlignment.Center;
			control.AddThemeFontSizeOverride("font_size", 20);
			control.AddThemeColorOverride("font_color", ColorPalette.TextSecondary);
			return this;
		}

		/// <summary>
		/// Applies heading styling
		/// </summary>
		public LabelBuilder Heading()
		{
			control.AddThemeFontSizeOverride("font_size", 24);
			control.AddThemeColorOverride("font_color", ColorPalette.TextPrimary);
			return this;
		}

		/// <summary>
		/// Applies muted text styling
		/// </summary>
		public LabelBuilder Muted()
		{
			control.AddThemeColorOverride("font_color", ColorPalette.TextMuted);
			return this;
		}

		/// <summary>
		/// Applies accent text styling
		/// </summary>
		public LabelBuilder Accent()
		{
			control.AddThemeColorOverride("font_color", ColorPalette.TextAccent);
			return this;
		}

		/// <summary>
		/// Applies error text styling
		/// </summary>
		public LabelBuilder Error()
		{
			control.AddThemeColorOverride("font_color", ColorPalette.Error);
			return this;
		}

		/// <summary>
		/// Applies success text styling
		/// </summary>
		public LabelBuilder Success()
		{
			control.AddThemeColorOverride("font_color", ColorPalette.Success);
			return this;
		}

		/// <summary>
		/// Sets text wrapping mode
		/// </summary>
		public LabelBuilder Wrap(TextServer.AutowrapMode mode = TextServer.AutowrapMode.WordSmart)
		{
			control.AutowrapMode = mode;
			return this;
		}

		/// <summary>
		/// Makes the text bold
		/// </summary>
		public LabelBuilder Bold()
		{
			// Note: This would require loading a bold font variant
			// For now, we can increase the font size slightly as a workaround
			var currentSize = control.GetThemeFontSize("font_size");
			control.AddThemeFontSizeOverride("font_size", currentSize + 1);
			return this;
		}

		/// <summary>
		/// Sets text to clip if it overflows
		/// </summary>
		public LabelBuilder Clip()
		{
			control.ClipText = true;
			return this;
		}

		/// <summary>
		/// Sets text to show ellipsis if it overflows
		/// </summary>
		public LabelBuilder Ellipsis()
		{
			control.ClipText = true;
			control.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
			return this;
		}
	}

	/// <summary>
	/// Builder for LineEdit controls
	/// </summary>
	public class LineEditBuilder : UIBuilder<LineEdit, LineEditBuilder>
	{
		public LineEditBuilder(LineEdit lineEdit)
			: base(lineEdit) { }

		/// <summary>
		/// Sets the text
		/// </summary>
		public LineEditBuilder Text(string text)
		{
			control.Text = text;
			return this;
		}

		/// <summary>
		/// Sets the placeholder text
		/// </summary>
		public LineEditBuilder Placeholder(string text)
		{
			control.PlaceholderText = text;
			return this;
		}

		/// <summary>
		/// Sets max length
		/// </summary>
		public LineEditBuilder MaxLength(int length)
		{
			control.MaxLength = length;
			return this;
		}

		/// <summary>
		/// Sets the text alignment
		/// </summary>
		public LineEditBuilder Align(HorizontalAlignment alignment)
		{
			control.Alignment = alignment;
			return this;
		}

		/// <summary>
		/// Makes it a password field
		/// </summary>
		public LineEditBuilder Password(bool isPassword = true)
		{
			control.Secret = isPassword;
			return this;
		}

		/// <summary>
		/// Sets whether the field is editable
		/// </summary>
		public LineEditBuilder Editable(bool isEditable = true)
		{
			control.Editable = isEditable;
			return this;
		}

		/// <summary>
		/// Adds a text changed handler
		/// </summary>
		public LineEditBuilder OnTextChanged(Action<string> handler)
		{
			control.TextChanged += (text) => handler(text);
			return this;
		}

		/// <summary>
		/// Adds a text submitted handler
		/// </summary>
		public LineEditBuilder OnSubmit(Action<string> handler)
		{
			control.TextSubmitted += (text) => handler(text);
			return this;
		}
	}

	/// <summary>
	/// Builder for ColorRect controls
	/// </summary>
	public class ColorRectBuilder : UIBuilder<ColorRect, ColorRectBuilder>
	{
		public ColorRectBuilder(ColorRect colorRect)
			: base(colorRect) { }

		/// <summary>
		/// Sets the color
		/// </summary>
		public ColorRectBuilder SetColor(Color color)
		{
			control.Color = color;
			return this;
		}
	}
}

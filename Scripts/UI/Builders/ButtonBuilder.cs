using System;
using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;

namespace ChessPlusPlus.UI.Builders
{
	/// <summary>
	/// Builder for Button controls with fluent interface
	/// </summary>
	public class ButtonBuilder : UIBuilder<Button, ButtonBuilder>
	{
		public ButtonBuilder(Button button)
			: base(button) { }

		/// <summary>
		/// Sets the button text
		/// </summary>
		public ButtonBuilder Text(string text)
		{
			control.Text = text;
			return this;
		}

		/// <summary>
		/// Sets whether the button is a toggle button
		/// </summary>
		public ButtonBuilder Toggle(bool isToggle = true)
		{
			control.ToggleMode = isToggle;
			return this;
		}

		/// <summary>
		/// Sets the button as pressed (for toggle buttons)
		/// </summary>
		public ButtonBuilder Pressed(bool isPressed = true)
		{
			control.ButtonPressed = isPressed;
			return this;
		}

		/// <summary>
		/// Sets the button as disabled
		/// </summary>
		public ButtonBuilder Disabled(bool isDisabled = true)
		{
			control.Disabled = isDisabled;
			return this;
		}

		/// <summary>
		/// Adds a click handler to the button
		/// </summary>
		public ButtonBuilder OnPress(Action handler)
		{
			control.Pressed += handler;
			return this;
		}

		/// <summary>
		/// Adds a toggle handler to the button
		/// </summary>
		public ButtonBuilder OnToggle(Action<bool> handler)
		{
			control.Toggled += (pressed) => handler(pressed);
			return this;
		}

		/// <summary>
		/// Sets the button icon
		/// </summary>
		public ButtonBuilder Icon(Texture2D texture)
		{
			control.Icon = texture;
			return this;
		}

		/// <summary>
		/// Sets icon alignment
		/// </summary>
		public ButtonBuilder IconAlignment(HorizontalAlignment alignment)
		{
			control.IconAlignment = alignment;
			return this;
		}

		/// <summary>
		/// Sets the button to expand icon
		/// </summary>
		public ButtonBuilder ExpandIcon(bool expand = true)
		{
			control.ExpandIcon = expand;
			return this;
		}

		/// <summary>
		/// Applies primary button styling
		/// </summary>
		public ButtonBuilder Primary()
		{
			ApplyStyle(ColorPalette.PrimaryNormal, ColorPalette.PrimaryHover, ColorPalette.PrimaryPressed);
			control.AddThemeColorOverride("font_color", ColorPalette.TextPrimary);
			return this;
		}

		/// <summary>
		/// Applies secondary button styling
		/// </summary>
		public ButtonBuilder Secondary()
		{
			ApplyStyle(ColorPalette.ButtonNormal, ColorPalette.ButtonHover, ColorPalette.ButtonPressed);
			control.AddThemeColorOverride("font_color", ColorPalette.TextSecondary);
			return this;
		}

		/// <summary>
		/// Applies danger/destructive button styling
		/// </summary>
		public ButtonBuilder Danger()
		{
			ApplyStyle(
				ColorPalette.Darken(ColorPalette.Error),
				ColorPalette.Error,
				ColorPalette.Lighten(ColorPalette.Error)
			);
			control.AddThemeColorOverride("font_color", ColorPalette.TextPrimary);
			return this;
		}

		/// <summary>
		/// Applies success button styling
		/// </summary>
		public ButtonBuilder Success()
		{
			ApplyStyle(
				ColorPalette.Darken(ColorPalette.Success),
				ColorPalette.Success,
				ColorPalette.Lighten(ColorPalette.Success)
			);
			control.AddThemeColorOverride("font_color", ColorPalette.TextPrimary);
			return this;
		}

		/// <summary>
		/// Sets the font size
		/// </summary>
		public ButtonBuilder FontSize(int size)
		{
			control.AddThemeFontSizeOverride("font_size", size);
			return this;
		}

		/// <summary>
		/// Makes the button flat (no background)
		/// </summary>
		public ButtonBuilder Flat(bool isFlat = true)
		{
			control.Flat = isFlat;
			return this;
		}

		/// <summary>
		/// Sets custom button colors
		/// </summary>
		public ButtonBuilder Colors(Color normal, Color hover, Color pressed)
		{
			ApplyStyle(normal, hover, pressed);
			return this;
		}

		/// <summary>
		/// Applies a style to the button
		/// </summary>
		private void ApplyStyle(Color normal, Color hover, Color pressed)
		{
			var normalStyle = new StyleBoxFlat();
			normalStyle.BgColor = normal;
			normalStyle.CornerRadiusTopLeft = 4;
			normalStyle.CornerRadiusTopRight = 4;
			normalStyle.CornerRadiusBottomLeft = 4;
			normalStyle.CornerRadiusBottomRight = 4;
			control.AddThemeStyleboxOverride("normal", normalStyle);

			var hoverStyle = new StyleBoxFlat();
			hoverStyle.BgColor = hover;
			hoverStyle.CornerRadiusTopLeft = 4;
			hoverStyle.CornerRadiusTopRight = 4;
			hoverStyle.CornerRadiusBottomLeft = 4;
			hoverStyle.CornerRadiusBottomRight = 4;
			control.AddThemeStyleboxOverride("hover", hoverStyle);

			var pressedStyle = new StyleBoxFlat();
			pressedStyle.BgColor = pressed;
			pressedStyle.CornerRadiusTopLeft = 4;
			pressedStyle.CornerRadiusTopRight = 4;
			pressedStyle.CornerRadiusBottomLeft = 4;
			pressedStyle.CornerRadiusBottomRight = 4;
			control.AddThemeStyleboxOverride("pressed", pressedStyle);

			var disabledStyle = new StyleBoxFlat();
			disabledStyle.BgColor = ColorPalette.ButtonDisabled;
			disabledStyle.CornerRadiusTopLeft = 4;
			disabledStyle.CornerRadiusTopRight = 4;
			disabledStyle.CornerRadiusBottomLeft = 4;
			disabledStyle.CornerRadiusBottomRight = 4;
			control.AddThemeStyleboxOverride("disabled", disabledStyle);
		}

		/// <summary>
		/// Creates a button group where only one can be selected at a time
		/// </summary>
		public ButtonBuilder ButtonGroup(ButtonGroup group)
		{
			control.ButtonGroup = group;
			return this;
		}
	}
}

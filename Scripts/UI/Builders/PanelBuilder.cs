using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;

namespace ChessPlusPlus.UI.Builders
{
	/// <summary>
	/// Builder for Panel controls with styling helpers
	/// </summary>
	public class PanelBuilder : UIBuilder<Panel, PanelBuilder>
	{
		private StyleBoxFlat? styleBox;

		public PanelBuilder(Panel panel)
			: base(panel) { }

		/// <summary>
		/// Sets the background color
		/// </summary>
		public PanelBuilder Background(Color color)
		{
			EnsureStyleBox();
			styleBox!.BgColor = color;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Sets border width for all sides
		/// </summary>
		public PanelBuilder Border(int width, Color? color = null)
		{
			EnsureStyleBox();
			styleBox!.BorderWidthLeft = width;
			styleBox!.BorderWidthRight = width;
			styleBox!.BorderWidthTop = width;
			styleBox!.BorderWidthBottom = width;
			if (color.HasValue)
			{
				styleBox!.BorderColor = color.Value;
			}
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Sets individual border widths
		/// </summary>
		public PanelBuilder Border(int left, int top, int right, int bottom, Color? color = null)
		{
			EnsureStyleBox();
			styleBox!.BorderWidthLeft = left;
			styleBox!.BorderWidthTop = top;
			styleBox!.BorderWidthRight = right;
			styleBox!.BorderWidthBottom = bottom;
			if (color.HasValue)
			{
				styleBox!.BorderColor = color.Value;
			}
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Sets corner radius for all corners
		/// </summary>
		public PanelBuilder Rounded(int radius)
		{
			EnsureStyleBox();
			styleBox!.CornerRadiusTopLeft = radius;
			styleBox!.CornerRadiusTopRight = radius;
			styleBox!.CornerRadiusBottomLeft = radius;
			styleBox!.CornerRadiusBottomRight = radius;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Sets individual corner radii
		/// </summary>
		public PanelBuilder Corners(int topLeft, int topRight, int bottomLeft, int bottomRight)
		{
			EnsureStyleBox();
			styleBox!.CornerRadiusTopLeft = topLeft;
			styleBox!.CornerRadiusTopRight = topRight;
			styleBox!.CornerRadiusBottomLeft = bottomLeft;
			styleBox!.CornerRadiusBottomRight = bottomRight;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Sets content margins
		/// </summary>
		public PanelBuilder ContentMargins(int pixels)
		{
			EnsureStyleBox();
			styleBox!.ContentMarginLeft = pixels;
			styleBox!.ContentMarginRight = pixels;
			styleBox!.ContentMarginTop = pixels;
			styleBox!.ContentMarginBottom = pixels;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Sets individual content margins
		/// </summary>
		public PanelBuilder ContentMargins(int left, int top, int right, int bottom)
		{
			EnsureStyleBox();
			styleBox!.ContentMarginLeft = left;
			styleBox!.ContentMarginTop = top;
			styleBox!.ContentMarginRight = right;
			styleBox!.ContentMarginBottom = bottom;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Applies a card-like style
		/// </summary>
		public PanelBuilder Card()
		{
			EnsureStyleBox();
			styleBox!.BgColor = ColorPalette.BackgroundPanel;
			styleBox!.BorderWidthLeft = 1;
			styleBox!.BorderWidthRight = 1;
			styleBox!.BorderWidthTop = 1;
			styleBox!.BorderWidthBottom = 1;
			styleBox!.BorderColor = ColorPalette.BorderPrimary;
			styleBox!.CornerRadiusTopLeft = 8;
			styleBox!.CornerRadiusTopRight = 8;
			styleBox!.CornerRadiusBottomLeft = 8;
			styleBox!.CornerRadiusBottomRight = 8;
			styleBox!.ContentMarginLeft = 12;
			styleBox!.ContentMarginRight = 12;
			styleBox!.ContentMarginTop = 12;
			styleBox!.ContentMarginBottom = 12;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Applies a dark panel style
		/// </summary>
		public PanelBuilder Dark()
		{
			EnsureStyleBox();
			styleBox!.BgColor = ColorPalette.BackgroundDark;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Applies a light panel style
		/// </summary>
		public PanelBuilder Light()
		{
			EnsureStyleBox();
			styleBox!.BgColor = ColorPalette.BackgroundLight;
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Makes the panel transparent
		/// </summary>
		public PanelBuilder Transparent(float alpha = 0.0f)
		{
			EnsureStyleBox();
			var color = styleBox!.BgColor;
			styleBox!.BgColor = new Color(color.R, color.G, color.B, alpha);
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Adds a shadow effect
		/// </summary>
		public PanelBuilder Shadow(int size = 4, Color? color = null)
		{
			EnsureStyleBox();
			styleBox!.ShadowSize = size;
			if (color.HasValue)
			{
				styleBox!.ShadowColor = color.Value;
			}
			else
			{
				styleBox!.ShadowColor = new Color(0, 0, 0, 0.5f);
			}
			ApplyStyle();
			return this;
		}

		/// <summary>
		/// Adds a child control to the panel
		/// </summary>
		public PanelBuilder Child(Control child)
		{
			control.AddChild(child);
			return this;
		}

		/// <summary>
		/// Sets minimum size
		/// </summary>
		public PanelBuilder MinSize(float width, float height)
		{
			control.CustomMinimumSize = new Vector2(width, height);
			return this;
		}

		/// <summary>
		/// Applies a complete style to the panel
		/// </summary>
		public PanelBuilder Style(Color bgColor, Color borderColor, int borderWidth = 2, int cornerRadius = 0)
		{
			var style = new StyleBoxFlat
			{
				BgColor = bgColor,
				BorderWidthLeft = borderWidth,
				BorderWidthRight = borderWidth,
				BorderWidthTop = borderWidth,
				BorderWidthBottom = borderWidth,
				BorderColor = borderColor,
				CornerRadiusTopLeft = cornerRadius,
				CornerRadiusTopRight = cornerRadius,
				CornerRadiusBottomLeft = cornerRadius,
				CornerRadiusBottomRight = cornerRadius,
			};
			control.AddThemeStyleboxOverride("panel", style);
			return this;
		}

		/// <summary>
		/// Adds multiple children to the panel
		/// </summary>
		public PanelBuilder Children(params Control[] children)
		{
			foreach (var child in children)
			{
				control.AddChild(child);
			}
			return this;
		}

		private void EnsureStyleBox()
		{
			if (styleBox == null)
			{
				styleBox = new StyleBoxFlat();
				styleBox.BgColor = ColorPalette.BackgroundMedium;
			}
		}

		private void ApplyStyle()
		{
			if (styleBox != null)
			{
				control.AddThemeStyleboxOverride("panel", styleBox);
			}
		}
	}
}

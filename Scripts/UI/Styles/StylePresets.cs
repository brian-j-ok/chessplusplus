using Godot;
using ColorPalette = ChessPlusPlus.UI.Styles.ColorPalette;

namespace ChessPlusPlus.UI.Styles
{
	/// <summary>
	/// Predefined style configurations for common UI patterns
	/// </summary>
	public static class StylePresets
	{
		/// <summary>
		/// Standard button sizes
		/// </summary>
		public static class ButtonSizes
		{
			public static readonly Vector2 Small = new Vector2(80, 30);
			public static readonly Vector2 Medium = new Vector2(120, 40);
			public static readonly Vector2 Large = new Vector2(160, 50);
			public static readonly Vector2 ExtraLarge = new Vector2(200, 60);
			public static readonly Vector2 Square = new Vector2(40, 40);
			public static readonly Vector2 Wide = new Vector2(200, 40);
		}

		/// <summary>
		/// Standard spacing values
		/// </summary>
		public static class Spacing
		{
			public const int None = 0;
			public const int Tiny = 5;
			public const int Small = 10;
			public const int Medium = 15;
			public const int Large = 20;
			public const int ExtraLarge = 30;
			public const int Huge = 40;
		}

		/// <summary>
		/// Standard margin values
		/// </summary>
		public static class Margins
		{
			public const int None = 0;
			public const int Tiny = 4;
			public const int Small = 8;
			public const int Medium = 12;
			public const int Large = 16;
			public const int ExtraLarge = 20;
			public const int Huge = 32;
		}

		/// <summary>
		/// Standard font sizes
		/// </summary>
		public static class FontSizes
		{
			public const int Tiny = 10;
			public const int Small = 12;
			public const int Normal = 14;
			public const int Medium = 16;
			public const int Large = 20;
			public const int ExtraLarge = 24;
			public const int Title = 32;
			public const int Huge = 48;
		}

		/// <summary>
		/// Standard border configurations
		/// </summary>
		public static class Borders
		{
			public const int None = 0;
			public const int Thin = 1;
			public const int Medium = 2;
			public const int Thick = 3;
			public const int ExtraThick = 4;
		}

		/// <summary>
		/// Standard corner radius values
		/// </summary>
		public static class CornerRadius
		{
			public const int None = 0;
			public const int Small = 4;
			public const int Medium = 8;
			public const int Large = 12;
			public const int Round = 16;
			public const int Pill = 999;
		}

		/// <summary>
		/// Creates a standard card style
		/// </summary>
		public static StyleBoxFlat CardStyle()
		{
			var style = new StyleBoxFlat();
			style.BgColor = ColorPalette.BackgroundPanel;
			style.BorderWidthLeft = Borders.Thin;
			style.BorderWidthRight = Borders.Thin;
			style.BorderWidthTop = Borders.Thin;
			style.BorderWidthBottom = Borders.Thin;
			style.BorderColor = ColorPalette.BorderPrimary;
			style.CornerRadiusTopLeft = CornerRadius.Medium;
			style.CornerRadiusTopRight = CornerRadius.Medium;
			style.CornerRadiusBottomLeft = CornerRadius.Medium;
			style.CornerRadiusBottomRight = CornerRadius.Medium;
			style.ContentMarginLeft = Margins.Medium;
			style.ContentMarginRight = Margins.Medium;
			style.ContentMarginTop = Margins.Medium;
			style.ContentMarginBottom = Margins.Medium;
			style.ShadowSize = 2;
			style.ShadowColor = new Color(0, 0, 0, 0.2f);
			return style;
		}

		/// <summary>
		/// Creates a dialog/modal style
		/// </summary>
		public static StyleBoxFlat DialogStyle()
		{
			var style = CardStyle();
			style.BgColor = ColorPalette.BackgroundMedium;
			style.BorderWidthLeft = Borders.Medium;
			style.BorderWidthRight = Borders.Medium;
			style.BorderWidthTop = Borders.Medium;
			style.BorderWidthBottom = Borders.Medium;
			style.ShadowSize = 8;
			style.ShadowColor = new Color(0, 0, 0, 0.4f);
			return style;
		}

		/// <summary>
		/// Creates a board/game area style
		/// </summary>
		public static StyleBoxFlat BoardStyle()
		{
			var style = new StyleBoxFlat();
			style.BgColor = ColorPalette.BackgroundLight;
			style.BorderWidthLeft = Borders.Medium;
			style.BorderWidthRight = Borders.Medium;
			style.BorderWidthTop = Borders.Medium;
			style.BorderWidthBottom = Borders.Medium;
			style.BorderColor = ColorPalette.BorderAccent;
			style.CornerRadiusTopLeft = CornerRadius.Medium;
			style.CornerRadiusTopRight = CornerRadius.Medium;
			style.CornerRadiusBottomLeft = CornerRadius.Medium;
			style.CornerRadiusBottomRight = CornerRadius.Medium;
			return style;
		}

		/// <summary>
		/// Creates a flat/borderless style
		/// </summary>
		public static StyleBoxFlat FlatStyle(Color? bgColor = null)
		{
			var style = new StyleBoxFlat();
			style.BgColor = bgColor ?? ColorPalette.BackgroundMedium;
			return style;
		}

		/// <summary>
		/// Creates an input field style
		/// </summary>
		public static StyleBoxFlat InputStyle()
		{
			var style = new StyleBoxFlat();
			style.BgColor = ColorPalette.BackgroundDark;
			style.BorderWidthLeft = Borders.Thin;
			style.BorderWidthRight = Borders.Thin;
			style.BorderWidthTop = Borders.Thin;
			style.BorderWidthBottom = Borders.Thin;
			style.BorderColor = ColorPalette.BorderSecondary;
			style.CornerRadiusTopLeft = CornerRadius.Small;
			style.CornerRadiusTopRight = CornerRadius.Small;
			style.CornerRadiusBottomLeft = CornerRadius.Small;
			style.CornerRadiusBottomRight = CornerRadius.Small;
			style.ContentMarginLeft = Margins.Small;
			style.ContentMarginRight = Margins.Small;
			style.ContentMarginTop = Margins.Tiny;
			style.ContentMarginBottom = Margins.Tiny;
			return style;
		}

		/// <summary>
		/// Creates a tooltip style
		/// </summary>
		public static StyleBoxFlat TooltipStyle()
		{
			var style = new StyleBoxFlat();
			style.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
			style.BorderWidthLeft = Borders.Thin;
			style.BorderWidthRight = Borders.Thin;
			style.BorderWidthTop = Borders.Thin;
			style.BorderWidthBottom = Borders.Thin;
			style.BorderColor = ColorPalette.BorderAccent;
			style.CornerRadiusTopLeft = CornerRadius.Small;
			style.CornerRadiusTopRight = CornerRadius.Small;
			style.CornerRadiusBottomLeft = CornerRadius.Small;
			style.CornerRadiusBottomRight = CornerRadius.Small;
			style.ContentMarginLeft = Margins.Small;
			style.ContentMarginRight = Margins.Small;
			style.ContentMarginTop = Margins.Tiny;
			style.ContentMarginBottom = Margins.Tiny;
			return style;
		}
	}
}

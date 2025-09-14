using Godot;

namespace ChessPlusPlus.UI.Styles
{
	/// <summary>
	/// Centralized color palette for consistent theming across the UI
	/// </summary>
	public static class ColorPalette
	{
		// Background colors
		public static readonly Color BackgroundDark = new Color(0.15f, 0.15f, 0.2f);
		public static readonly Color BackgroundMedium = new Color(0.2f, 0.2f, 0.25f);
		public static readonly Color BackgroundLight = new Color(0.25f, 0.25f, 0.3f);
		public static readonly Color BackgroundPanel = new Color(0.2f, 0.2f, 0.25f);

		// Chess board colors
		public static readonly Color BoardLight = new Color(0.9f, 0.9f, 0.8f);
		public static readonly Color BoardDark = new Color(0.4f, 0.3f, 0.2f);
		public static readonly Color BoardHighlight = new Color(1.0f, 1.0f, 0.3f, 0.7f);
		public static readonly Color BoardValidMove = new Color(0.3f, 0.7f, 0.3f, 0.5f);
		public static readonly Color BoardAttack = new Color(0.9f, 0.3f, 0.3f, 0.5f);

		// Border colors
		public static readonly Color BorderPrimary = new Color(0.4f, 0.4f, 0.5f);
		public static readonly Color BorderSecondary = new Color(0.3f, 0.3f, 0.4f);
		public static readonly Color BorderAccent = new Color(0.5f, 0.5f, 0.6f);
		public static readonly Color BorderLight = new Color(0.4f, 0.4f, 0.5f);

		// Text colors
		public static readonly Color TextPrimary = Colors.White;
		public static readonly Color TextSecondary = new Color(0.8f, 0.8f, 0.8f);
		public static readonly Color TextMuted = new Color(0.6f, 0.6f, 0.6f);
		public static readonly Color TextAccent = new Color(1.0f, 0.9f, 0.3f);

		// Button colors
		public static readonly Color ButtonNormal = new Color(0.3f, 0.3f, 0.35f);
		public static readonly Color ButtonHover = new Color(0.4f, 0.4f, 0.45f);
		public static readonly Color ButtonPressed = new Color(0.5f, 0.5f, 0.55f);
		public static readonly Color ButtonDisabled = new Color(0.25f, 0.25f, 0.3f);

		// Primary action colors
		public static readonly Color PrimaryNormal = new Color(0.2f, 0.5f, 0.8f);
		public static readonly Color PrimaryHover = new Color(0.3f, 0.6f, 0.9f);
		public static readonly Color PrimaryPressed = new Color(0.1f, 0.4f, 0.7f);

		// Success/Error colors
		public static readonly Color Success = new Color(0.3f, 0.8f, 0.3f);
		public static readonly Color Warning = new Color(0.9f, 0.7f, 0.2f);
		public static readonly Color Error = new Color(0.9f, 0.3f, 0.3f);

		// Player colors
		public static readonly Color WhitePlayer = Colors.White;
		public static readonly Color BlackPlayer = Colors.DarkGray;

		/// <summary>
		/// Gets a semi-transparent version of a color
		/// </summary>
		public static Color WithAlpha(this Color color, float alpha)
		{
			return new Color(color.R, color.G, color.B, alpha);
		}

		/// <summary>
		/// Darkens a color by a percentage
		/// </summary>
		public static Color Darken(this Color color, float amount = 0.2f)
		{
			return color.Lerp(Colors.Black, amount);
		}

		/// <summary>
		/// Lightens a color by a percentage
		/// </summary>
		public static Color Lighten(this Color color, float amount = 0.2f)
		{
			return color.Lerp(Colors.White, amount);
		}
	}
}

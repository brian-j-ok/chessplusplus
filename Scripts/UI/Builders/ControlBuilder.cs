using Godot;

namespace ChessPlusPlus.UI.Builders
{
	/// <summary>
	/// Builder for basic Control nodes
	/// </summary>
	public class ControlBuilder : UIBuilder<Control, ControlBuilder>
	{
		public ControlBuilder(Control control)
			: base(control) { }

		/// <summary>
		/// Sets the position of the control
		/// </summary>
		public ControlBuilder Position(float x, float y)
		{
			control.Position = new Vector2(x, y);
			return this;
		}

		/// <summary>
		/// Sets the position of the control
		/// </summary>
		public ControlBuilder Position(Vector2 position)
		{
			control.Position = position;
			return this;
		}

		/// <summary>
		/// Sets the size
		/// </summary>
		public ControlBuilder Size(int width, int height)
		{
			control.CustomMinimumSize = new Vector2(width, height);
			control.Size = new Vector2(width, height);
			return this;
		}

		/// <summary>
		/// Adds a child control
		/// </summary>
		public ControlBuilder Child(Node child)
		{
			control.AddChild(child);
			return this;
		}

		/// <summary>
		/// Adds multiple children
		/// </summary>
		public ControlBuilder Children(params Node[] children)
		{
			foreach (var child in children)
			{
				control.AddChild(child);
			}
			return this;
		}
	}
}

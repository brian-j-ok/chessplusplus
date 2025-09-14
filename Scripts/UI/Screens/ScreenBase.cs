using Godot;

namespace ChessPlusPlus.UI.Screens
{
	/// <summary>
	/// Base class for all UI screens providing common functionality
	/// </summary>
	public abstract partial class ScreenBase : Control
	{
		protected bool isInitialized = false;

		public override void _Ready()
		{
			// Set to full rect by default
			SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

			// Build the UI
			var content = BuildUI();
			if (content != null)
			{
				AddChild(content);
			}

			// Allow derived classes to do additional setup
			OnReady();
			isInitialized = true;
		}

		/// <summary>
		/// Builds the UI for this screen. Override in derived classes.
		/// </summary>
		protected abstract Control BuildUI();

		/// <summary>
		/// Called after the UI is built. Override for additional setup.
		/// </summary>
		protected virtual void OnReady() { }

		/// <summary>
		/// Shows the screen with optional transition
		/// </summary>
		public virtual void Show(float duration = 0.0f)
		{
			if (duration > 0)
			{
				Modulate = new Color(1, 1, 1, 0);
				Visible = true;

				var tween = GetTree().CreateTween();
				tween.TweenProperty(this, "modulate:a", 1.0f, duration);
			}
			else
			{
				Visible = true;
			}
		}

		/// <summary>
		/// Hides the screen with optional transition
		/// </summary>
		public virtual void Hide(float duration = 0.0f)
		{
			if (duration > 0)
			{
				var tween = GetTree().CreateTween();
				tween.TweenProperty(this, "modulate:a", 0.0f, duration);
				tween.TweenCallback(Callable.From(() => Visible = false));
			}
			else
			{
				Visible = false;
			}
		}

		/// <summary>
		/// Transitions to another screen
		/// </summary>
		protected void TransitionTo(ScreenBase nextScreen, float duration = 0.3f)
		{
			Hide(duration);
			nextScreen.Show(duration);
		}

		/// <summary>
		/// Cleans up the screen
		/// </summary>
		public override void _ExitTree()
		{
			OnExit();
		}

		/// <summary>
		/// Called when the screen is being removed. Override for cleanup.
		/// </summary>
		protected virtual void OnExit() { }
	}
}

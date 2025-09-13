using Godot;

namespace ChessPlusPlus.Core
{
	public partial class WindowManager : Node
	{
		public override void _Ready()
		{
			// Add to autoload group so it persists across scenes
			GetTree().SetAutoAcceptQuit(false);
		}

		public override void _UnhandledKeyInput(InputEvent @event)
		{
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				switch (keyEvent.Keycode)
				{
					case Key.F11:
						ToggleFullscreen();
						break;
					case Key.Escape:
						if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
						{
							SetWindowed();
						}
						break;
				}
			}
		}

		private void ToggleFullscreen()
		{
			if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
			{
				SetWindowed();
			}
			else
			{
				SetFullscreen();
			}
		}

		private void SetFullscreen()
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
			GD.Print("Switched to fullscreen mode");
		}

		private void SetWindowed()
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			GD.Print("Switched to windowed mode");
		}

		public override void _Notification(int what)
		{
			if (what == NotificationWMCloseRequest)
			{
				GetTree().Quit();
			}
		}
	}
}

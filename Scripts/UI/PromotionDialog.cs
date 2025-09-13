namespace ChessPlusPlus.UI
{
	using ChessPlusPlus.Pieces;
	using Godot;

	public partial class PromotionDialog : Control
	{
		[Signal]
		public delegate void PieceSelectedEventHandler(int selectedType);

		private PieceColor promotingColor;

		public override void _Ready()
		{
			Visible = false;
			// Position the dialog in center
			Position = new Vector2(400, 300);
		}

		public void ShowPromotionDialog(PieceColor color)
		{
			promotingColor = color;
			Visible = true;

			// Clear existing buttons
			foreach (Node child in GetChildren())
			{
				child.QueueFree();
			}

			CreatePromotionButtons();
		}

		private void CreatePromotionButtons()
		{
			var vbox = new VBoxContainer();
			AddChild(vbox);

			var label = new Label();
			label.Text = $"Promote {promotingColor} Pawn";
			label.HorizontalAlignment = HorizontalAlignment.Center;
			vbox.AddChild(label);

			var hbox = new HBoxContainer();
			vbox.AddChild(hbox);

			CreatePromotionButton(hbox, PieceType.Queen, "Queen");
			CreatePromotionButton(hbox, PieceType.Rook, "Rook");
			CreatePromotionButton(hbox, PieceType.Bishop, "Bishop");
			CreatePromotionButton(hbox, PieceType.Knight, "Knight");
		}

		private void CreatePromotionButton(Container parent, PieceType pieceType, string name)
		{
			var button = new Button();
			button.Text = name;
			button.Pressed += () => OnPieceSelected(pieceType);
			parent.AddChild(button);
		}

		private void OnPieceSelected(PieceType pieceType)
		{
			EmitSignal(SignalName.PieceSelected, (int)pieceType);
			Visible = false;
		}
	}
}

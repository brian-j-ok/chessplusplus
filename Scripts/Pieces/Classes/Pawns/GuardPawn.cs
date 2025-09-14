using Godot;
using System.Collections.Generic;
using ChessPlusPlus.Core;

namespace ChessPlusPlus.Pieces
{
	public partial class GuardPawn : Pawn
	{
		public GuardPawn()
		{
			Type = PieceType.Pawn;
			ClassName = "Guard";
		}

		// Guard Pawns have normal pawn movement
		// Their special ability is immunity to horizontal/vertical captures
		// which is handled in Board.CanBeCapturedFrom()
	}
}

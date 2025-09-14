using System;
using System.Collections.Generic;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
	public partial class GuardPawn : Pawn, IDefensiveAbility
	{
		public GuardPawn()
		{
			Type = PieceType.Pawn;
			ClassName = "Guard";
		}

		// IDefensiveAbility implementation
		public string AbilityName => "Defensive Stance";
		public string Description => "Cannot be captured from horizontal or vertical directions";

		public bool CanBeCapturedFrom(Piece piece, Vector2I attackerPos, Board board)
		{
			var delta = piece.BoardPosition - attackerPos;
			var absX = Math.Abs(delta.X);
			var absY = Math.Abs(delta.Y);

			// Check if movement is purely horizontal or vertical
			if (absX == 0 || absY == 0)
			{
				return false; // Guard Pawn is immune to horizontal/vertical captures
			}
			return true; // Can be captured diagonally
		}
	}
}

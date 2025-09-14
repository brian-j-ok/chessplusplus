# Chess++ Ability System Documentation

## Overview

The Chess++ ability system provides a scalable framework for implementing custom piece behaviors and board-wide effects. This system separates ability logic from core piece movement, making it easy to add new abilities without modifying existing code.

## Architecture

### Core Components

1. **Ability Interfaces** (`Scripts/Core/Abilities/IAbility.cs`)
   - Define contracts for different types of abilities
   - Located in the `ChessPlusPlus.Core.Abilities` namespace

2. **BoardStateManager** (`Scripts/Core/Abilities/BoardStateManager.cs`)
   - Tracks piece states and persistent effects
   - Manages ability triggers and interactions
   - Integrated into the Board class

3. **BoardMovementValidator** (`Scripts/Core/Validators/BoardMovementValidator.cs`)
   - Enhanced to check abilities when validating moves
   - Integrates with BoardStateManager for state-aware validation

## Ability Interface Types

### IMovementModifier
Modifies or replaces a piece's movement pattern.

**Use Cases:**
- Pieces with extended movement ranges
- Alternative movement patterns
- Movement restrictions

**Example Implementation:** `RangerPawn`, `ChargeKnight`

```csharp
public interface IMovementModifier : IAbility
{
    List<Vector2I> ModifyMovement(Piece piece, Board board, List<Vector2I> standardMoves);
}
```

### ICaptureModifier
Modifies how a piece captures enemies, separate from movement.

**Use Cases:**
- Extended capture range
- Special capture patterns
- Conditional captures

**Example Implementation:** `BombingRook`

```csharp
public interface ICaptureModifier : IAbility
{
    List<Vector2I> GetAdditionalCaptures(Piece piece, Board board);
    bool CanCapture(Piece piece, Vector2I targetPos, Board board);
}
```

### IDefensiveAbility
Provides defensive capabilities to a piece.

**Use Cases:**
- Immunity to certain capture types
- Damage reduction
- Capture redirection

**Example Implementation:** `GuardPawn`

```csharp
public interface IDefensiveAbility : IAbility
{
    bool CanBeCapturedFrom(Piece piece, Vector2I attackerPos, Board board);
}
```

### IPassiveAbility
Triggers based on game events.

**Use Cases:**
- Area effects
- Reaction abilities
- Turn-based effects

**Example Implementation:** `FreezingBishop`

```csharp
public interface IPassiveAbility : IAbility
{
    void OnPieceMoved(Piece movedPiece, Vector2I from, Vector2I to, Board board);
    void OnTurnStart(PieceColor currentTurn, Board board);
    void OnSelfMoved(Vector2I from, Vector2I to, Board board);
}
```

### IBoardEffect
Affects other pieces on the board.

**Use Cases:**
- Area of effect abilities
- Buff/debuff zones
- Environmental effects

**Example Implementation:** `FreezingBishop`

```csharp
public interface IBoardEffect : IAbility
{
    List<Piece> GetAffectedPieces(Piece source, Board board);
    void ApplyEffect(Piece source, List<Piece> targets, Board board);
    void RemoveEffect(Piece source, List<Piece> targets, Board board);
}
```

### IPersistentEffect
Effects that last across turns.

**Use Cases:**
- Status effects (frozen, poisoned, etc.)
- Temporary buffs/debuffs
- Timed abilities

```csharp
public interface IPersistentEffect : IAbility
{
    int Duration { get; }
    bool ShouldEnd(Piece source, Board board);
}
```

## Implementing New Custom Pieces

### Step 1: Create the Piece Class

Create a new class inheriting from the base piece type and implementing appropriate ability interfaces:

```csharp
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;

namespace ChessPlusPlus.Pieces
{
    public partial class CustomBishop : Bishop, IPassiveAbility, IBoardEffect
    {
        public CustomBishop()
        {
            Type = PieceType.Bishop;
            ClassName = "Custom";
        }

        // Implement ability interfaces...
    }
}
```

### Step 2: Implement Ability Logic

Implement the required interface methods:

```csharp
// IPassiveAbility implementation
public string AbilityName => "Custom Ability";
public string Description => "Description of what this ability does";

public void OnPieceMoved(Piece movedPiece, Vector2I from, Vector2I to, Board board)
{
    // React to piece movements
}

public void OnTurnStart(PieceColor currentTurn, Board board)
{
    // Perform turn-based effects
}

public void OnSelfMoved(Vector2I from, Vector2I to, Board board)
{
    // React when this piece moves
}
```

### Step 3: Register in Army Class

Add the new piece variant to the Army class factory method:

```csharp
private Piece CreateBishop(string className)
{
    return className switch
    {
        "Custom" => new CustomBishop(),
        "Freezing" => new FreezingBishop(),
        _ => new Bishop(),
    };
}
```

### Step 4: Update PieceRegistry

Add a description for the new piece in PieceRegistry:

```csharp
private static string GenerateDescription(string className, string typeName, PieceType pieceType)
{
    return className switch
    {
        // ... existing cases ...
        "Custom" when pieceType == PieceType.Bishop =>
            "Description of the custom bishop's abilities",
        _ => $"{className} variant of {pieceType.ToString().ToLower()}",
    };
}
```

## Current Custom Pieces

### Movement Modifiers
- **RangerPawn**: Can always move 2 squares forward and capture diagonally backwards
- **ChargeKnight**: Moves exactly 3 squares in straight lines, jumping over pieces

### Defensive Abilities
- **GuardPawn**: Cannot be captured from horizontal or vertical directions

### Complex Abilities
- **FreezingBishop**: Freezes enemy pieces that land adjacent until the bishop moves
- **BombingRook**: Can capture enemies one square beyond normal range by throwing bombs
- **ResurrectingKing**: Once per game, when checkmated, can teleport to a random safe square
- **GlassQueen**: Must move twice per turn but shatters if exposed to horizontal threats

## State Management

The `BoardStateManager` tracks piece states including:

- **Frozen State**: Pieces unable to move
- **Effect Sources**: Which piece is causing an effect
- **Effect Duration**: How long effects last
- **Move Tracking**: Number of moves made this turn (for multi-move pieces)
- **Double Move Requirement**: Tracks pieces that must move twice per turn
- **Auto-Capture Pending**: Marks pieces for automatic capture
- **Custom States**: Extensible dictionary for new state types

### PieceState Class

```csharp
public class PieceState
{
    public bool IsFrozen { get; set; }
    public Piece? FrozenBy { get; set; }
    public int FrozenDuration { get; set; }
    public int MovesThisTurn { get; set; }
    public bool RequiresDoubleMove { get; set; }
    public bool PendingAutoCapture { get; set; }
    public Dictionary<string, object> CustomStates { get; set; }
}
```

## Best Practices

### 1. Separation of Concerns
- Keep ability logic separate from base piece movement
- Use interfaces to define ability contracts
- Let BoardStateManager handle cross-piece interactions

### 2. Performance Considerations
- Cache ability checks when possible
- Avoid recursive ability triggers
- Use lazy evaluation for expensive calculations

### 3. Testing New Abilities
- Test ability interactions with existing pieces
- Verify state persistence across turns
- Check edge cases (piece capture, board boundaries)

### 4. Naming Conventions
- Use descriptive class names (e.g., `FreezingBishop`, not `IceBishop`)
- Keep ability names concise but clear
- Follow existing patterns for consistency

## Future Extensions

The ability system is designed to be extensible. Potential additions include:

- **Terrain Effects**: Board squares with special properties
- **Combo Abilities**: Abilities that trigger when specific pieces are adjacent
- **Resource System**: Abilities that consume/generate resources
- **Evolution System**: Pieces that transform based on conditions
- **Team Synergies**: Abilities that affect all pieces of the same color

## Debugging

When debugging ability interactions:

1. Check `BoardStateManager.RegisterPiece()` is called for new pieces
2. Verify ability interfaces are properly implemented
3. Use GD.Print() statements in ability methods to trace execution
4. Check that `OnTurnStart()` is being called in TurnManager
5. Verify state persistence in `PieceState` objects

## Example: Creating a "Teleporting Queen"

Here's a complete example of adding a new custom piece:

```csharp
// Scripts/Pieces/Classes/Queens/TeleportingQueen.cs
using System.Collections.Generic;
using ChessPlusPlus.Core;
using ChessPlusPlus.Core.Abilities;
using Godot;

namespace ChessPlusPlus.Pieces
{
    public partial class TeleportingQueen : Queen, IMovementModifier
    {
        private bool hasTeleported = false;

        public TeleportingQueen()
        {
            Type = PieceType.Queen;
            ClassName = "Teleporting";
        }

        // IMovementModifier implementation
        public string AbilityName => "Quantum Leap";
        public string Description => "Once per game, can teleport to any empty square";

        public List<Vector2I> ModifyMovement(Piece piece, Board board, List<Vector2I> standardMoves)
        {
            var moves = new List<Vector2I>(standardMoves);

            // If hasn't teleported yet, can move to any empty square
            if (!hasTeleported)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        var pos = new Vector2I(x, y);
                        if (board.IsSquareEmpty(pos) && !moves.Contains(pos))
                        {
                            moves.Add(pos);
                        }
                    }
                }
            }

            return moves;
        }

        public override void OnMoved(Vector2I from, Vector2I to, Board board)
        {
            base.OnMoved(from, to, board);

            // Check if this was a teleport (distance > 2)
            var distance = (to - from).Length();
            if (distance > 2)
            {
                hasTeleported = true;
                GD.Print($"Teleporting Queen used its once-per-game teleport!");
            }
        }
    }
}
```

Then update Army.cs:
```csharp
private Piece CreateQueen(string className)
{
    return className switch
    {
        "Teleporting" => new TeleportingQueen(),
        _ => new Queen(),
    };
}
```

And PieceRegistry.cs:
```csharp
"Teleporting" when pieceType == PieceType.Queen =>
    "Once per game, can teleport to any empty square",
```

This documentation should be updated as new abilities and features are added to the system.
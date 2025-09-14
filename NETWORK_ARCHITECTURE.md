# Chess++ Network Architecture

## Overview
Chess++ implements an authoritative host-client networking model for LAN multiplayer games. The host acts as the single source of truth for game state, while clients send inputs and receive state updates.

## Key Components

### 1. NetworkManager (`Scripts/Network/NetworkManager.cs`)
- Handles low-level networking using Godot's ENet multiplayer
- Manages connection establishment and RPC communication
- Provides methods for:
  - Hosting/joining games
  - Sending moves with multi-move support
  - Army configuration synchronization
  - Game state broadcasting
  - Move validation requests/responses

### 2. NetworkStateManager (`Scripts/Network/NetworkStateManager.cs`)
- Manages complete game state serialization/deserialization
- Creates `GameStateSnapshot` containing:
  - All piece positions and states
  - Timer values
  - Game state (check, checkmate, etc.)
  - Multi-move tracking
  - Turn information
- Handles state reconciliation on clients
- Validates moves on the host

### 3. LANSetupScreen (`Scripts/UI/LANSetupScreen.cs`)
- Pre-game lobby for LAN games
- Features:
  - Host chooses colors for both players
  - Both players can customize their armies
  - Ready system before game start
  - Synchronizes army configurations

## Network Flow

### Game Setup
1. Host creates game via `NetworkManager.HostGame()`
2. Client joins via `NetworkManager.JoinGame()`
3. Players enter `LANSetupScreen`
4. Host assigns colors to players
5. Both players customize armies
6. Host starts game, sending army configs to client
7. Game scene loads with synchronized armies

### During Gameplay

#### Host (Authoritative)
- Validates all moves
- Maintains canonical game state
- Broadcasts state snapshots every 500ms
- Processes game logic (check, checkmate, etc.)
- Manages timers

#### Client
- Sends move requests to host
- Receives and applies state snapshots
- Interpolates visual updates
- Displays synchronized timers

### Move Synchronization
1. Player makes move locally
2. Move sent to network:
   - Regular moves: `SendMove(from, to)`
   - Multi-moves: `SendMove(from, to, isMultiMove, moveNumber, totalMoves)`
3. Host validates move
4. If valid, host updates state and broadcasts
5. All clients apply new state

### State Synchronization
- Host broadcasts complete `GameStateSnapshot` every 500ms
- Snapshot includes:
  - Piece positions and types
  - Timer values
  - Game state flags
  - Multi-move progress
- Clients reconcile local state with authoritative snapshot

## Special Features

### Multi-Move Support
- Pieces like `GlassQueen` requiring multiple moves per turn
- Tracked in `BoardStateManager`
- Synchronized via `NetworkStateManager`
- Move counter included in network messages

### Army Customization Sync
- Custom piece classes serialized as JSON
- Transmitted during game setup
- Both players can have different custom armies
- Properly loaded on both clients

### Timer Synchronization
- Host manages canonical timer values
- Timer state included in every snapshot
- Clients display synchronized times
- Prevents timer desync issues

## Error Handling

### Connection Issues
- Graceful disconnection handling
- State recovery mechanisms
- Timeout detection

### State Conflicts
- Host state always authoritative
- Client predictions can be rolled back
- Move validation prevents illegal states

## Performance Optimizations

### Periodic Sync
- 500ms interval balances responsiveness and bandwidth
- Full state snapshots ensure consistency
- Delta compression possible for future optimization

### Move Validation
- Host validates before applying
- Prevents illegal moves from corrupting state
- Reduces need for rollbacks

## Testing Considerations

### LAN Testing
1. Start Godot project on two machines
2. Host creates game from main menu
3. Client joins using host's IP
4. Both customize armies in setup screen
5. Verify synchronized gameplay:
   - Pieces appear correctly
   - Moves sync properly
   - Timers match
   - Special abilities work

### Known Limitations
- Currently LAN only (no internet play)
- Maximum 2 players (chess limitation)
- No reconnection support yet
- No spectator mode

## Future Enhancements

### Planned Features
- Internet play via relay server
- Reconnection support
- Replay system
- Spectator mode
- Delta compression for bandwidth optimization
- Lag compensation techniques

### Architecture Improvements
- Message queuing with acknowledgments
- Client-side prediction with rollback
- Interpolation for smoother visuals
- Enhanced error recovery

## Code Examples

### Sending a Move (Client)
```csharp
if (networkManager != null && networkManager.IsConnected)
{
    networkManager.SendMove(from, to, isMultiMove, moveNumber, totalMoves);
}
```

### Broadcasting State (Host)
```csharp
if (networkManager.IsHost && networkStateManager != null)
{
    networkStateManager.BroadcastState();
}
```

### Applying State (Client)
```csharp
private void OnNetworkStateReceived(string serializedState)
{
    networkStateManager.OnNetworkStateReceived(serializedState);
}
```

## Conclusion
The Chess++ network architecture provides a robust foundation for synchronized multiplayer chess with support for complex piece mechanics and army customization. The authoritative host model ensures consistency while periodic state synchronization maintains accuracy across all clients.
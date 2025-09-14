namespace ChessPlusPlus.Network
{
	using System;
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;

	public partial class NetworkManager : Node
	{
		private static NetworkManager? instance;
		public static NetworkManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new NetworkManager();
				}
				return instance;
			}
		}

		private const int DefaultPort = 7000;
		private const int MaxClients = 1; // Chess is 2 players, host + 1 client

		private MultiplayerApi multiplayer = null!;

		public bool IsHost { get; private set; }
		public bool IsConnected { get; private set; }
		public int LocalPlayerId { get; private set; }
		public int RemotePlayerId { get; private set; }

		// Network events
		[Signal]
		public delegate void PlayerConnectedEventHandler(int id);

		[Signal]
		public delegate void PlayerDisconnectedEventHandler(int id);

		[Signal]
		public delegate void ConnectionSucceededEventHandler();

		[Signal]
		public delegate void ConnectionFailedEventHandler();

		[Signal]
		public delegate void ServerDisconnectedEventHandler();

		[Signal]
		public delegate void MoveReceivedEventHandler(Vector2I from, Vector2I to);

		[Signal]
		public delegate void ArmyConfigReceivedEventHandler(string whiteArmyData, string blackArmyData);

		[Signal]
		public delegate void ColorAssignmentReceivedEventHandler(int hostColor);

		[Signal]
		public delegate void GameStateReceivedEventHandler(string serializedState);

		[Signal]
		public delegate void MoveValidationRequestedEventHandler(Vector2I from, Vector2I to, int peerId);

		[Signal]
		public delegate void MoveValidationReceivedEventHandler(bool isValid, Vector2I from, Vector2I to);

		[Signal]
		public delegate void ClientReadyReceivedEventHandler(int clientId, string armyData);

		public override void _Ready()
		{
			if (instance != null && instance != this)
			{
				QueueFree();
				return;
			}

			instance = this;
			ProcessMode = ProcessModeEnum.Always; // Keep running even when paused

			// Set up multiplayer signals
			Multiplayer.PeerConnected += OnPeerConnected;
			Multiplayer.PeerDisconnected += OnPeerDisconnected;
			Multiplayer.ConnectedToServer += OnConnectedToServer;
			Multiplayer.ConnectionFailed += OnConnectionFailed;
			Multiplayer.ServerDisconnected += OnServerDisconnected;

			GD.Print("NetworkManager initialized");
		}

		public override void _ExitTree()
		{
			if (IsConnected)
			{
				CloseConnection();
			}
		}

		/// <summary>
		/// Host a game on the local network
		/// </summary>
		public Error HostGame(int port = DefaultPort)
		{
			var peer = new ENetMultiplayerPeer();
			var error = peer.CreateServer(port, MaxClients);

			if (error != Error.Ok)
			{
				GD.PrintErr($"Failed to create server: {error}");
				return error;
			}

			Multiplayer.MultiplayerPeer = peer;
			LocalPlayerId = Multiplayer.GetUniqueId();
			IsHost = true;
			IsConnected = true;

			GD.Print($"Hosting game on port {port}, Player ID: {LocalPlayerId}");
			return Error.Ok;
		}

		/// <summary>
		/// Join a game on the local network
		/// </summary>
		public Error JoinGame(string hostAddress, int port = DefaultPort)
		{
			if (string.IsNullOrEmpty(hostAddress))
			{
				GD.PrintErr("Host address is empty");
				return Error.InvalidParameter;
			}

			var peer = new ENetMultiplayerPeer();
			var error = peer.CreateClient(hostAddress, port);

			if (error != Error.Ok)
			{
				GD.PrintErr($"Failed to create client: {error}");
				return error;
			}

			Multiplayer.MultiplayerPeer = peer;
			LocalPlayerId = Multiplayer.GetUniqueId();
			IsHost = false;

			GD.Print($"Attempting to join game at {hostAddress}:{port}");
			return Error.Ok;
		}

		/// <summary>
		/// Close the network connection
		/// </summary>
		public void CloseConnection()
		{
			if (Multiplayer.MultiplayerPeer != null)
			{
				Multiplayer.MultiplayerPeer.Close();
				Multiplayer.MultiplayerPeer = null;
			}

			IsConnected = false;
			IsHost = false;
			LocalPlayerId = 0;
			RemotePlayerId = 0;

			GD.Print("Network connection closed");
		}

		/// <summary>
		/// Send a chess move to the other player
		/// </summary>
		public void SendMove(
			Vector2I from,
			Vector2I to,
			bool isMultiMove = false,
			int moveNumber = 1,
			int totalMoves = 1
		)
		{
			if (!IsConnected)
			{
				GD.PrintErr("Not connected to send move");
				return;
			}

			GD.Print($"Sending move: {from} to {to} (Move {moveNumber}/{totalMoves})");
			Rpc(MethodName.ReceiveMove, from.X, from.Y, to.X, to.Y, isMultiMove, moveNumber, totalMoves);
		}

		/// <summary>
		/// Receive a chess move from the other player
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ReceiveMove(
			int fromX,
			int fromY,
			int toX,
			int toY,
			bool isMultiMove = false,
			int moveNumber = 1,
			int totalMoves = 1
		)
		{
			var from = new Vector2I(fromX, fromY);
			var to = new Vector2I(toX, toY);

			if (isMultiMove)
			{
				GD.Print($"Received multi-move from network: {from} to {to} (Move {moveNumber}/{totalMoves})");
			}
			else
			{
				GD.Print($"Received move from network: {from} to {to}");
			}

			EmitSignal(SignalName.MoveReceived, from, to);
		}

		/// <summary>
		/// Send complete game state snapshot (host only)
		/// </summary>
		public void SendGameState(string serializedState)
		{
			if (!IsConnected || !IsHost)
				return;

			Rpc(MethodName.ReceiveGameState, serializedState);
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
		private void ReceiveGameState(string serializedState)
		{
			GD.Print($"Received game state snapshot from host");
			EmitSignal(SignalName.GameStateReceived, serializedState);
		}

		/// <summary>
		/// Request move validation from host (client only)
		/// </summary>
		public void RequestMoveValidation(Vector2I from, Vector2I to)
		{
			if (!IsConnected || IsHost)
				return;

			RpcId(1, MethodName.ValidateMove, from.X, from.Y, to.X, to.Y);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ValidateMove(int fromX, int fromY, int toX, int toY)
		{
			if (!IsHost)
				return;

			var from = new Vector2I(fromX, fromY);
			var to = new Vector2I(toX, toY);

			// This will be handled by the game manager
			EmitSignal(SignalName.MoveValidationRequested, from, to, Multiplayer.GetRemoteSenderId());
		}

		/// <summary>
		/// Send move validation result (host only)
		/// </summary>
		public void SendMoveValidationResult(int peerId, bool isValid, Vector2I from, Vector2I to)
		{
			if (!IsHost)
				return;

			RpcId(peerId, MethodName.ReceiveMoveValidationResult, isValid, from.X, from.Y, to.X, to.Y);
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
		private void ReceiveMoveValidationResult(bool isValid, int fromX, int fromY, int toX, int toY)
		{
			var from = new Vector2I(fromX, fromY);
			var to = new Vector2I(toX, toY);
			EmitSignal(SignalName.MoveValidationReceived, isValid, from, to);
		}

		// Network event handlers
		private void OnPeerConnected(long id)
		{
			GD.Print($"Peer connected: {id}");
			RemotePlayerId = (int)id;
			EmitSignal(SignalName.PlayerConnected, (int)id);
		}

		private void OnPeerDisconnected(long id)
		{
			GD.Print($"Peer disconnected: {id}");
			RemotePlayerId = 0;
			EmitSignal(SignalName.PlayerDisconnected, (int)id);
		}

		private void OnConnectedToServer()
		{
			GD.Print("Connected to server successfully");
			IsConnected = true;
			// Server is always ID 1
			RemotePlayerId = 1;
			EmitSignal(SignalName.ConnectionSucceeded);
		}

		private void OnConnectionFailed()
		{
			GD.PrintErr("Failed to connect to server");
			IsConnected = false;
			EmitSignal(SignalName.ConnectionFailed);
		}

		private void OnServerDisconnected()
		{
			GD.Print("Server disconnected");
			IsConnected = false;
			RemotePlayerId = 0;
			EmitSignal(SignalName.ServerDisconnected);
		}

		/// <summary>
		/// Send army configuration to the other player
		/// </summary>
		public void SendArmyConfig(string whiteArmyData, string blackArmyData)
		{
			if (!IsConnected || !IsHost)
			{
				GD.PrintErr("Only host can send army configuration");
				return;
			}

			GD.Print($"Sending army configuration to client");
			GD.Print($"White army data length: {whiteArmyData.Length}");
			GD.Print($"Black army data length: {blackArmyData.Length}");
			Rpc(MethodName.ReceiveArmyConfig, whiteArmyData, blackArmyData);
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
		private void ReceiveArmyConfig(string whiteArmyData, string blackArmyData)
		{
			GD.Print("Received army configuration from host");
			GD.Print($"White army data length: {whiteArmyData.Length}");
			GD.Print($"Black army data length: {blackArmyData.Length}");
			EmitSignal(SignalName.ArmyConfigReceived, whiteArmyData, blackArmyData);
		}

		/// <summary>
		/// Send color assignment to the other player
		/// </summary>
		public void SendColorAssignment(PieceColor hostColor)
		{
			if (!IsConnected || !IsHost)
			{
				GD.PrintErr("Only host can send color assignment");
				return;
			}

			GD.Print($"Sending color assignment: Host plays as {hostColor}");
			Rpc(MethodName.ReceiveColorAssignment, (int)hostColor);
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
		private void ReceiveColorAssignment(int hostColorInt)
		{
			var hostColor = (PieceColor)hostColorInt;
			GD.Print($"Received color assignment: Host plays as {hostColor}");
			EmitSignal(SignalName.ColorAssignmentReceived, (int)hostColor);
		}

		/// <summary>
		/// Client sends their army selection and ready status to the host
		/// </summary>
		public void SendClientReady(string armyData)
		{
			if (!IsConnected || IsHost)
			{
				GD.PrintErr("Only client can send ready status");
				return;
			}

			GD.Print("Sending ready status and army to host");
			RpcId(1, MethodName.ReceiveClientReady, armyData);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ReceiveClientReady(string armyData)
		{
			if (IsHost)
			{
				GD.Print("Host received client ready status with army");
				var senderId = Multiplayer.GetRemoteSenderId();
				EmitSignal(SignalName.ClientReadyReceived, senderId, armyData);
			}
		}

		/// <summary>
		/// Get the local IP address for displaying to host
		/// </summary>
		public string GetLocalIPAddress()
		{
			foreach (var address in IP.GetLocalAddresses())
			{
				// Skip loopback and IPv6 addresses
				if (!address.Contains(":") && !address.StartsWith("127."))
				{
					return address;
				}
			}
			return "127.0.0.1";
		}
	}
}

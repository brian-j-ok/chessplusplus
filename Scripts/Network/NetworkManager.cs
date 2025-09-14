namespace ChessPlusPlus.Network
{
	using ChessPlusPlus.Core;
	using ChessPlusPlus.Pieces;
	using ChessPlusPlus.Players;
	using Godot;
	using System;

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
		public void SendMove(Vector2I from, Vector2I to)
		{
			if (!IsConnected)
			{
				GD.PrintErr("Not connected to send move");
				return;
			}

			GD.Print($"Sending move: {from} to {to}");
			Rpc(MethodName.ReceiveMove, from.X, from.Y, to.X, to.Y);
		}

		/// <summary>
		/// Receive a chess move from the other player
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ReceiveMove(int fromX, int fromY, int toX, int toY)
		{
			var from = new Vector2I(fromX, fromY);
			var to = new Vector2I(toX, toY);

			GD.Print($"Received move from network: {from} to {to}");
			EmitSignal(SignalName.MoveReceived, from, to);
		}

		/// <summary>
		/// Send game state update (check, checkmate, etc.)
		/// </summary>
		public void SendGameState(GameState state)
		{
			if (!IsConnected)
				return;

			Rpc(MethodName.ReceiveGameState, (int)state);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ReceiveGameState(int stateInt)
		{
			var state = (GameState)stateInt;
			GD.Print($"Received game state: {state}");
			// Handle game state update
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

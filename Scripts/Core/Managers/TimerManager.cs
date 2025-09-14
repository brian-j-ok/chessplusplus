namespace ChessPlusPlus.Core.Managers
{
	using ChessPlusPlus.Pieces;
	using Godot;

	/// <summary>
	/// Manages game timers and time limits for both players
	/// </summary>
	public partial class TimerManager : Node
	{
		[Export]
		public float InitialTimeSeconds { get; set; } = 600.0f; // 10 minutes default

		[Export]
		public float IncrementSeconds { get; set; } = 0.0f; // Time increment per move

		private float whiteTimeRemaining;
		private float blackTimeRemaining;
		private bool timersRunning = false;

		[Signal]
		public delegate void TimerUpdatedEventHandler(float whiteTime, float blackTime);

		[Signal]
		public delegate void TimeExpiredEventHandler(PieceColor color);

		/// <summary>
		/// Initializes timers for a new game
		/// </summary>
		public void Initialize()
		{
			whiteTimeRemaining = InitialTimeSeconds;
			blackTimeRemaining = InitialTimeSeconds;
			timersRunning = true;
			EmitSignal(SignalName.TimerUpdated, whiteTimeRemaining, blackTimeRemaining);
			GD.Print($"TimerManager initialized with {InitialTimeSeconds} seconds per player");
		}

		/// <summary>
		/// Updates the active timer based on the current turn
		/// </summary>
		public void Update(float delta, PieceColor currentTurn, GameState gameState)
		{
			if (!timersRunning || gameState != GameState.Playing)
				return;

			if (currentTurn == PieceColor.White)
			{
				whiteTimeRemaining -= delta;
				if (whiteTimeRemaining <= 0)
				{
					whiteTimeRemaining = 0;
					timersRunning = false;
					EmitSignal(SignalName.TimeExpired, (int)PieceColor.White);
				}
			}
			else
			{
				blackTimeRemaining -= delta;
				if (blackTimeRemaining <= 0)
				{
					blackTimeRemaining = 0;
					timersRunning = false;
					EmitSignal(SignalName.TimeExpired, (int)PieceColor.Black);
				}
			}

			EmitSignal(SignalName.TimerUpdated, whiteTimeRemaining, blackTimeRemaining);
		}

		/// <summary>
		/// Adds increment time after a move is completed
		/// </summary>
		public void AddIncrement(PieceColor color)
		{
			if (IncrementSeconds <= 0)
				return;

			if (color == PieceColor.White)
			{
				whiteTimeRemaining += IncrementSeconds;
			}
			else
			{
				blackTimeRemaining += IncrementSeconds;
			}

			EmitSignal(SignalName.TimerUpdated, whiteTimeRemaining, blackTimeRemaining);
		}

		/// <summary>
		/// Pauses the timers
		/// </summary>
		public void PauseTimers()
		{
			timersRunning = false;
		}

		/// <summary>
		/// Resumes the timers
		/// </summary>
		public void ResumeTimers()
		{
			timersRunning = true;
		}

		/// <summary>
		/// Stops the timers completely
		/// </summary>
		public void StopTimers()
		{
			timersRunning = false;
		}

		/// <summary>
		/// Gets the remaining time for a specific color
		/// </summary>
		public float GetRemainingTime(PieceColor color)
		{
			return color == PieceColor.White ? whiteTimeRemaining : blackTimeRemaining;
		}

		/// <summary>
		/// Checks if timers are currently running
		/// </summary>
		public bool AreTimersRunning()
		{
			return timersRunning;
		}
	}
}

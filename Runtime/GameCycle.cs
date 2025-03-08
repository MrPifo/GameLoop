namespace Sperlich.GameLoop {
	[System.Flags]
	public enum GameCycle {
		Update = 0,
		Fixed = 1 << 0,
		LateUpdate = 1 << 1,
		LateFixedUpdate = 1 << 2,
		Tick = 1 << 4,
	}
}

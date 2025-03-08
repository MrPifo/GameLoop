using UnityEngine;

namespace Sperlich.GameLoop {
	public static class GameLoopExt {

		public static void AddToCycle(this IEntityLoop entity) => GameLoop.Instance.AddToCycle(entity);
		public static void AddToCycle(this IEntityLoop entity, GameCycle loop) => GameLoop.Instance.AddToCycle(loop, entity);
		public static void RemoveFromCycle(this IEntityLoop entity, GameCycle loop) => GameLoop.Instance.RemoveFromCycle(loop, entity);
		public static void RemoveFromCycle(this IEntityLoop entity) => GameLoop.Instance.RemoveFromCycle(entity);

	}
}

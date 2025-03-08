using System;

namespace Sperlich.GameLoop {
	public class LoopAction : IEntityLoop {

		private Action action;
		private Action onRemoveAction;
		internal readonly GameCycle cycle;

		public LoopAction(GameCycle cycle) {
			this.cycle = cycle;
		}
		public LoopAction(GameCycle cycle, Action action, bool autoAddToCycle = true) {
			this.cycle = cycle;
			this.action = action;

			if (autoAddToCycle) {
				this.AddToCycle(cycle);
			}
		}

		internal void Enable() {
			this.AddToCycle(cycle);
		}
		internal void Disable() {
			this.RemoveFromCycle(cycle);
		}
		public void OnFixed() {
			action.Invoke();
		}
		public void OnUpdate() {
			action.Invoke();
		}
		public void OnLateUpdate() {
			action.Invoke();
		}
		public void OnLateFixedUpdate() {
			action.Invoke();
		}

		public void RemoveFromCycle() {
			onRemoveAction?.Invoke();

			GameLoop.Instance.RemoveFromCycle(this);
		}
		public void RemoveFromCycle(GameCycle cycle) {
			onRemoveAction?.Invoke();

			GameLoop.Instance.RemoveFromCycle(cycle, this);
		}
		public void AddToCycle() {

		}
		public void SetAction(Action action) {
			this.action = action;
		}
		public void OnRemove(Action onRemoveCallback) {
			onRemoveAction = onRemoveCallback;
		}
	}
}

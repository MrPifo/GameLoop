using System;

namespace Sperlich.GameLoop {
	public class LoopAction : IEntityLoop {

		private Action<float> action;
		private Action onRemoveAction;
		internal readonly GameCycle cycle;

		public LoopAction(GameCycle cycle) {
			this.cycle = cycle;
		}
		public LoopAction(GameCycle cycle, Action<float> action, bool autoAddToCycle = true) {
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
		public void OnFixed(float deltaTime) {
			action.Invoke(deltaTime);
		}
		public void OnUpdate(float deltaTime) {
			action.Invoke(deltaTime);
		}
		public void OnLateUpdate(float deltaTime) {
			action.Invoke(deltaTime);
		}
		public void OnLateFixedUpdate(float deltaTime) {
			action.Invoke(deltaTime);
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
			this.AddToCycle(cycle);
		}
		public void SetAction(Action<float> action) {
			this.action = action;
		}
		public void OnRemove(Action onRemoveCallback) {
			onRemoveAction = onRemoveCallback;
		}
	}
}

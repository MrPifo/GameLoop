using Cysharp.Threading.Tasks;
using Sperlich.PauseManager;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.GameLoop {
	public class GameLoop : IPausable {
		public bool IsPaused { get; set; } = false;
		public UnityEvent OnPauseEvent { get; set; } = new();
		public UnityEvent OnResumeEvent { get; set; } = new();
		public CancellationTokenSource CancelToken { get; private set; } = new();
		public Dictionary<GameCycle, List<IEntityLoop>> ActiveLoops { get; private set; } = new();

		private static GameLoop _instance;
		public static GameLoop Instance {
			get {
				if (_instance == null) {
					_instance = new GameLoop();
				}
				return _instance;
			}
			set => _instance = value;
		}

		public GameLoop() {
			foreach (GameCycle l in Enum.GetValues(typeof(GameCycle))) {
				ActiveLoops.Add(l, new());
			}

			// Initialize update cycles with a unified approach
			InitializeCycle(GameCycle.Update, PlayerLoopTiming.Update,
				(entity, delta) => entity.OnUpdate(delta), () => Time.deltaTime);

			InitializeCycle(GameCycle.LateUpdate, PlayerLoopTiming.PostLateUpdate,
				(entity, delta) => entity.OnLateUpdate(delta), () => Time.deltaTime);

			InitializeCycle(GameCycle.Fixed, PlayerLoopTiming.FixedUpdate,
				(entity, delta) => entity.OnFixed(delta), () => Time.fixedDeltaTime);

			InitializeCycle(GameCycle.LateFixedUpdate, PlayerLoopTiming.LastFixedUpdate,
				(entity, delta) => entity.OnLateFixedUpdate(delta), () => Time.fixedDeltaTime);

			// Initialize tick cycle with custom timing
			InitializeTickCycle(0.05f, () => 0.05f);
		}

		private void InitializeCycle(GameCycle cycle, PlayerLoopTiming timing, Action<IEntityLoop, float> updateAction, Func<float> deltaTimeProvider) {
			UniTask.Create(async () => {
				var list = ActiveLoops[cycle];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();

					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (!IsPaused) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							if (current == null) {
								foundNull = true;
								continue;
							}

							try {
								updateAction(current, deltaTimeProvider());
							} catch (Exception ex) {
								foundNull = true;
								Debug.LogError($"Error in {cycle}: {ex.Message} \n {ex.StackTrace}");
							}
						}
					}

					await UniTask.Yield(timing);
				}
			}).AttachExternalCancellation(CancelToken.Token);
		}
		private void InitializeTickCycle(float tickSpeed, Func<float> deltaTimeProvider) {
			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.Tick];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();

					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (!IsPaused) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							if (current == null) {
								foundNull = true;
								continue;
							}

							try {
								current.OnTick(deltaTimeProvider());
							} catch (Exception ex) {
								foundNull = true;
								Debug.LogError($"Error in Tick: {ex.Message} \n {ex.StackTrace}");
							}
						}
					}

					await UniTask.WaitForSeconds(tickSpeed, false, PlayerLoopTiming.FixedUpdate);
				}
			}).AttachExternalCancellation(CancelToken.Token);
		}

		public void OnPause() => IsPaused = true;
		public void OnResume() => IsPaused = false;

		public LoopAction AddListener(Action<float> action, GameCycle cycle, bool autoAddToCycle = true) {
			var loopAction = new LoopAction(cycle, action, autoAddToCycle);
			AddToCycle(loopAction);
			return loopAction;
		}

		public void RemoveListener(LoopAction action) => action.RemoveFromCycle();

		public void AddToCycle(IEntityLoop entity) {
			foreach (GameCycle l in Enum.GetValues(typeof(GameCycle))) {
				AddToCycle(l, entity);
			}
		}
		public void AddToCycle(GameCycle loop, IEntityLoop entity) {
			if (!ActiveLoops[loop].Contains(entity)) {
				ActiveLoops[loop].Add(entity);
			}
		}

		public void RemoveFromCycle(IEntityLoop entity) {
			foreach (GameCycle l in Enum.GetValues(typeof(GameCycle))) {
				RemoveFromCycle(l, entity);
			}
		}
		public void RemoveFromCycle(GameCycle loop, IEntityLoop entity) {
			if (ActiveLoops[loop].Contains(entity)) {
				ActiveLoops[loop].Remove(entity);
			}
		}

		public void Reset() {
			CancelToken.Cancel();
			OnPauseEvent = new();
			OnResumeEvent = new();
			CancelToken = new CancellationTokenSource();

			foreach (var l in ActiveLoops) {
				l.Value.Clear();
			}

			// Reinitialize loops after reset
			// This is missing in the original implementation
			// Consider adding loop reinitialization here
		}
	}
}
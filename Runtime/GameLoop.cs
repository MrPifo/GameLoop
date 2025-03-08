using Cysharp.Threading.Tasks;
using Sperlich.PauseManager;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.GameLoop {
	public class GameLoop : IPausable {

		public bool IsPaused { get; set; } = new();
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
		}

		public GameLoop() {
			foreach (GameCycle l in System.Enum.GetValues(typeof(GameCycle))) {
				ActiveLoops.Add(l, new());
			}

			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.Update];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null) {
								foundNull = true;
								continue;
							}

							try {
								current.OnUpdate(Time.deltaTime);
							} catch (Exception ex) {
								foundNull = true;
								UnityEngine.Debug.LogError($"Error in OnUpdate: {ex.Message}");
							}
						}
					}

					await UniTask.Yield(PlayerLoopTiming.Update);
				}
			}).AttachExternalCancellation(CancelToken.Token);
			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.LateUpdate];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null || current.Equals(null)) {
								foundNull = true;
								continue;
							}

							try {
								current.OnLateUpdate(Time.deltaTime);
							} catch (Exception ex) {
								foundNull = true;
								UnityEngine.Debug.LogError($"Error in OnUpdate: {ex.Message}");
							}
						}
					}

					await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
				}
			}).AttachExternalCancellation(CancelToken.Token);
			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.LateFixedUpdate];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null || current.Equals(null)) {
								foundNull = true;
								continue;
							}

							try {
								current.OnLateFixedUpdate(Time.fixedDeltaTime);
							} catch (Exception ex) {
								foundNull = true;
								UnityEngine.Debug.LogError($"Error in OnUpdate: {ex.Message}");
							}
						}
					}

					await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate);
				}
			}).AttachExternalCancellation(CancelToken.Token);
			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.Fixed];
				bool foundNull = false;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null || current.Equals(null)) {
								foundNull = true;
								continue;
							}

							try {
								current.OnFixed(Time.fixedDeltaTime);
							} catch (Exception ex) {
								foundNull = true;
								UnityEngine.Debug.LogError($"Error in OnUpdate: {ex.Message} \n {ex.StackTrace}");
							}
						}
					}

					await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
				}
			}).AttachExternalCancellation(CancelToken.Token);
			UniTask.Create(async () => {
				var list = ActiveLoops[GameCycle.Tick];
				bool foundNull = false;
				const float tickSpeed = 0.05f;

				while (Application.isPlaying) {
					CancelToken.Token.ThrowIfCancellationRequested();
					if (foundNull) {
						foundNull = false;
						list.RemoveAll(e => e == null);
					}

					if (IsPaused == false) {
						for (int i = 0; i < list.Count; i++) {
							var current = list[i];

							// Check if the object is null or destroyed before calling OnUpdate
							if (current == null || current.Equals(null)) {
								foundNull = true;
								continue;
							}

							try {
								current.OnTick(tickSpeed);
							} catch (Exception ex) {
								foundNull = true;
								UnityEngine.Debug.LogError($"Error in Tick: {ex.Message} \n {ex.StackTrace}");
							}
						}
					}

					await UniTask.WaitForSeconds(tickSpeed, false, PlayerLoopTiming.FixedUpdate);
				}
			}).AttachExternalCancellation(CancelToken.Token);
		}

		public void OnPause() {
			IsPaused = true;
		}
		public void OnResume() {
			IsPaused = false;
		}
		public LoopAction AddListener(Action action, GameCycle cycle, bool autoAddToCycle = true) {
			var loopAction = new LoopAction(cycle, action, autoAddToCycle);
			AddToCycle(loopAction);

			return loopAction;
		}
		public void RemoveListener(LoopAction action) {
			action.RemoveFromCycle();
		}
		public void AddToCycle(IEntityLoop entity) {
			foreach (GameCycle l in System.Enum.GetValues(typeof(GameCycle))) {
				AddToCycle(l, entity);
			}
		}
		public void AddToCycle(GameCycle loop, IEntityLoop entity) {
			if (ActiveLoops[loop].Contains(entity) == false) {
				ActiveLoops[loop].Add(entity);
			}
		}
		public void RemoveFromCycle(IEntityLoop entity) {
			foreach (GameCycle l in System.Enum.GetValues(typeof(GameCycle))) {
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
		}
	}
}
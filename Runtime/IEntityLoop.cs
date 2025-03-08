namespace Sperlich.GameLoop {
	public interface IEntityLoop {

		/// <summary>
		/// Gets executed after the ordinary Update.
		/// </summary>
		public void OnLateUpdate(float delta) { }
		/// <summary>
		/// Gets executed after the ordinary FixedUpdate.
		/// </summary>
		public void OnLateFixedUpdate(float delta) { }
		/// <summary>
		/// Gets executed every frame
		/// </summary>
		public void OnUpdate(float delta) { }
		/// <summary>
		/// Gets executed every Fixed Timestep.
		/// </summary>
		public void OnFixed(float delta) { }
		/// <summary>
		/// Gets executed at a fixed custom configured Timestep.
		/// </summary>
		/// <param name="delta"></param>
		public void OnTick(float delta) { }

#if UNITY_EDITOR
		public void OnEditorUpdate() { }
#endif
	}
}

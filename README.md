# Sperlich.GameLoop

A lightweight, flexible game loop management system for Unity applications.

## Overview

Sperlich.GameLoop manages different update cycles in Unity with support for:
- Standard Update
- Fixed Update
- Late Update
- Late Fixed Update
- Custom Tick Updates

## Requirements

- Unity (2020.3+)
- [UniTask](https://github.com/Cysharp/UniTask)

## Installation

### Via Package Manager

Add to your `manifest.json`:

```json
{
  "dependencies": {
    "com.sperlich.gameloop": https://github.com/MrPifo/GameLoop.git
  }
}
```

### Manual

1. Clone this repository
2. Copy the `Sperlich.GameLoop` folder into your Unity project

## Usage

### Implementing IEntityLoop

```csharp
public class Enemy : MonoBehaviour, IEntityLoop {
    private void Awake(){
        this.AddToCycle();
    }
    private void OnDestroy(){
        this.RemoveFromCycle();
    }

    public void OnUpdate(float delta){
        // Runs every frame
    }
    public void OnFixed(float delta){
        // Runs at fixed intervals
    }
    public void OnTick(float delta){
        // Runs at custom intervals
    }
}
```

### Using LoopAction

```csharp
// Create an action for Update cycle
var loopAction = new LoopAction(GameCycle.Update, (float deltaTime) => {
	// Your Code
}, false);
loopAction.AddToCycle();

// Cleanup
loopAction.RemoveFromCycle();
```

### Pausing

```csharp
// Pause all loops
GameLoop.Instance.Pause();

// Resume all loops
GameLoop.Instance.Resume();
```

## API Reference

### GameLoop

```csharp
// Core instance
GameLoop.Instance

// Entity management
// MonoBehaviour with IEntityLoop interface
GameLoop.Instance.AddToCycle(entity);  // Subsribe to every Loop GameCycle
GameLoop.Instance.AddToCycle(GameCycle.Update, entity);   // Only subsribes to Update GameCycle
GameLoop.Instance.RemoveFromCycle(entity);  // Unsubsribe from all GameCycle's
GameLoop.Instance.RemoveFromCycle(GameCycle.Update, entity);  // Unsubscribe from specific GameCycle

// Action-based listeners
LoopAction action = GameLoop.Instance.AddListener(callback, GameCycle.Update);

// Pause functionality
GameLoop.Instance.Pause();
GameLoop.Instance.Resume();

// Reset everything
GameLoop.Instance.Reset();
```

### GameCycle

```csharp
public enum GameCycle {
    Update = 0,
    Fixed = 1 << 0,
    LateUpdate = 1 << 1,
    LateFixedUpdate = 1 << 2,
    Tick = 1 << 4
}
```

### IEntityLoop

```csharp
public interface IEntityLoop {
    void OnUpdate(float delta) { }
    void OnFixed(float delta) { }
    void OnLateUpdate(float delta) { }
    void OnLateFixedUpdate(float delta) { }
    void OnTick(float delta) { }
}
```

### LoopAction

```csharp
// Create and register
var action = new LoopAction(GameCycle.Update, callback);

// Enable/disable
action.Enable();
action.Disable();

// Modify
action.SetAction(newCallback);
action.OnRemove(onRemoveCallback);

// Remove
action.RemoveFromCycle();
```

## License

[MIT License](LICENSE)

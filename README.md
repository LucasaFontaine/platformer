# Platformer

A 2D platformer prototype built in Unity, focused on fast, momentum-based movement - wall jumps, dashing, and a grappling hook you can swing on or use to pull objects toward you.

## Requirements

- Unity 6000.3.11f1 (Unity 6.3)
- Universal Render Pipeline (URP), 2D renderer
- New Input System package (an `InputSystem_Actions` asset is included, though movement currently reads from the legacy `Input` class)

## Getting started

1. Open the project folder in Unity Hub, using editor version 6000.3.11f1.
2. Let Unity import the project and resolve packages from `Packages/manifest.json`.
3. Open `Assets/Scenes/TestScene.unity` and press Play.

## Movement and abilities

Player movement lives in `PlayerController2D`, driven by a `Rigidbody2D`:

- **Run and jump** - horizontal movement plus a jump with coyote time and jump buffering for forgiving input timing.
- **Double jump** - configurable max jump count (defaults to 2), refreshed on landing or on touching a wall.
- **Wall jump** - detects walls via raycasts on both sides, with a short coyote window after leaving a wall so a jump still counts as a wall jump.
- **Dash** - a quick burst of horizontal speed with a cooldown and limited charges, refreshed by landing or touching a wall.
- **Swing momentum** - horizontal velocity carried out of a grapple swing decays smoothly back to normal movement instead of cutting off abruptly.

## Grappling hook

Handled by `Hook.cs`, fired with the right mouse button by default:

- Raycasts from the player toward the mouse cursor.
- If it hits an object tagged `Grapple`, the player swings from that point like a pendulum, with simple gravity-driven angular physics and drag.
- If it hits an object tagged `GrappleObject` (see `GrappleObject.cs`), that object gets pulled toward the player instead - useful for movable props or hazards.
- Releasing the grapple (mouse up, jump, or space) grants a bonus jump and hands control back to `PlayerController2D`.

## Gun

`Gun.cs` implements a shotgun-style weapon that aims at the mouse cursor and fires a spread of pellets (`Projectile.cs`) on a cooldown, with optional random jitter added to the spread. Pellets ignore the shooter's own colliders and self-destruct after a short lifetime or on hit.

## Camera

`CameraFollow.cs` follows the player horizontally and smooths toward a target height. `CameraHeightTrigger.cs` lets level designers place trigger zones that switch the camera between two preset heights as the player passes through, useful for framing vertical sections of a level.

## Project layout

```text
Assets/
  Scenes/          Unity scenes (TestScene is the main playground)
  Scripts/         Gameplay code (movement, grapple, gun, camera, cursor)
  Sprites/         Character and UI sprites
  Reusable Assets/ Prefabs (player variants, hook point, pellet, crosshair canvas)
  Settings/        URP and render pipeline settings
Packages/          Unity package manifest and lockfile
ProjectSettings/   Unity project configuration
```

## Status

This is an active work-in-progress prototype - expect placeholder art, debug logging, and mechanics that are still being tuned.

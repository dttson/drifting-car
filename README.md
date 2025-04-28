# Drifting Car Documentation

## Setup Components

### 1. Drift Car Controller

- 2 approaches of control: Physics-based or Logic-based
    - Choose Physics-based for fast implementation + simulate most closed to the real car drifting behavior
- Implement movement & rotatiton using keyboard (WASD)
- Damp down the velocity to create “drift” effect → SHOULD CREATE ANOTHER STEP???
- //TODO: Add explanation for configuration

### 2. Isometric Camera

- Follow the player with lerp function
- Has offset to make it smoother

### 3. Road

- First → using Spline to generate road →  need mesh of road fragment → takes too much time
- Choose the sample map from the asset

## First Testing

- The control of the car is function well, but it still has some issues:
    - Problem 1 (P1): The car is not stick on the ground (sometimes floating in the air)
    - Problem 2 (P2): After hitting the fence (on the side of the road), it turning around and non-stop
- Problem 3 (P3): The camera function well, but it too closed → changing the distance will help but sometimes we cannot see the car

## Problem Solving (#1)

### Problem 1: The car is not stick on the ground (sometimes floating in the air)

- Try to increase mass of the RigidBody and enable `useGravity` → not worked
- Add a logic to always force the car snap to the ground
    - Raycast from the car to the ground → if hit the road (with `road` layer) → then process the snapping logic
    - Add some configuration
        - `groundOffset` the offset from the road to the car (in order avoid collider flickering)
        - `groundSnapSpeed` how fast the car will snap to the ground

```csharp
Vector3 targetPos = hit.point + Vector3.up * groundOffset;
Vector3 newPos = Vector3.Lerp(rb.position, targetPos, groundSnapSpeed * Time.fixedDeltaTime);
rb.MovePosition(newPos);
```

### Problem 2: Infinite turning-around car

- Try to reduce `angularDrag` in RigidBody → not really worked
- Add logic to force set the `angularVelocity` to 0

```csharp
 void PreventCollisionRotation()
  {
      // Zero out angular velocity to stop physics-driven rotation from collisions
      rb.angularVelocity = Vector3.zero;
  }
```

## Second Testing

- **P4**: The car sometimes cannot move, especially on the slope

## Problem Solving (#2)

### Problem 4: Car cannot move on slopes

- After some testing and investigation, I found that it because the car rotation always forward world space Z-axis, but the road sometimes has slope. Additionally, when we force the car stick to the ground, it will pull the RigidBody lower, which makes the front of the car “plugged” into the road in some cases.
- To fix that, I connect to the real world car behavior and see that the car need to be align with the slope to keep moving. So we will need the logic to rotate the car in X-axis to make it align with the slope.
- To do that, I firstly get the `normal` vector of the plane (using `hit.normal`). Then calculate the projection vector of the car’s forward direction along the plane using `Vector3.ProjectOnPlane(Vector3 vector, Vector3 planeNormal)` . Finally rotate the car to look forward the projection vector.

```csharp
Vector3 forwardDir = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
Quaternion targetRot = Quaternion.LookRotation(forwardDir, hit.normal);
rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, groundSnapSpeed * Time.fixedDeltaTime));
```

## Third Testing

- The car can move well on slopes

## Adding AI

Add the AICarBehavior to compete with user

- Generate road for AI car, using Unity’s Spline package:
    - `SplineContainer` to draw the path of the AI car
    - `SplineAnimate` control the AI car through time.
- **Problem:**
    - The map is too big → takes much time to draw path manually
    - Solution:
        - Record the movement of the real user as a list of Vector3 points → store it to `json` file → load it into the `SplineContainer`
        - The AI car with move along the sample points that user has generated
- Additional
    - Set `SetTangentMode` of the Spline to `AutoSmooth`

## Implement game logic

### GameManager

- Adding `GameManager` class to handle to main logic of the game:
    - `GameState` represent the current state: Ready / Racing / Finished
    - Logic to handle game end when 1 car finished
    - Store result of the game
    - Request `GameUIManager` to show count down or result at the end

### GameUIManager

- Adding `GameUIManager` class to handle UI + animation
    - Function to show update countdown
    - Function to show end result

### Utils

- `SoundManager` utility class to play SFX + BGM


# Preview Links in Luna

https://playground.lunalabs.io/preview/227652/313127/ca3ddaa0673f1053128443df913abd4999b539f5e9d9bbd91ebe5accfe665378


To be continue…
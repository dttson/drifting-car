# Drifting Car Documentation

## Setup Components

### 1. Drift Car Controller

- 2 approaches of control: Physics-based or Logic-based
    - Choose Physics-based for fast implementation + simulate most closed to the real car drifting behavior
- Implement movement & rotatiton using keyboard (WASD)
- Damp down the velocity to create “drift” effect
    - Calculate magnitte of forward velocity and sideway velocity from `RidigBody` velocity
    - Reapply the forward velocity (no changed) and sideway velocity (with a `driftFactor` to create drift effect)

```csharp
Vector3 vel = rb.velocity;
float forwardVel = Vector3.Dot(vel, transform.forward);
float sidewaysVel = Vector3.Dot(vel, transform.right);
rb.velocity = transform.forward * forwardVel + transform.right * sidewaysVel * driftFactor;
```

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
- Result Panel
    - Show records of users + AI cars
    - Each record will contains:
        - Ranking
        - Image of the car → script `CarScreenshotCapture` and save the image
        - Duration
    - Animate the record
        - Move fast from the right
        - Slowly moving back and forth around center

### Utils

- `SoundManager` utility class to play SFX + BGM
    - CarEngine
    - CarIdle
    - CarDrift
    - CarHit
- Utilize [Joystick Pack Asset](https://assetstore.unity.com/packages/tools/input-management/joystick-pack-107631) to implement joystick UI and function
    - Apply the joystick to the movement

### VFX

- Adding smoke particle for car drifting effect
    - Using `Cone` shape to simulate smoke effect
    - Adjust `Rate over Time` to 5

## Fourth Testing

- P5: The control of joystick is difficult, because it has fixed to world direction, while the car is rotate continously
- P6: The car sometimes stick on the fence and cannot move forward
- P7: The smoke particle sometimes not appear while car drift
- P8: The sfx sometimes not play correctly, especially the drift audio

## Problem Solving (#3)

### Problem 5: Joystick control is difficult

- Instead of using fixed world direction (joystick up/down to move forward/backward, left/right to turn left/right), I tried to approach a car direction based control, so the car direction will follow the direction of the joystick (from the center to the handle). How to do it?
- To achieve this, I applied the dot product of joystick direction (which was converted to 3D), and applied the dot product with the car forward and right direction to get the `rawMove` and `rawTurn` direction

```csharp
Vector2 dir = joystick.Direction;
float mag = dir.magnitude;
if (mag > joystickDeadzone)
{
    Vector3 worldDir = new Vector3(dir.x, 0f, dir.y).normalized;
    rawMove = Vector3.Dot(worldDir, transform.forward) * mag;
    rawTurn = Vector3.Dot(worldDir, transform.right) * mag;
}

//... Previous movement logic here
```

### Problem 6: Car stick to the fences

- After many trial and errors, I found that I can fix this by apply both 2 solution
    - Using the `CapsuleCollider` for the car collider instead of `BoxCollider`
    - Apply `PhysicMaterial` to the car and the fence, to make it slippery on each other

### Problem 7: The smoke not appear sometimes

- The particle seems appear correctly when the car stay, but when the car moving it becomes continous in a while and then disappear.
- After trying many ways, from enable / disable the particle game object, to extend the duration, increase the max particles, I found that right parameters to adjust is `Rate over Distance`.
- By increase the `Rate over Distance` to 1, the particle still remain whenever the car moving or staying.

### Problem 8: SFX not playing correctly

- This is because of the cycle re-use of the AudioSource in `SoundManager`
- I deciced to create separate audio sources attached to the `DriftCarController` game object, and turn on/off any specific source.

## Fifth Testing

- The joystick control is much easier than previous version
- The car was running smoothly, no stick the fence
- The audio and the particles worked aswell.

## Integrate Luna

### Problem #1:

- The very first problem I ran into is the plugin not working on the Editor at all. I used Windows 11 with Unity 2022.3.20f1). I also tried to install another version or running the [LunaSampleGame](https://github.com/LunaCommunity/LunaSampleGame) but nothing works. Here is the error:

![](https://raw.githubusercontent.com/dttson/drifting-car/refs/heads/main/Documents/LunaWindowsError.png)

**Solution:**

- There is no solution for Windows Editor
- Switching machine to MacOS will helps (no error at all).

### Problem #2:

The Unity Spline library depend on `Unity.Mathematics` package, which is not supported by Luna ([Common Issue Page](https://docs.lunalabs.io/docs/playable/common-issues/code/unity-mathematics-system-math))

**Solution:**

- Switch to an open source path creator https://github.com/SebLague/Path-Creator to re-implement the movement of AI car.
- The basic logic of the AI car (moving follow path, update rotation, overtaking) will be the same, just need to use another API to get the road’s length.

### Problem #3:

There are too many meshes and texture in the game, which affect performance and build size.

**Solution:**

- Remove all unecessary environment assets / objects from the scene
- Remove all the hidden parts inside the car model.

### Problem #4:

When running on browser, the car often stick to the ground or the fence.

**Solution:**

- Increase the `groundOffset`  to make the car more separate from the road, also expose it to browser using `LunaPlaygroundField`

# Preview Links in Luna

[https://playground.lunalabs.io/preview/227652/313127/ca3ddaa0673f1053128443df913abd4999b539f5e9d9bbd91ebe5accfe665378](https://playground.lunalabs.io/preview/227652/313127/ca3ddaa0673f1053128443df913abd4999b539f5e9d9bbd91ebe5accfe665378)
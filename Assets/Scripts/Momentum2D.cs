using UnityEngine;

// Ramps a velocity toward a target velocity over time, using a faster
// rate while approaching the target and a slower rate while easing off
// it. Shared by anything that wants eased-in/eased-out movement instead
// of snapping straight to a target speed (player run, grapple pull, etc).
[System.Serializable]
public class Momentum2D
{
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 30f;

    public float Step(float currentVelocity, float targetVelocity, float deltaTime)
    {
        float rate = Mathf.Abs(targetVelocity) > Mathf.Abs(currentVelocity)
            ? acceleration
            : deceleration;

        return Mathf.MoveTowards(currentVelocity, targetVelocity, rate * deltaTime);
    }

    public Vector2 Step(Vector2 currentVelocity, Vector2 targetVelocity, float deltaTime)
    {
        float rate = targetVelocity.magnitude > currentVelocity.magnitude
            ? acceleration
            : deceleration;

        return Vector2.MoveTowards(currentVelocity, targetVelocity, rate * deltaTime);
    }

    public static float StepAtRate(float currentVelocity, float targetVelocity, float rate, float deltaTime)
    {
        return Mathf.MoveTowards(currentVelocity, targetVelocity, rate * deltaTime);
    }
}

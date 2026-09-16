using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrappleObject : MonoBehaviour
{
    [SerializeField] private float pullSpeed = 8f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private Momentum2D pullMomentum = new Momentum2D();

    private Rigidbody2D rb;
    private Transform target;
    private bool isBeingPulled;
    private float arrivedTime = -1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StartPull(Transform pullTarget)
    {
        target = pullTarget;
        isBeingPulled = true;
        arrivedTime = -1f;
    }

    public void StopPull()
    {
        isBeingPulled = false;
        rb.linearVelocity = Vector2.zero;
    }

    public bool HasArrivedFor(float duration)
    {
        return arrivedTime >= 0f && Time.time - arrivedTime >= duration;
    }

    private void FixedUpdate()
    {
        if (!isBeingPulled || target == null) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float distance = toTarget.magnitude;

        if (distance <= stopDistance)
        {
            StopPull();
            arrivedTime = Time.time;
            return;
        }

        Vector2 targetVelocity = toTarget.normalized * pullSpeed;

        rb.linearVelocity = pullMomentum.Step(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime);
    }
}

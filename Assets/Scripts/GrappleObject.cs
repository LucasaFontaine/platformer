using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrappleObject : MonoBehaviour
{
    [SerializeField] private float pullSpeed = 8f;
    [SerializeField] private float stopDistance = 0.5f;

    private Rigidbody2D rb;
    private Transform target;
    private bool isBeingPulled;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StartPull(Transform pullTarget)
    {
        target = pullTarget;
        isBeingPulled = true;
    }

    public void StopPull()
    {
        isBeingPulled = false;
        rb.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!isBeingPulled || target == null) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float distance = toTarget.magnitude;

        if (distance <= stopDistance)
        {
            StopPull();
            return;
        }

        rb.linearVelocity = toTarget.normalized * pullSpeed;
    }
}
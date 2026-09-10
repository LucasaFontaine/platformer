using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.5f;
    [Tooltip("Which layers this pellet reacts to on hit. Leave as Everything if unsure.")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private bool destroyOnHit = true;

    private void Start()
    {
        // Self-destruct after a bit so pellets don't fly forever if they miss everything.
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        if (((1 << hitObject.layer) & hitMask) == 0)
            return;

        // Hook up damage / hit effects / impact VFX here.

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
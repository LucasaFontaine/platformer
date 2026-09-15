using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(damagePerSecond * Time.deltaTime);
    }
}
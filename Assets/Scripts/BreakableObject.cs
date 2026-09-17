using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableObject : MonoBehaviour
{
    [SerializeField] private GameObject[] fragmentPrefabs;
    [SerializeField] private float fragmentForce = 5f;
    [SerializeField] private string playerTag = "Player";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        PlayerController2D player = collision.collider.GetComponent<PlayerController2D>();
        if (player == null || !player.IsDashing) return;

        Break();
    }

    private void Break()
    {
        foreach (GameObject fragmentPrefab in fragmentPrefabs)
        {
            if (fragmentPrefab == null) continue;

            GameObject fragment = Instantiate(fragmentPrefab, transform.position, Quaternion.identity);

            Rigidbody2D fragmentRb = fragment.GetComponent<Rigidbody2D>();
            if (fragmentRb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                fragmentRb.AddForce(randomDir * fragmentForce, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
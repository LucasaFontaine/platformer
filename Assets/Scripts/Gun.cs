using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Aiming")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform firePoint;

    [Header("Gun Rotation")]
    [SerializeField] private Transform gunPivot;

    [Tooltip("Flips the gun vertically when aiming left.")]
    [SerializeField] private bool flipVerticallyWhenAimingLeft = true;

    [Header("Shot")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("Number of pellets fired per shot.")]
    [SerializeField] private int pelletCount = 5;

    [Tooltip("Total spread of the pellets in degrees.")]
    [SerializeField] private float spreadAngle = 30f;

    [Tooltip("Speed of each pellet.")]
    [SerializeField] private float projectileSpeed = 20f;

    [Tooltip("Shots per second.")]
    [SerializeField] private float fireRate = 1f;

    [Tooltip("Adds a small amount of random spread.")]
    [SerializeField] private bool addRandomJitter = true;

    private float fireCooldownTimer;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        if (firePoint == null)
            firePoint = transform;

        if (gunPivot == null)
            gunPivot = transform;
    }

    private void Update()
    {
        AimGunAtMouse();

        if (fireCooldownTimer > 0f)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        // Hold left mouse button to fire.
        if (Input.GetMouseButton(0) && fireCooldownTimer <= 0f)
        {
            Fire();
        }
    }

    private void AimGunAtMouse()
    {
        if (cam == null || gunPivot == null)
            return;

        Vector2 mouseWorld =
            cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 aimDirection =
            mouseWorld - (Vector2)gunPivot.position;

        if (aimDirection.sqrMagnitude < 0.0001f)
            return;

        float angle =
            Mathf.Atan2(
                aimDirection.y,
                aimDirection.x
            ) * Mathf.Rad2Deg;

        gunPivot.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        if (flipVerticallyWhenAimingLeft)
        {
            bool aimingLeft =
                Mathf.Abs(
                    Mathf.DeltaAngle(0f, angle)
                ) > 90f;

            Vector3 scale =
                gunPivot.localScale;

            scale.x = Mathf.Abs(scale.x);
            scale.y = aimingLeft
                ? -Mathf.Abs(scale.y)
                : Mathf.Abs(scale.y);

            gunPivot.localScale = scale;
        }
    }

    private void Fire()
    {
        // Check the required references.
        if (projectilePrefab == null)
        {
            Debug.LogError(
                "GUN ERROR: Projectile Prefab is not assigned!",
                this
            );

            return;
        }

        if (firePoint == null)
        {
            Debug.LogError(
                "GUN ERROR: Fire Point is not assigned!",
                this
            );

            return;
        }

        if (cam == null)
        {
            Debug.LogError(
                "GUN ERROR: Camera is not assigned and Camera.main was not found!",
                this
            );

            return;
        }

        // Start cooldown.
        fireCooldownTimer =
            1f / Mathf.Max(
                fireRate,
                0.01f
            );

        Vector2 mouseWorld =
            cam.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector2 spawnPosition =
            firePoint.position;

        Vector2 direction =
            mouseWorld - spawnPosition;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction =
                firePoint.right;
        }
        else
        {
            direction.Normalize();
        }

        float baseAngle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        // Spawn every pellet.
        for (int i = 0; i < pelletCount; i++)
        {
            float pelletAngle =
                baseAngle;

            // Evenly distribute pellets across the spread.
            if (pelletCount > 1)
            {
                float t =
                    (float)i /
                    (pelletCount - 1);

                pelletAngle =
                    baseAngle
                    - spreadAngle * 0.5f
                    + spreadAngle * t;
            }

            // Add slight random variation.
            if (addRandomJitter)
            {
                pelletAngle += Random.Range(
                    -spreadAngle * 0.05f,
                    spreadAngle * 0.05f
                );
            }

            SpawnPellet(
                spawnPosition,
                pelletAngle
            );
        }

        Debug.Log(
            "Gun fired " +
            pelletCount +
            " pellets."
        );
    }

    private void SpawnPellet(
        Vector2 spawnPosition,
        float angleDegrees
    )
    {
        Quaternion rotation =
            Quaternion.Euler(
                0f,
                0f,
                angleDegrees
            );

        // Create the projectile.
        GameObject pellet =
            Instantiate(
                projectilePrefab,
                spawnPosition,
                rotation
            );

        // Prevent the pellet from immediately colliding with the shooter
        // (fixes pellets spawning inside the shooter's own collider and
        // self-destructing a fraction of a second after firing).
        Collider2D pelletCollider =
            pellet.GetComponent<Collider2D>();

        if (pelletCollider != null)
        {
            Collider2D[] shooterColliders =
                GetComponentsInParent<Collider2D>();

            foreach (Collider2D shooterCollider in shooterColliders)
            {
                Physics2D.IgnoreCollision(
                    pelletCollider,
                    shooterCollider
                );
            }
        }

        // Find Rigidbody2D.
        Rigidbody2D pelletRb =
            pellet.GetComponent<Rigidbody2D>();

        if (pelletRb != null)
        {
            Vector2 direction =
                rotation * Vector2.right;

            pelletRb.linearVelocity =
                direction * projectileSpeed;
        }
        else
        {
            Debug.LogError(
                "GUN ERROR: Projectile prefab needs a Rigidbody2D!",
                pellet
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin =
            firePoint != null
                ? firePoint
                : transform;

        Transform aimSource =
            gunPivot != null
                ? gunPivot
                : origin;

        Gizmos.color = Color.yellow;

        Vector3 forward =
            aimSource.right;

        float halfSpread =
            spreadAngle * 0.5f;

        Vector3 leftEdge =
            Quaternion.Euler(
                0f,
                0f,
                halfSpread
            ) * forward;

        Vector3 rightEdge =
            Quaternion.Euler(
                0f,
                0f,
                -halfSpread
            ) * forward;

        const float coneLength = 3f;

        Gizmos.DrawLine(
            origin.position,
            origin.position +
            leftEdge * coneLength
        );

        Gizmos.DrawLine(
            origin.position,
            origin.position +
            rightEdge * coneLength
        );

        Gizmos.DrawLine(
            origin.position,
            origin.position +
            forward * coneLength
        );
    }
}
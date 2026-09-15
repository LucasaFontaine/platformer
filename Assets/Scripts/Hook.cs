using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Hook : MonoBehaviour
{
    [SerializeField] private float maxGrappleDistance = 10f;
    [SerializeField] private string grappleTag = "Grapple";
    [SerializeField] private string grappleObjectTag = "GrappleObject";
    [SerializeField] private int grappleMouseButton = 1; // 0 = left, 1 = right, 2 = middle
    [SerializeField] private Camera cam;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private float swingDrag = 0.15f;
    [SerializeField] private float grappleCooldown = 1f;
    [SerializeField] private float originOffset = 0.5f;
    [SerializeField] private float pulledObjectReleaseDelay = 0.5f;

    private Rigidbody2D rb;
    private PlayerController2D playerController;

    private bool isSwinging;
    private Vector2 anchorPoint;
    private float swingLength;
    private Vector2 radialDir;
    private float angularVelocity;
    private float grappleCooldownTimer;
    private GameObject spawnedHook;
    private GrappleObject pulledObject;

    private bool IsGrappling => isSwinging || pulledObject != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController2D>();

        if (cam == null)
        {
            cam = Camera.main;
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (grappleCooldownTimer > 0f)
        {
            grappleCooldownTimer -= Time.deltaTime;
        }

        if (!IsGrappling && grappleCooldownTimer <= 0f && Input.GetMouseButtonDown(grappleMouseButton))
        {
            TryStartGrapple();
        }
        else if (IsGrappling && (Input.GetMouseButtonUp(grappleMouseButton) || Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)))
        {
            StopGrapple();
        }

        if (pulledObject != null && pulledObject.HasArrivedFor(pulledObjectReleaseDelay))
        {
            StopGrapple();
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = IsGrappling;
            if (isSwinging)
            {
                lineRenderer.SetPosition(0, anchorPoint);
                lineRenderer.SetPosition(1, transform.position);
            }
            else if (pulledObject != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, pulledObject.transform.position);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isSwinging) return;

        Vector2 tangent = new Vector2(-radialDir.y, radialDir.x);

        float gravity = Physics2D.gravity.y * rb.gravityScale;
        float angularAccel = (gravity * tangent.y) / swingLength;
        angularVelocity += angularAccel * Time.fixedDeltaTime;
        angularVelocity *= Mathf.Clamp01(1f - swingDrag * Time.fixedDeltaTime);

        float deltaAngle = angularVelocity * Time.fixedDeltaTime;
        radialDir = Rotate(radialDir, deltaAngle);
        Vector2 newPos = anchorPoint + radialDir * swingLength;

        rb.MovePosition(newPos);

        Vector2 newTangent = new Vector2(-radialDir.y, radialDir.x);
        rb.linearVelocity = newTangent * (angularVelocity * swingLength);
    }

    private void TryStartGrapple()
    {
        if (cam == null) return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorld - (Vector2)transform.position).normalized;
        Vector2 origin = (Vector2)transform.position + direction * originOffset;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxGrappleDistance - originOffset);

        Debug.DrawRay(origin, direction * maxGrappleDistance, Color.red, 1f);

        if (hit.collider == null)
        {
            Debug.Log("Grapple raycast hit nothing.");
            return;
        }

        if (hit.collider.CompareTag(grappleObjectTag))
        {
            GrappleObject obj = hit.collider.GetComponent<GrappleObject>();
            if (obj == null)
            {
                Debug.Log($"'{hit.collider.name}' is tagged '{grappleObjectTag}' but has no GrappleObject component.");
                return;
            }

            pulledObject = obj;
            pulledObject.StartPull(transform);
            Debug.Log($"Pulling '{hit.collider.name}' towards the player.");
            return;
        }

        if (!hit.collider.CompareTag(grappleTag))
        {
            Debug.Log($"Grapple raycast hit '{hit.collider.name}' but it's tagged '{hit.collider.tag}', not '{grappleTag}'.");
            return;
        }

        Debug.Log($"Grapple attached to '{hit.collider.name}' at {hit.point}.");

        StartSwing(hit.point);
    }

    private void StartSwing(Vector2 point)
    {
        isSwinging = true;
        anchorPoint = point;
        swingLength = Vector2.Distance(anchorPoint, transform.position);
        radialDir = ((Vector2)transform.position - anchorPoint).normalized;

        Vector2 tangent = new Vector2(-radialDir.y, radialDir.x);
        angularVelocity = Vector2.Dot(rb.linearVelocity, tangent) / swingLength;

        if (playerController != null)
        {
            playerController.isSwinging = true;
        }

        if (hookPrefab != null)
        {
            spawnedHook = Instantiate(hookPrefab, anchorPoint, Quaternion.identity);
        }
    }

    private void StopGrapple()
    {
        grappleCooldownTimer = grappleCooldown;

        if (isSwinging)
        {
            isSwinging = false;

            if (playerController != null)
            {
                playerController.isSwinging = false;
                playerController.GrantJump();
            }

            if (spawnedHook != null)
            {
                Destroy(spawnedHook);
                spawnedHook = null;
            }
        }

        if (pulledObject != null)
        {
            pulledObject.StopPull();
            pulledObject = null;
        }
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxGrappleDistance);

        if (isSwinging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, anchorPoint);
            Gizmos.DrawWireSphere(anchorPoint, 0.15f);
        }
    }
}
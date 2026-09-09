using UnityEngine;

public class CameraSnap : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform snapPoint;

    private Camera cam;
    private bool hasSnapped;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (player == null || snapPoint == null)
            return;

        Vector3 viewportPos = cam.WorldToViewportPoint(player.position);

        bool playerOffScreen =
            viewportPos.x < 0f ||
            viewportPos.x > 1f ||
            viewportPos.y < 0f ||
            viewportPos.y > 1f;

        if (playerOffScreen && !hasSnapped)
        {
            SnapCamera();
        }

        if (!playerOffScreen)
        {
            hasSnapped = false;
        }
    }

    private void SnapCamera()
    {
        transform.position = new Vector3(
            snapPoint.position.x,
            snapPoint.position.y,
            transform.position.z
        );

        hasSnapped = true;
    }
}
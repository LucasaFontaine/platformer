using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;

    private float targetHeight;
    private Vector3 velocity;

    private void Awake()
    {
        targetHeight = transform.position.y;
    }

    public void SetTargetHeight(float height)
    {
        targetHeight = height;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float clampedX = Mathf.Clamp(player.position.x, minX, maxX);
        Vector3 desiredPosition = new Vector3(clampedX, targetHeight, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
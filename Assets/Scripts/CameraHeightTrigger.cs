using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraHeightTrigger : MonoBehaviour
{
    [SerializeField] private float heightA;
    [SerializeField] private float heightB;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private string playerTag = "Player";

    private bool useHeightA = true;

    private void Awake()
    {
        if (cameraFollow == null)
        {
            Camera main = Camera.main;
            if (main != null)
            {
                cameraFollow = main.GetComponent<CameraFollow>();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (cameraFollow == null) return;

        float target = useHeightA ? heightA : heightB;
        cameraFollow.SetTargetHeight(target);
        useHeightA = !useHeightA;
    }
}
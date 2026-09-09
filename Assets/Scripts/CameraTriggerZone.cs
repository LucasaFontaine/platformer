using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    [SerializeField] private Transform cameraPointA;
    [SerializeField] private Transform cameraPointB;

    private bool usingPointA = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SwitchCamera();
    }

    private void SwitchCamera()
    {
        Transform targetPoint = usingPointA ? cameraPointA : cameraPointB;

        Camera.main.transform.position = new Vector3(
            targetPoint.position.x,
            targetPoint.position.y,
            Camera.main.transform.position.z
        );

        usingPointA = !usingPointA;
    }
}
using UnityEngine;

public class CanvasFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}
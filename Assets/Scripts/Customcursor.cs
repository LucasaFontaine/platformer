using UnityEngine;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursorRect;
    [SerializeField] private Image cursorImage;

    [Tooltip("Pixel offset from the mouse position to the sprite's pivot. Use this to align the sprite's 'tip' with the actual click point.")]
    [SerializeField] private Vector2 hotspotOffset = Vector2.zero;

    private void Awake()
    {
        if (cursorRect == null)
        {
            cursorRect = GetComponent<RectTransform>();
        }

        if (cursorImage == null)
        {
            cursorImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        ApplyCursorState(Application.isFocused);
    }

    private void OnDisable()
    {
        // Always give the OS cursor back if this component goes away.
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!Application.isFocused)
            return;

        if (cursorRect != null)
        {
            cursorRect.position = (Vector2)Input.mousePosition + hotspotOffset;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        ApplyCursorState(hasFocus);
    }

    private void ApplyCursorState(bool hasFocus)
    {
        // Focused: hide the OS cursor, show our sprite.
        // Not focused: restore the OS cursor, hide our sprite (mouse input
        // isn't reliably ours while another window has focus anyway).
        Cursor.visible = !hasFocus;

        if (cursorImage != null)
        {
            cursorImage.enabled = hasFocus;
        }
    }
}
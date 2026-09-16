using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class StaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Image[] staminaSegments;
    [SerializeField] private float fadeSpeed = 3f;

    private CanvasGroup canvasGroup;
    private float previousStamina;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (player != null)
        {
            previousStamina = player.CurrentStamina;
            canvasGroup.alpha = previousStamina >= player.MaxStamina ? 0f : 1f;
        }
    }

    private void Update()
    {
        if (player == null || staminaSegments == null)
            return;

        float stamina = player.CurrentStamina;

        for (int i = 0; i < staminaSegments.Length; i++)
        {
            if (staminaSegments[i] == null)
                continue;

            staminaSegments[i].fillAmount = Mathf.Clamp01(stamina - i);
        }

        bool staminaUsed = stamina < previousStamina;
        bool isFull = stamina >= player.MaxStamina;

        if (staminaUsed)
        {
            canvasGroup.alpha = 1f;
        }
        else if (isFull)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
        }
        else
        {
            canvasGroup.alpha = 1f;
        }

        previousStamina = stamina;
    }
}
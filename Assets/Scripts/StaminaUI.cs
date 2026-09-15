using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Image[] staminaSegments;

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
    }
}
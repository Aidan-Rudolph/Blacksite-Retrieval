using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class FlashlightUI : MonoBehaviour
{
    [Tooltip("The UI Slider used as a battery bar. Min=0 Max=1.")]
    public Slider batterySlider;

    [Tooltip("Optional label that shows percentage.")]
    public TextMeshProUGUI batteryLabel;

    private static FlashlightUI instance;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(true); // hidden until flashlight is picked up
    }

    // Show or hide the battery UI.
    public static void SetVisible(bool visible)
    {
        if (instance == null) return;
        instance.gameObject.SetActive(visible);
    }

    // Update the battery bar. Pass a 0-1 normalized value.
    public static void SetBattery(float normalized)
    {
        if (instance == null) return;

        normalized = Mathf.Clamp01(normalized);

        if (instance.batterySlider != null)
            instance.batterySlider.value = normalized;

        if (instance.batteryLabel != null)
            instance.batteryLabel.text = $"Battery: {Mathf.RoundToInt(normalized * 100f)}%";

        // Change bar color based on charge level
        if (instance.batterySlider != null)
        {
            Image fill = instance.batterySlider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                if (normalized > 0.5f) fill.color = Color.green;
                else if (normalized > 0.2f) fill.color = Color.yellow;
                else fill.color = Color.red;
            }
        }
    }
}
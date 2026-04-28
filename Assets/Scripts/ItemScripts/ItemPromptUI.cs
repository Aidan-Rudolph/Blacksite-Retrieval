using UnityEngine;
using TMPro;

// Simple static UI handler for the "Press E to pick up" prompt.
// Attach this to the prompt Text object in your Canvas.

public class ItemPromptUI : MonoBehaviour
{
    [Tooltip("The TextMeshPro text object that shows the prompt message.")]
    public TextMeshProUGUI promptText;

    private static ItemPromptUI instance;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false); // hidden by default
    }

    // Show a prompt message
    public static void Show(string message)
    {
        if (instance == null) return;
        instance.promptText.text = message;
        instance.gameObject.SetActive(true);
    }

    // Hide the prompt
    public static void Hide()
    {
        if (instance == null) return;
        instance.gameObject.SetActive(false);
    }
}
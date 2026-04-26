using UnityEngine;

// A flashlight the player can pick up, toggle on/off, and recharge at base.
// Requires a Light component on this GameObject or a child object.

public class FlashlightItem : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [Tooltip("Maximum battery life in seconds.")]
    public float maxBattery = 120f;

    [Tooltip("How fast the battery drains per second while on.")]
    public float drainRate = 1f;

    [Tooltip("How fast the battery recharges per second at base.")]
    public float rechargeRate = 10f;

    [Tooltip("Key to toggle the flashlight on/off while held.")]
    public KeyCode toggleKey = KeyCode.F;

    // running states
    public float CurrentBattery { get; private set; }
    public bool IsOn { get; private set; }
    public bool IsRecharging { get; private set; }

    // references

    private Light flashlight;
    private AudioSource audioSource;

    [Header("Optional Audio")]
    public AudioClip toggleSound;
    public AudioClip batteryDeadSound;

    private void Awake()
    {
        CurrentBattery = maxBattery;
        audioSource = GetComponent<AudioSource>();

        // Look for a Light component on this object or any child
        flashlight = GetComponentInChildren<Light>();
        if (flashlight == null)
            Debug.LogWarning($"[FlashlightItem] No Light component found on {gameObject.name} or its children.");

        SetLight(false);

        FlashlightUI.SetVisible(true);
        FlashlightUI.SetBattery(1f);
    }

    private void Update()
    {
        // only local player should be able to toggle in multiplayer
        if (Input.GetKeyDown(toggleKey)) Toggle();

        if (IsRecharging)
        {
            Recharge();
            return;
        }

        // Drain battery while on
        if (IsOn)
        {
            CurrentBattery -= drainRate * Time.deltaTime;
            CurrentBattery = Mathf.Max(CurrentBattery, 0f);

            // Update UI battery bar
            FlashlightUI.SetBattery(CurrentBattery / maxBattery);

            if (CurrentBattery <= 0f) BatteryDead();
        }
    }

    // Flashlight logic 
    // Called when enter base
    public void StartCharging()
    {
        IsRecharging = true;
        IsOn = false;
        SetLight(false);
    }

    // called when exit base
    public void StopCharging()
    {
        IsRecharging = false;
    }


    private void Toggle()
    {
        if (CurrentBattery <= 0f) return; // cant turn on with dead battery

        IsOn = !IsOn;
        SetLight(IsOn);

        PlaySound(toggleSound);
    }

    private void SetLight(bool state)
    {
        if (flashlight != null) flashlight.enabled = state;
    }

    private void BatteryDead()
    {
        IsOn = false;
        SetLight(false);
        PlaySound(batteryDeadSound);
        FlashlightUI.SetBattery(0f);
    }

    private void Recharge()
    {
        if (CurrentBattery >= maxBattery)
        {
            CurrentBattery = maxBattery;
            IsRecharging = false;
            FlashlightUI.SetBattery(1f);
            return;
        }

        CurrentBattery += rechargeRate * Time.deltaTime;
        FlashlightUI.SetBattery(CurrentBattery / maxBattery);
    }

    // Sound helper
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
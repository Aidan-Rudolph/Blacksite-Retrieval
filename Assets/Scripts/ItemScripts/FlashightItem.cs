using UnityEngine;

// A flashlight the player can pick up, toggle on/off, and recharge at base.
// Requires a Light component on this GameObject or a child object.

public class FlashlightItem : ItemBase
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

    // runnint states

    public float CurrentBattery { get; private set; }
    public bool IsOn { get; private set; }
    public bool IsRecharging { get; private set; }

    // references

    private Light flashlight;
    private AudioSource audioSource;

    [Header("Optional Audio")]
    public AudioClip toggleOnSound;
    public AudioClip toggleOffSound;
    public AudioClip batteryDeadSound;

    protected override void Awake()
    {
        base.Awake();
        CurrentBattery = maxBattery;
        audioSource = GetComponent<AudioSource>();

        // Look for a Light component on this object or any child
        flashlight = GetComponentInChildren<Light>();
        if (flashlight == null)
            Debug.LogWarning($"[FlashlightItem] No Light component found on {gameObject.name} or its children.");

        SetLight(false);
    }

    protected override void Update()
    {
        base.Update(); // keep ItemBase proximity + pickup logic running

        if (IsRecharging)
        {
            Recharge();
            return;
        }

        if (!IsPickedUp) return;

        // Toggle on/off
        // only local player should be able to toggle in multiplayer
        if (Input.GetKeyDown(toggleKey)) Toggle();

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

    private void Toggle()
    {
        if (CurrentBattery <= 0f) return; // cant turn on with dead battery

        IsOn = !IsOn;
        SetLight(IsOn);

        if (IsOn) PlaySound(toggleOnSound);
        else PlaySound(toggleOffSound);
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

    // ItemBase overrides 

    protected override void OnPlayerEnterRange()
    {
        ItemPromptUI.Show($"Press E to pick up Flashlight  [{itemWeight}kg]");
    }

    protected override void OnPlayerExitRange()
    {
        ItemPromptUI.Hide();
    }

    protected override void OnPickedUp()
    {
        ItemPromptUI.Hide();
        FlashlightUI.SetVisible(true);
        FlashlightUI.SetBattery(CurrentBattery / maxBattery);
    }

    protected override void OnDropped()
    {
        SetLight(false);
        IsOn = false;
        FlashlightUI.SetVisible(false);
    }

    public override void OnReturnedToBase()
    {
        // Start recharging — BaseZone calls this
        // sync recharge state in multiplayer so it shows for all players
        IsRecharging = true;
        IsOn = false;
        SetLight(false);

        // Detach from player so it sits at base while charging
        transform.SetParent(null);
    }

    // Sound helper

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
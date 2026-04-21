using UnityEngine;

public class MonsterAttackHitbox : MonoBehaviour
{
    private MonsterBase monster;
    private Collider hitboxCollider;
    private bool hasHitThisSwing = false;

    private void Awake()
    {
        // Get the MonsterBase component from the parent GameObject and force box settings
        monster = GetComponentInParent<MonsterBase>();
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        //hitboxCollider.enabled = false;
    }

    // here if we want to enable/disable the hitbox to avoid damage when not attacking but will do that later.

    public void EnableHitbox()
    {
        hasHitThisSwing = false;
        hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        hasHitThisSwing = false;
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        // need to sync with multiplayer
        if (hasHitThisSwing) return; // Prevent multiple hits in the same attack swing
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(monster.attackDamage);

        hasHitThisSwing = true;
    }
}
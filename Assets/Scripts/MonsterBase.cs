using UnityEngine;

// Base class for all monsters.
// - Detects the player by proximity (enter range vs exit range are separate)
// - Chases directly toward the player
// - Drives the animator: Speed (Float), Attack (bool), Dead (bool), Hit (trigger)
// - Tracks health; canDie controls whether reaching 0 HP actually kills it
// - Uses Rigidbody.MovePosition for movement so collisions work properly
//
// Rigidbody settings: Is Kinematic = FALSE, Freeze Rotation X Y Z = TRUE
//
// Not sure how to connect with multiplayer

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public abstract class MonsterBase : MonoBehaviour
{

    [Header("Detection")]
    [Tooltip("Distance at which the monster starts chasing the player.")]
    public float aggroRange = 10f;

    [Tooltip("Distance the player must reach to LOSE the monster (should be > aggroRange).")]
    public float deaggroRange = 14f;

    [Header("Movement")]
    [Tooltip("How fast the monster moves while chasing.")]
    public float moveSpeed = 4f;

    [Tooltip("How quickly the monster rotates to face the player.")]
    public float rotateSpeed = 8f;

    [Header("Attack")]
    [Tooltip("Range at which the monster can hit the player.")]
    public float attackRange = 1.5f;

    [Tooltip("Seconds between attacks.")]
    public float attackCooldown = 1.2f;

    [Tooltip("Damage dealt per attack.")]
    public float attackDamage = 25f;

    [Header("Health")]
    [Tooltip("Maximum (and starting) health.")]
    public float maxHealth = 100f;

    [Tooltip("If false, the monster can be hit and reacts, but its HP never reaches 0.")]
    public bool canDie = true;

    // ── Runtime state (read-only from outside) ────────────────────

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsChasing { get; private set; }

    // ── Private refs ──────────────────────────────────────────────

    protected Animator anim;
    protected Transform player;
    private Rigidbody rb;
    private float lastAttackTime = -99f;

    // Animator parameter names — match these exactly to your Animator
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimAttack = Animator.StringToHash("Attack");
    private static readonly int AnimDead = Animator.StringToHash("Dead");
    private static readonly int AnimHit = Animator.StringToHash("Hit");



    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        CurrentHealth = maxHealth;

        // force rigid body rotation freeze
        rb.freezeRotation = true;

        // Need to fix for multiplayer
        FindPlayer();
    }

    protected virtual void Update()
    {
        // TEMP TEST — press K to kill the monster
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(50f);
        }

        if (IsDead) return;
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        UpdateChaseState(distToPlayer);

        if (IsChasing)
        {
            if (distToPlayer <= attackRange)
            {
                StopMoving();
                TryAttack();
            }
            else
            {
                // Rotation
                RotateTowardPlayer();
                anim.SetFloat(AnimSpeed, moveSpeed);
                anim.SetBool(AnimAttack, false);
            }
        }
        else
        {
            StopMoving();
            anim.SetBool(AnimAttack, false);
        }

        OnMonsterUpdate();
    }

    // Actual movement with rigidbody
    private void FixedUpdate()
    {
        if (IsDead) return;
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (IsChasing && distToPlayer > attackRange)
        {
            Vector3 newPos = rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    // aggro / deaggro 

    private void UpdateChaseState(float dist)
    {
        if (!IsChasing && dist <= aggroRange)
        {
            IsChasing = true;
            OnAggroed();
        }
        else if (IsChasing && dist > deaggroRange)
        {
            IsChasing = false;
            OnDeaggroed();
        }
    }

    // Movement 
    private void RotateTowardPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                   rotateSpeed * Time.deltaTime);
        }
    }

    private void StopMoving()
    {
        // Zero out horizontal velocity so the monster doesn't slide
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        anim.SetFloat(AnimSpeed, 0f);
    }

    // Attacking 

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        anim.SetBool(AnimAttack, true);

        MonsterAttackHitbox[] hitboxes = GetComponentsInChildren<MonsterAttackHitbox>();
        foreach (var hb in hitboxes)
        {
            hb.EnableHitbox();
        }
    }

    //  Damage / Death 
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        anim.SetTrigger(AnimHit);

        if (!canDie) return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0f);

        // need to sync health here for mulitplayer

        if (CurrentHealth <= 0f) Die();
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        IsChasing = false;

        anim.SetBool(AnimDead, true);
        anim.SetBool(AnimAttack, false);
        anim.SetFloat(AnimSpeed, 0f);

        rb.velocity = Vector3.zero;
        rb.isKinematic = true; // stop physics interactions

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // need to synce death here for multiplayer

        OnDeath();
    }

    // Overridable hooks for subclasses 

    protected virtual void OnMonsterUpdate() { }
    protected virtual void OnAggroed() { }
    protected virtual void OnDeaggroed() { }
    protected virtual void OnDeath() { }

    // Helpers 

    private void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning($"[{gameObject.name}] No GameObject tagged 'Player' found in scene.");
    }

    // visuals for ranges — only visible when the monster is selected in the editor
    // yellow - aggro, orange - deaggro, red - attack

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, deaggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
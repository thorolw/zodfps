using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float moveSpeed = 3.5f;
    public float detectionRange = 20f;
    
    [Header("Visual Effects")]
    public GameObject bloodSplatterPrefab;
    public Material hitMaterial;
    public float hitFlashDuration = 0.1f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] hurtSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] idleSounds;
    public float idleSoundInterval = 8f;
    
    [Header("Drops")]
    public GameObject[] possibleDrops;
    [Range(0, 1)]
    public float dropChance = 0.3f;
    
    // References
    private Transform player;
    private NavMeshAgent navAgent;
    private Renderer enemyRenderer;
    private Material[] originalMaterials;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private bool isDead = false;
    private float nextIdleSoundTime;
    
    // Animation
    private Animator animator;
    private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AnimAttack = Animator.StringToHash("Attack");
    private static readonly int AnimDie = Animator.StringToHash("Die");
    private static readonly int AnimTakeHit = Animator.StringToHash("TakeHit");
    
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (enemyRenderer != null)
        {
            originalMaterials = enemyRenderer.materials;
        }
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        currentHealth = maxHealth;
        nextIdleSoundTime = Time.time + Random.Range(idleSoundInterval * 0.5f, idleSoundInterval);
        
        // Set NavMeshAgent speed
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
        }
    }
    
    private void Update()
    {
        if (isDead || player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Move towards player
            if (distanceToPlayer > attackRange)
            {
                if (navAgent != null && navAgent.isActiveAndEnabled)
                {
                    navAgent.SetDestination(player.position);
                    
                    if (animator != null)
                    {
                        animator.SetBool(AnimIsMoving, true);
                    }
                }
            }
            else
            {
                // Stop moving when in attack range
                if (navAgent != null && navAgent.isActiveAndEnabled)
                {
                    navAgent.SetDestination(transform.position);
                    
                    if (animator != null)
                    {
                        animator.SetBool(AnimIsMoving, false);
                    }
                }
                
                // Look at player
                Vector3 direction = player.position - transform.position;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(direction),
                        Time.deltaTime * 5f
                    );
                }
                
                // Attack if cooldown is over
                if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
        }
        else
        {
            // Not in detection range, stop moving
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(transform.position);
                
                if (animator != null)
                {
                    animator.SetBool(AnimIsMoving, false);
                }
            }
        }
        
        // Play random idle sounds
        if (Time.time >= nextIdleSoundTime && idleSounds.Length > 0)
        {
            PlayRandomSound(idleSounds);
            nextIdleSoundTime = Time.time + Random.Range(idleSoundInterval * 0.8f, idleSoundInterval * 1.2f);
        }
    }
    
    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger(AnimAttack);
        }
        
        // Play attack sound
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }
        
        // Wait for animation to reach damage point (adjust timing based on your animation)
        yield return new WaitForSeconds(0.5f);
        
        // Apply damage if still in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Get player health component
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
        
        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Flash hit material
        StartCoroutine(FlashHitMaterial());
        
        // Play hurt sound
        if (hurtSounds.Length > 0)
        {
            PlayRandomSound(hurtSounds);
        }
        
        // Spawn blood effect
        if (bloodSplatterPrefab != null)
        {
            Instantiate(bloodSplatterPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        
        // Play hit animation
        if (animator != null)
        {
            animator.SetTrigger(AnimTakeHit);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger(AnimDie);
        }
        
        // Play death sound
        if (deathSounds.Length > 0)
        {
            PlayRandomSound(deathSounds);
        }
        
        // Disable NavMeshAgent and Collider
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // Random chance to drop an item
        if (possibleDrops.Length > 0 && Random.value <= dropChance)
        {
            int dropIndex = Random.Range(0, possibleDrops.Length);
            Instantiate(possibleDrops[dropIndex], transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
        
        // Destroy the enemy object after delay (to allow death animation and sound to play)
        Destroy(gameObject, 5f);
    }
    
    private IEnumerator FlashHitMaterial()
    {
        if (enemyRenderer == null || hitMaterial == null) yield break;
        
        // Store original materials
        Material[] originalMats = enemyRenderer.materials;
        
        // Replace all materials with hit material
        Material[] newMats = new Material[originalMats.Length];
        for (int i = 0; i < originalMats.Length; i++)
        {
            newMats[i] = hitMaterial;
        }
        enemyRenderer.materials = newMats;
        
        // Wait for flash duration
        yield return new WaitForSeconds(hitFlashDuration);
        
        // Restore original materials
        enemyRenderer.materials = originalMats;
    }
    
    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (audioSource == null || sounds.Length == 0) return;
        
        int index = Random.Range(0, sounds.Length);
        audioSource.clip = sounds[index];
        audioSource.Play();
    }
    
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

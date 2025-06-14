using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float armor = 0f;
    public float maxArmor = 100f;
    
    [Header("UI Elements")]
    public Image healthBar;
    public Image armorBar;
    public Image bloodOverlay;
    public Text healthText;
    public Text armorText;
    
    [Header("Damage Effects")]
    public float damageFlashDuration = 0.2f;
    public Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
    public float bloodOverlayMaxAlpha = 0.7f;
    public float bloodOverlayDecaySpeed = 1f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] hurtSounds;
    public AudioClip[] armorHitSounds;
    public AudioClip deathSound;
    
    [Header("Game Over")]
    public string gameOverSceneName = "GameOver";
    public float gameOverDelay = 2f;
    
    // Private variables
    private PlayerController playerController;
    private CanvasGroup bloodOverlayGroup;
    private float currentBloodAlpha = 0f;
    
    private void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        
        // Set up blood overlay
        if (bloodOverlay != null)
        {
            bloodOverlayGroup = bloodOverlay.GetComponent<CanvasGroup>();
            if (bloodOverlayGroup == null)
            {
                bloodOverlayGroup = bloodOverlay.gameObject.AddComponent<CanvasGroup>();
            }
            bloodOverlayGroup.alpha = 0f;
        }
        
        UpdateHealthUI();
    }
    
    private void Update()
    {
        // Fade blood overlay over time
        if (bloodOverlayGroup != null && currentBloodAlpha > 0)
        {
            currentBloodAlpha = Mathf.Max(0, currentBloodAlpha - (bloodOverlayDecaySpeed * Time.deltaTime));
            bloodOverlayGroup.alpha = currentBloodAlpha;
        }
    }
    
    public void TakeDamage(float damage)
    {
        float damageAfterArmor = damage;
        
        // Apply armor reduction if available
        if (armor > 0)
        {
            // Armor absorbs 2/3 of damage
            float armorDamage = damage * 0.667f;
            damageAfterArmor = damage - armorDamage;
            
            // Reduce armor
            armor -= armorDamage;
            if (armor < 0) 
            {
                // If armor is depleted, add remaining damage to health
                damageAfterArmor -= armor; // (armor is negative here)
                armor = 0;
            }
            
            // Play armor hit sound
            if (armorHitSounds.Length > 0)
            {
                PlayRandomSound(armorHitSounds);
            }
        }
        else
        {
            // No armor, play hurt sound
            if (hurtSounds.Length > 0)
            {
                PlayRandomSound(hurtSounds);
            }
        }
        
        // Apply damage to health
        currentHealth -= damageAfterArmor;
        
        // Clamp health
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Flash screen red to indicate damage
        StartCoroutine(DamageFlash());
        
        // Update UI
        UpdateHealthUI();
        
        // Check if player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void AddHealth(float amount)
    {
        if (currentHealth <= 0) return; // Can't heal if dead
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }
    
    public void AddArmor(float amount)
    {
        if (currentHealth <= 0) return; // Can't add armor if dead
        
        armor = Mathf.Min(armor + amount, maxArmor);
        UpdateHealthUI();
    }
    
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
        
        if (armorBar != null)
        {
            armorBar.fillAmount = armor / maxArmor;
        }
        
        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(currentHealth).ToString();
        }
        
        if (armorText != null)
        {
            armorText.text = Mathf.CeilToInt(armor).ToString();
        }
    }
    
    private IEnumerator DamageFlash()
    {
        // Set blood overlay based on health percentage
        if (bloodOverlayGroup != null)
        {
            float healthPercentage = 1 - (currentHealth / maxHealth);
            currentBloodAlpha = bloodOverlayMaxAlpha * healthPercentage;
            bloodOverlayGroup.alpha = currentBloodAlpha;
        }
        
        // Set damage flash
        if (bloodOverlay != null)
        {
            Color originalColor = bloodOverlay.color;
            bloodOverlay.color = damageFlashColor;
            
            yield return new WaitForSeconds(damageFlashDuration);
            
            bloodOverlay.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
    
    private void Die()
    {
        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Disable player controller
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Show max blood overlay
        if (bloodOverlayGroup != null)
        {
            bloodOverlayGroup.alpha = 1f;
        }
        
        // Go to game over scene after delay
        StartCoroutine(GameOverSequence());
    }
    
    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(gameOverDelay);
        
        // Load game over scene or restart current scene
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            // Reload current scene if game over scene not specified
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (audioSource == null || sounds.Length == 0) return;
        
        int index = Random.Range(0, sounds.Length);
        audioSource.clip = sounds[index];
        audioSource.Play();
    }
}

using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        Armor,
        Ammo,
        Weapon,
        KeyCard
    }

    [Header("Pickup Settings")]
    public PickupType type;
    public float amount = 25f;
    public int weaponId = 0;  // Used for weapon and ammo pickups
    public string keyCardColor = "";  // Used for key cards
    
    [Header("Visual Effects")]
    public float bobHeight = 0.2f;
    public float bobSpeed = 1.5f;
    public float rotationSpeed = 50f;
    public GameObject pickupEffect;
    public bool destroyOnPickup = true;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    
    // Private variables
    private Vector3 startPosition;
    private float bobTime = 0;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        // Bob up and down
        bobTime += Time.deltaTime * bobSpeed;
        transform.position = startPosition + new Vector3(0, Mathf.Sin(bobTime) * bobHeight, 0);
        
        // Rotate
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool pickedUp = false;
            
            // Apply pickup effect based on type
            switch (type)
            {
                case PickupType.Health:
                    PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                    if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
                    {
                        playerHealth.AddHealth(amount);
                        pickedUp = true;
                    }
                    break;
                    
                case PickupType.Armor:
                    PlayerHealth playerArmor = other.GetComponent<PlayerHealth>();
                    if (playerArmor != null && playerArmor.armor < playerArmor.maxArmor)
                    {
                        playerArmor.AddArmor(amount);
                        pickedUp = true;
                    }
                    break;
                    
                case PickupType.Ammo:
                    WeaponController weaponController = other.GetComponent<WeaponController>();
                    if (weaponController != null)
                    {
                        weaponController.AddAmmo(weaponId, Mathf.RoundToInt(amount));
                        pickedUp = true;
                    }
                    break;
                    
                case PickupType.Weapon:
                    // TODO: Add weapon to inventory
                    // This would need to be implemented in your weapon system
                    pickedUp = true;
                    break;
                    
                case PickupType.KeyCard:
                    GameManager gameManager = FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        gameManager.AddKeyCard(keyCardColor);
                        pickedUp = true;
                    }
                    break;
            }
            
            // If successfully picked up
            if (pickedUp)
            {
                // Play pickup sound
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Spawn pickup effect
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Destroy or deactivate the pickup
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}

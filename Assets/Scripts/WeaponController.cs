using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string name;
        public GameObject weaponModel;
        public GameObject muzzleFlash;
        public ParticleSystem impactEffect;
        public AudioClip shootSound;
        public AudioClip emptySound;
        public AudioClip reloadSound;
        public float damage = 20.0f;
        public float range = 100.0f;
        public float fireRate = 1.0f;
        public float reloadTime = 2.0f;
        public int maxAmmo = 30;
        public int currentAmmo;
        public int maxReserveAmmo = 90;
        public int reserveAmmo;
        public bool isAutomatic;
        public float recoil = 0.1f;
        public Sprite crosshairImage;
        public Sprite weaponUIImage;
        
        [HideInInspector] public bool isReloading = false;
    }
    
    [Header("Weapon Settings")]
    public List<Weapon> weapons = new List<Weapon>();
    public int currentWeaponIndex = 0;
    
    [Header("UI Elements")]
    public Text ammoText;
    public Image crosshairImage;
    public Image weaponImage;
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    [Header("Effects")]
    public float impactForce = 50.0f;
    
    // Camera shake
    private CameraShake cameraShake;
    
    // References
    private Camera playerCamera;
    private float nextTimeToFire = 0f;
    
    private void Start()
    {
        playerCamera = Camera.main;
        cameraShake = playerCamera.GetComponent<CameraShake>();
        
        // Initialize ammo for all weapons
        foreach (Weapon weapon in weapons)
        {
            weapon.currentAmmo = weapon.maxAmmo;
            weapon.reserveAmmo = weapon.maxReserveAmmo;
        }
        
        // Show initial weapon
        SwitchWeapon(0);
        UpdateAmmoUI();
    }
    
    private void Update()
    {
        if (weapons.Count == 0) return;
        
        Weapon currentWeapon = weapons[currentWeaponIndex];
        
        // Weapon switching with number keys
        for (int i = 0; i < Mathf.Min(weapons.Count, 9); i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SwitchWeapon(i);
                break;
            }
        }
        
        // Weapon switching with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            SwitchWeapon((currentWeaponIndex + 1) % weapons.Count);
        }
        else if (scroll < 0)
        {
            SwitchWeapon((currentWeaponIndex - 1 + weapons.Count) % weapons.Count);
        }
        
        // Handle reloading
        if (currentWeapon.isReloading)
            return;
            
        if (Input.GetKeyDown(KeyCode.R) && currentWeapon.currentAmmo < currentWeapon.maxAmmo && currentWeapon.reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }
        
        // Handle shooting
        bool shootInput = currentWeapon.isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
        
        if (shootInput && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / currentWeapon.fireRate;
            
            if (currentWeapon.currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                // Play empty sound
                if (currentWeapon.emptySound != null)
                {
                    audioSource.PlayOneShot(currentWeapon.emptySound);
                }
                
                // Auto-reload when empty
                if (currentWeapon.reserveAmmo > 0)
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }
    
    private void Shoot()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];
        
        // Decrease ammo
        currentWeapon.currentAmmo--;
        
        // Update UI
        UpdateAmmoUI();
        
        // Play shoot sound
        audioSource.PlayOneShot(currentWeapon.shootSound);
        
        // Show muzzle flash
        if (currentWeapon.muzzleFlash != null)
        {
            currentWeapon.muzzleFlash.SetActive(true);
            Invoke("HideMuzzleFlash", 0.05f);
        }
        
        // Add camera shake
        if (cameraShake != null)
        {
            cameraShake.Shake(currentWeapon.recoil, 0.1f);
        }
        
        // Raycast for hit detection
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, currentWeapon.range))
        {
            // Spawn impact effect
            if (currentWeapon.impactEffect != null)
            {
                ParticleSystem impact = Instantiate(currentWeapon.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact.gameObject, 2f);
            }
            
            // Apply damage if we hit an enemy
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentWeapon.damage);
            }
            
            // Apply force to rigidbodies
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }
        }
    }
    
    private void HideMuzzleFlash()
    {
        foreach (Weapon weapon in weapons)
        {
            if (weapon.muzzleFlash != null)
            {
                weapon.muzzleFlash.SetActive(false);
            }
        }
    }
    
    private IEnumerator Reload()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];
        
        currentWeapon.isReloading = true;
        
        // Play reload sound
        if (currentWeapon.reloadSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }
        
        yield return new WaitForSeconds(currentWeapon.reloadTime);
        
        // Calculate ammo after reload
        int ammoNeeded = currentWeapon.maxAmmo - currentWeapon.currentAmmo;
        int ammoToAdd = Mathf.Min(ammoNeeded, currentWeapon.reserveAmmo);
        
        currentWeapon.currentAmmo += ammoToAdd;
        currentWeapon.reserveAmmo -= ammoToAdd;
        
        currentWeapon.isReloading = false;
        
        // Update UI
        UpdateAmmoUI();
    }
    
    private void SwitchWeapon(int index)
    {
        if (index == currentWeaponIndex || index < 0 || index >= weapons.Count)
            return;
            
        // Hide all weapons
        foreach (Weapon weapon in weapons)
        {
            if (weapon.weaponModel != null)
            {
                weapon.weaponModel.SetActive(false);
            }
        }
        
        // Show selected weapon
        currentWeaponIndex = index;
        Weapon currentWeapon = weapons[currentWeaponIndex];
        
        if (currentWeapon.weaponModel != null)
        {
            currentWeapon.weaponModel.SetActive(true);
        }
        
        // Update UI
        if (crosshairImage != null && currentWeapon.crosshairImage != null)
        {
            crosshairImage.sprite = currentWeapon.crosshairImage;
        }
        
        if (weaponImage != null && currentWeapon.weaponUIImage != null)
        {
            weaponImage.sprite = currentWeapon.weaponUIImage;
        }
        
        UpdateAmmoUI();
    }
    
    private void UpdateAmmoUI()
    {
        if (ammoText == null || weapons.Count == 0) return;
        
        Weapon currentWeapon = weapons[currentWeaponIndex];
        ammoText.text = currentWeapon.currentAmmo + " / " + currentWeapon.reserveAmmo;
    }
    
    public void AddAmmo(int weaponIndex, int amount)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Count)
        {
            weapons[weaponIndex].reserveAmmo = Mathf.Min(
                weapons[weaponIndex].reserveAmmo + amount,
                weapons[weaponIndex].maxReserveAmmo
            );
            
            // Update UI if it's the current weapon
            if (weaponIndex == currentWeaponIndex)
            {
                UpdateAmmoUI();
            }
        }
    }
}

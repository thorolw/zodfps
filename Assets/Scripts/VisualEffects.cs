using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffects : MonoBehaviour
{
    // Singleton instance
    public static VisualEffects Instance;
    
    [System.Serializable]
    public class SurfaceEffect
    {
        public string surfaceTag;
        public ParticleSystem impactEffect;
        public AudioClip[] impactSounds;
        public GameObject decal;
        public float decalSize = 0.2f;
        public float decalLifetime = 30f; // How long decals stay before being destroyed
    }
    
    [Header("Impact Effects")]
    public SurfaceEffect defaultEffect;
    public List<SurfaceEffect> surfaceEffects = new List<SurfaceEffect>();
    public int maxDecals = 100; // Maximum number of decals to prevent performance issues
    
    [Header("Weapon Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletTracerPrefab;
    public GameObject bloodSprayPrefab;
    
    [Header("Environment Effects")]
    public ParticleSystem smokePrefab;
    public ParticleSystem sparksPrefab;
    public ParticleSystem explosionPrefab;
    
    // Keep track of decals for limiting
    private Queue<GameObject> activeDecals = new Queue<GameObject>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    // Create impact effect based on surface type
    public void CreateImpactEffect(RaycastHit hit)
    {
        SurfaceEffect effect = defaultEffect;
        
        // Find specific surface effect if available
        foreach (SurfaceEffect surfaceEffect in surfaceEffects)
        {
            if (hit.collider.CompareTag(surfaceEffect.surfaceTag))
            {
                effect = surfaceEffect;
                break;
            }
        }
        
        // Create particle effect
        if (effect.impactEffect != null)
        {
            ParticleSystem impact = Instantiate(effect.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            impact.transform.position += hit.normal * 0.001f; // Slight offset to prevent clipping
            Destroy(impact.gameObject, 2f); // Destroy after effect finishes
        }
        
        // Play sound
        if (effect.impactSounds != null && effect.impactSounds.Length > 0)
        {
            AudioClip clip = effect.impactSounds[Random.Range(0, effect.impactSounds.Length)];
            AudioSource.PlayClipAtPoint(clip, hit.point, 0.8f);
        }
        
        // Create decal
        if (effect.decal != null)
        {
            CreateDecal(hit, effect.decal, effect.decalSize, effect.decalLifetime);
        }
    }
    
    // Create a decal on a surface
    private void CreateDecal(RaycastHit hit, GameObject decalPrefab, float size, float lifetime)
    {
        // Check if we need to remove old decals
        if (activeDecals.Count >= maxDecals)
        {
            GameObject oldDecal = activeDecals.Dequeue();
            if (oldDecal != null)
            {
                Destroy(oldDecal);
            }
        }
        
        // Create new decal
        GameObject decal = Instantiate(decalPrefab);
        decal.transform.position = hit.point;
        decal.transform.rotation = Quaternion.LookRotation(hit.normal);
        decal.transform.position += hit.normal * 0.005f; // Slight offset to prevent z-fighting
        
        // Random rotation around normal
        decal.transform.Rotate(Vector3.forward, Random.Range(0, 360));
        
        // Random size variation
        float randomSize = Random.Range(size * 0.8f, size * 1.2f);
        decal.transform.localScale = new Vector3(randomSize, randomSize, randomSize);
        
        // Parent to hit object if possible
        if (hit.collider.transform != null)
        {
            decal.transform.SetParent(hit.collider.transform, true);
        }
        
        // Add to queue and destroy after lifetime
        activeDecals.Enqueue(decal);
        Destroy(decal, lifetime);
    }
    
    // Create muzzle flash at gun position
    public void CreateMuzzleFlash(Transform gunTransform)
    {
        if (muzzleFlashPrefab == null || gunTransform == null) return;
        
        GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, gunTransform.position, gunTransform.rotation);
        muzzleFlash.transform.SetParent(gunTransform);
        
        // Random rotation and scale variation for variety
        muzzleFlash.transform.Rotate(0, 0, Random.Range(0, 360));
        float randomScale = Random.Range(0.8f, 1.2f);
        muzzleFlash.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        
        Destroy(muzzleFlash, 0.05f);
    }
    
    // Create bullet tracer effect
    public void CreateBulletTracer(Vector3 startPos, Vector3 endPos, float speed = 100f)
    {
        if (bulletTracerPrefab == null) return;
        
        GameObject tracer = Instantiate(bulletTracerPrefab, startPos, Quaternion.identity);
        
        // Point tracer in direction of travel
        tracer.transform.forward = (endPos - startPos).normalized;
        
        // Calculate distance and destroy time
        float distance = Vector3.Distance(startPos, endPos);
        float destroyTime = distance / speed;
        
        // Set scale based on distance
        tracer.transform.localScale = new Vector3(tracer.transform.localScale.x, 
                                                 tracer.transform.localScale.y, 
                                                 distance);
        
        Destroy(tracer, destroyTime + 0.1f);
    }
    
    // Create blood spray effect
    public void CreateBloodEffect(Vector3 position, Vector3 normal)
    {
        if (bloodSprayPrefab == null) return;
        
        GameObject blood = Instantiate(bloodSprayPrefab, position, Quaternion.LookRotation(normal));
        
        // Random rotation and scale for variety
        blood.transform.Rotate(0, Random.Range(0, 360), 0);
        float randomScale = Random.Range(0.8f, 1.5f);
        blood.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        
        Destroy(blood, 3f);
    }
    
    // Create environmental smoke
    public void CreateSmoke(Vector3 position, float duration = 5f)
    {
        if (smokePrefab == null) return;
        
        ParticleSystem smoke = Instantiate(smokePrefab, position, Quaternion.identity);
        Destroy(smoke.gameObject, duration);
    }
    
    // Create sparks
    public void CreateSparks(Vector3 position, Vector3 normal)
    {
        if (sparksPrefab == null) return;
        
        ParticleSystem sparks = Instantiate(sparksPrefab, position, Quaternion.LookRotation(normal));
        Destroy(sparks.gameObject, 2f);
    }
    
    // Create explosion
    public void CreateExplosion(Vector3 position, float scale = 1f)
    {
        if (explosionPrefab == null) return;
        
        ParticleSystem explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.transform.localScale = Vector3.one * scale;
        
        // Add explosion force to nearby objects
        Collider[] colliders = Physics.OverlapSphere(position, 5f * scale);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(500f * scale, position, 5f * scale, 0.3f);
            }
            
            // Apply damage to nearby enemies
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                float damage = 100f * scale * (1f - (distance / (5f * scale)));
                enemy.TakeDamage(damage);
            }
        }
        
        Destroy(explosion.gameObject, 5f);
    }
}

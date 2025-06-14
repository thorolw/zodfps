using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isShaking = false;
    
    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }
    
    public void Shake(float intensity, float duration)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }
    }
    
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        
        float elapsed = 0.0f;
        
        while (elapsed < duration)
        {
            // Calculate shake values
            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);
            float x = Random.Range(-1f, 1f) * intensity * damper;
            float y = Random.Range(-1f, 1f) * intensity * damper;
            
            // Apply position shake
            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            
            // Apply slight rotational shake (optional)
            float rotX = Random.Range(-1f, 1f) * intensity * 5f * damper;
            float rotY = Random.Range(-1f, 1f) * intensity * 5f * damper;
            transform.localRotation = Quaternion.Euler(originalRotation.eulerAngles.x + rotX, originalRotation.eulerAngles.y + rotY, originalRotation.eulerAngles.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Return to original position and rotation
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        
        isShaking = false;
    }
}

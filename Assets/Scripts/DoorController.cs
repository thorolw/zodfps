using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public enum DoorType
    {
        Normal,
        KeyCard,
        Switch,
        Enemy
    }

    [Header("Door Settings")]
    public DoorType doorType = DoorType.Normal;
    public string keyCardColor = "";  // Only used if doorType is KeyCard
    public int enemiesRequired = 0;   // Only used if doorType is Enemy
    
    [Header("Door Animation")]
    public bool slidingDoor = true;
    public float openDistance = 3f;
    public float openSpeed = 2f;
    public float closeDelay = 2f;     // Only used for automatic doors
    public bool stayOpen = false;
    public Vector3 slideDirection = Vector3.up;
    public float rotationAngle = 90f; // Only used if slidingDoor is false
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    
    // Private variables
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private bool isLocked = false;
    private bool isMoving = false;
    private Coroutine autoCloseCoroutine;
    
    private void Start()
    {
        // Initialize positions
        closedPosition = transform.position;
        
        // If it's a sliding door
        if (slidingDoor)
        {
            openPosition = closedPosition + slideDirection.normalized * openDistance;
        }
        else
        {
            // For rotating doors
            closedRotation = transform.rotation;
            Vector3 rotationAxis = Vector3.up; // Default rotate around Y axis
            openRotation = Quaternion.Euler(transform.eulerAngles + (rotationAxis * rotationAngle));
        }
        
        // Set initial lock state
        switch (doorType)
        {
            case DoorType.KeyCard:
                isLocked = true;
                break;
                
            case DoorType.Switch:
                isLocked = true;
                break;
                
            case DoorType.Enemy:
                isLocked = true;
                break;
        }
    }
    
    public void Interact()
    {
        // Check if door is locked
        if (isLocked)
        {
            // Handle different door types
            switch (doorType)
            {
                case DoorType.KeyCard:
                    GameManager gameManager = GameManager.Instance;
                    if (gameManager != null && gameManager.HasKeyCard(keyCardColor))
                    {
                        Unlock();
                        ToggleDoor();
                    }
                    else
                    {
                        PlayLockedSound();
                        Debug.Log("You need a " + keyCardColor + " keycard to open this door.");
                    }
                    break;
                    
                case DoorType.Switch:
                case DoorType.Enemy:
                default:
                    PlayLockedSound();
                    break;
            }
        }
        else
        {
            ToggleDoor();
        }
    }
    
    public void Unlock()
    {
        isLocked = false;
        Debug.Log("Door unlocked!");
    }
    
    public void UnlockWithSwitch()
    {
        if (doorType == DoorType.Switch)
        {
            Unlock();
            ToggleDoor();
        }
    }
    
    // Called by the GameManager when enough enemies are killed
    public void CheckEnemyRequirement()
    {
        if (doorType == DoorType.Enemy)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.enemiesKilled >= enemiesRequired)
            {
                Unlock();
                ToggleDoor();
            }
        }
    }
    
    // Toggle door open/closed
    public void ToggleDoor()
    {
        if (isMoving) return;
        
        if (!isOpen)
        {
            OpenDoor();
        }
        else if (!stayOpen)
        {
            CloseDoor();
        }
    }
    
    public void OpenDoor()
    {
        if (isOpen || isMoving) return;
        
        if (audioSource != null && openSound != null)
        {
            audioSource.clip = openSound;
            audioSource.Play();
        }
        
        StartCoroutine(AnimateDoor(true));
        
        // If door automatically closes
        if (!stayOpen)
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
            }
            autoCloseCoroutine = StartCoroutine(AutoCloseDoor());
        }
    }
    
    public void CloseDoor()
    {
        if (!isOpen || isMoving) return;
        
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        if (audioSource != null && closeSound != null)
        {
            audioSource.clip = closeSound;
            audioSource.Play();
        }
        
        StartCoroutine(AnimateDoor(false));
    }
    
    private IEnumerator AnimateDoor(bool opening)
    {
        isMoving = true;
        float time = 0;
        
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        Vector3 endPos;
        Quaternion endRot;
        
        if (opening)
        {
            endPos = slidingDoor ? openPosition : transform.position;
            endRot = slidingDoor ? transform.rotation : openRotation;
        }
        else
        {
            endPos = closedPosition;
            endRot = closedRotation;
        }
        
        while (time < 1)
        {
            time += Time.deltaTime * openSpeed;
            
            if (slidingDoor)
            {
                transform.position = Vector3.Lerp(startPos, endPos, time);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(startRot, endRot, time);
            }
            
            yield return null;
        }
        
        // Ensure door reaches exact position/rotation
        if (slidingDoor)
        {
            transform.position = endPos;
        }
        else
        {
            transform.rotation = endRot;
        }
        
        isOpen = opening;
        isMoving = false;
    }
    
    private IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(closeDelay);
        CloseDoor();
    }
    
    private void PlayLockedSound()
    {
        if (audioSource != null && lockedSound != null)
        {
            audioSource.clip = lockedSound;
            audioSource.Play();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw indicator for open position
        Gizmos.color = Color.green;
        if (slidingDoor)
        {
            Vector3 openPos = transform.position + slideDirection.normalized * openDistance;
            Gizmos.DrawLine(transform.position, openPos);
            Gizmos.DrawWireCube(openPos, new Vector3(0.3f, 0.3f, 0.3f));
        }
        else
        {
            // For rotating doors
            Vector3 rotationAxis = Vector3.up;
            Quaternion openRot = Quaternion.Euler(transform.eulerAngles + (rotationAxis * rotationAngle));
            Vector3 direction = openRot * Vector3.forward;
            Gizmos.DrawRay(transform.position, direction * 2);
        }
    }
}

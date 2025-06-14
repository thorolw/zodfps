using UnityEngine;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public LayerMask interactableMask;
    
    [Header("UI Elements")]
    public GameObject interactionPrompt;
    public Text interactionText;
    
    // Camera reference
    private Camera playerCamera;
    
    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        
        // Hide prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void Update()
    {
        HandleInteractions();
    }
    
    private void HandleInteractions()
    {
        // Raycast from camera center
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        
        // Check if we're looking at an interactable object
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableMask))
        {
            // Get interactable component
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                // Show interaction prompt
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                    
                    if (interactionText != null)
                    {
                        interactionText.text = interactable.GetInteractionPrompt();
                    }
                }
                
                // Check for interaction input
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact(this.gameObject);
                }
                
                return;
            }
        }
        
        // If we reach here, we're not looking at anything interactable
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
}

// Interface for all interactable objects
public interface IInteractable
{
    string GetInteractionPrompt();
    void Interact(GameObject interactor);
}

// Example implementation for a door
public class InteractableDoor : MonoBehaviour, IInteractable
{
    public DoorController door;
    public string interactionPrompt = "Press E to open door";
    
    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }
    
    public void Interact(GameObject interactor)
    {
        if (door != null)
        {
            door.Interact();
        }
    }
}

// Example implementation for a pickup
public class InteractablePickup : MonoBehaviour, IInteractable
{
    public string interactionPrompt = "Press E to pick up";
    
    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }
    
    public void Interact(GameObject interactor)
    {
        PickupItem pickup = GetComponent<PickupItem>();
        if (pickup != null)
        {
            // Trigger pickup manually
            pickup.PickUp(interactor);
        }
    }
}

// Example implementation for a switch
public class InteractableSwitch : MonoBehaviour, IInteractable
{
    public DoorController[] connectedDoors;
    public string interactionPrompt = "Press E to activate switch";
    public bool oneTimeUse = true;
    private bool used = false;
    
    public string GetInteractionPrompt()
    {
        return used && oneTimeUse ? "Switch already used" : interactionPrompt;
    }
    
    public void Interact(GameObject interactor)
    {
        if (used && oneTimeUse) return;
        
        // Activate all connected doors
        foreach (DoorController door in connectedDoors)
        {
            if (door != null)
            {
                door.UnlockWithSwitch();
            }
        }
        
        // Mark as used
        used = true;
        
        // Play activation sound/animation here if needed
    }
}

// Extension for PickupItem to allow manual pickup
public partial class PickupItem : MonoBehaviour
{
    public void PickUp(GameObject player)
    {
        // Simulate the player entering the trigger
        OnTriggerEnter(player.GetComponent<Collider>());
    }
}

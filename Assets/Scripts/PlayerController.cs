using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 7.0f;
    public float runSpeed = 12.0f;
    public float jumpForce = 8.0f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2.0f;
    public Transform playerCamera;
    public float maxLookAngle = 80.0f;
    
    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.4f;
    
    // Private variables
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float verticalRotation = 0f;
    private float footstepTimer = 0f;
    private bool isRunning = false;
    private float gravity = 20.0f;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // Lock cursor to center of screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        // Look rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate player horizontally
        transform.Rotate(0, mouseX, 0);
        
        // Rotate camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        
        // Movement
        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : movementSpeed;
        
        float moveForward = Input.GetAxis("Vertical") * currentSpeed;
        float moveSide = Input.GetAxis("Horizontal") * currentSpeed;
        
        // Calculate movement direction
        Vector3 movement = new Vector3(moveSide, 0, moveForward);
        movement = transform.rotation * movement;
        
        // Apply gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -0.5f; // Small downward force when grounded
            
            // Jump
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
        }
        
        // Set horizontal movement
        moveDirection.x = movement.x;
        moveDirection.z = movement.z;
        
        // Apply movement
        characterController.Move(moveDirection * Time.deltaTime);
        
        // Play footstep sounds
        if (characterController.isGrounded && (moveForward != 0 || moveSide != 0))
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                footstepTimer = isRunning ? footstepInterval / 1.5f : footstepInterval;
                PlayFootstepSound();
            }
        }
    }
    
    private void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0 || footstepSource == null) return;
        
        int index = Random.Range(0, footstepSounds.Length);
        footstepSource.clip = footstepSounds[index];
        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.Play();
    }
}

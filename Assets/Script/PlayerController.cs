using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCam;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpPower = 0f;      // keep 0 for horror if you don't want jumping
    public float gravity = 10f;

    [Header("Look")]
    public float lookSpeed = 2f;
    public float lookXLimit = 75f;
    public float cameraRotationSmooth = 5f;

    [Header("Zoom")]
    public int zoomFOV = 35;
    public float cameraZoomSmooth = 8f;

    [Header("Footsteps")]
    public AudioClip[] woodFootstepSounds;
    public AudioClip[] tileFootstepSounds;
    public AudioClip[] carpetFootstepSounds;

    public Transform footstepAudioPosition;
    public AudioSource audioSource;


    
    private bool isWalking = false;
    private bool isFootstepCoroutineRunning = false;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float rotationY = 0;

    private float initialFOV;
    private bool isZoomed = false;
    private bool canMove = true;

    private CharacterController characterController;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (!playerCam) playerCam = Camera.main;
        initialFOV = playerCam.fieldOfView;

       

        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (characterController == null || !characterController.enabled || !gameObject.activeInHierarchy)
    return;
        
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        
        if (jumpPower > 0f && Input.GetButton("Jump") && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);

        
        if (canMove)
        {
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            rotationY += Input.GetAxis("Mouse X") * lookSpeed;

            Quaternion targetRotationX = Quaternion.Euler(rotationX, 0, 0);
            Quaternion targetRotationY = Quaternion.Euler(0, rotationY, 0);

            playerCam.transform.localRotation = Quaternion.Slerp(playerCam.transform.localRotation, targetRotationX, Time.deltaTime * cameraRotationSmooth);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationY, Time.deltaTime * cameraRotationSmooth);
        }

        
        if (Input.GetButtonDown("Fire2")) isZoomed = true;
        if (Input.GetButtonUp("Fire2")) isZoomed = false;

        float targetFOV = isZoomed ? zoomFOV : initialFOV;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, Time.deltaTime * cameraZoomSmooth);

        // Footsteps
        bool moving = (Mathf.Abs(curSpeedX) > 0.01f || Mathf.Abs(curSpeedY) > 0.01f) && characterController.isGrounded;

        if (moving && !isWalking && !isFootstepCoroutineRunning)
        {
            isWalking = true;
            float footstepDelay = 1.3f / (isRunning ? runSpeed : walkSpeed);
        }
        else if (!moving)
        {
            isWalking = false;
        }
    }

   

    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    
    public void SetCanMove(bool value) => canMove = value;
}
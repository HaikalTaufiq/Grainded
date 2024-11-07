using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float rotationSpeed = 500f;
    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;


    [Header("UI Info")]
    [SerializeField] private CastUIController castUIController;
    [SerializeField] private InventoryUIController inventoryUIController;
    [SerializeField] private ShopController shopController;
    [SerializeField] private float currentCastPower = 0f;

    private InventoryController inventoryController;
    private float factorCastPower = 100f;
    public bool isGrounded;
    public float moveAmount { get; private set; } // Menjadikan moveAmount sebagai properti yang bisa dibaca dari luar

    float ySpeed;
    private bool isPicking = false;
    private bool hasHooked = false;

    Quaternion targetRotation;
    CameraController cameraController;
    Animator animator;
    CharacterController characterController;
    BaitInfo baitInfo;

    public enum GameState { noCollect, isReady, isInventory, isCast, isWater }
    public GameState currentGameState;

    [Header("Game State")]
    [SerializeField] public GameState gameState = GameState.noCollect;
    [SerializeField] private FishingLineController floaterController = null;
    [SerializeField] private FishingLineController fishingRodController = null;
    [SerializeField] private FishingLineController cargoController = null;
    [SerializeField] private FishingLineController hookController = null;

    [Header("3D Objects")]
    [SerializeField] private MeshRenderer cylinderRenderer = null;
    [SerializeField] private MeshRenderer sphereRenderer = null;
    [SerializeField] private LineRenderer fishingRodlineRenderer = null;
    [SerializeField] private LineRenderer floaterlineRenderer = null;

    [Header("SFX")]
    [SerializeField] public AudioSource castingAudioSource;
    [SerializeField] public AudioSource windingAudioSource;
    [SerializeField] public AudioSource hookingAudioSource;
    public AudioSource splashAudioSource;

    private float fishingAmountTarget = 0f;
    private float fishingAmountSmoothTime = 0.1f; // Waktu transisi yang halus
    private float fishingAmountVelocity = 0f; // Variabel untuk SmoothDamp
    private float castDelay = 1.9f; // Waktu delay sebelum fishing rod ditransform
    private float soundDelay = 1.3f; // Waktu delay sebelum fishing rod ditransform
    private IEnumerator castCoroutine;
    private IEnumerator audioCoroutine;





    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        inventoryController = GetComponent<InventoryController>();
    }

    private void Update()
    {
        ManageVisibility();

        switch (gameState)
        {
            case GameState.noCollect:
                HandleMovement();
                HandleRotation();
                ShowInventory();
                CheckBait();
                Picking();
                if (windingAudioSource.isPlaying)
                {
                    windingAudioSource.Stop();
                }


                break;

            case GameState.isReady:
                ShowInventory();
                HandleMovement();
                HandleRotation();
                Picking();
                CheckBait();
                SetFloaterDepth();
                animator.SetBool("isFishing", false);

                if (windingAudioSource.isPlaying)
                {
                    windingAudioSource.Stop();
                }

                if (Input.GetKey(KeyCode.Mouse0))
                {
                    gameState = GameState.isCast;
                    CastFishingRod();

                    castUIController.SetCastSliderActive(true);

                    SetIsReadyAll(false);
                }

                break;

            case GameState.isInventory:

                break;

            case GameState.isCast:
                HandleRotation();
                CastFishingRod();

                break;

            case GameState.isWater:
                HandleRotation();
                FishingRodHooking();
                WindingFishingRod();
                break;

            default:
                HandleMovement();
                HandleRotation();
                Picking();
                break;
        }
    }

    public void Victory()
    {
        animator.SetBool("isVictory", true);
        StartCoroutine(ResetVictoryBool());
    }

    private IEnumerator ResetVictoryBool()
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("isVictory", false);
    }

    private void CheckBait()
    {
        // Mendapatkan item dari InventoryController
        List<Item> items = inventoryController.GetItem();

        // Memeriksa apakah ada bait di inventori
        bool hasBait = false;

        foreach (var item in items)
        {
            if (item.GetItemType() == Item.ItemType.Bait)
            {
                hasBait = true;
                break;
            }
        }

        // Jika tidak ada bait, ubah game state ke 'noCollect'
        if (!hasBait)
        {
            gameState = GameState.noCollect;
            Debug.Log("No bait available, game state set to noCollect");
        }
        else
        {
            // Jika ada bait, ubah game state ke 'isReady'
            gameState = GameState.isReady;
            Debug.Log("Bait found, game state set to isReady");
        }

    }

    private void Picking()
    {
        if (!isPicking) // Cegah pergerakan jika sedang melakukan animasi picking
        {
            HandleMovement();
            HandleRotation();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            StartPicking();
        }
    }

    private void StartPicking()
    {
        isPicking = true;
        animator.SetBool("isPicking", true);

        Invoke("StopPicking", 1.3f);
    }

    private void StopPicking()
    {
        isPicking = false;
        animator.SetBool("isPicking", false);
    }

    private void ManageVisibility()
    {
        bool isVisible = (gameState == GameState.isWater);

        if (cylinderRenderer != null)
            cylinderRenderer.enabled = isVisible;

        if (sphereRenderer != null)
            sphereRenderer.enabled = isVisible;

        if (fishingRodlineRenderer != null)
            fishingRodlineRenderer.enabled = isVisible;

        if (floaterlineRenderer != null)
            floaterlineRenderer.enabled = isVisible;

    }


    private void FishingRodHooking()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            fishingAmountTarget = 1f;
            fishingRodController.Hooking();
            hasHooked = true;

            hookingAudioSource.Stop();
            hookingAudioSource.time = 0.8f;
            hookingAudioSource.Play();
        }

        float currentFishingAmount = animator.GetFloat("fishingAmount");
        float smoothFishingAmount = Mathf.Lerp(currentFishingAmount, fishingAmountTarget, Time.deltaTime);
        animator.SetFloat("fishingAmount", smoothFishingAmount);

        if (hasHooked && Mathf.Approximately(smoothFishingAmount, fishingAmountTarget))
        {
            fishingAmountTarget = 0.5f;
            hasHooked = false;

        }
    }

    private void WindingFishingRod()
    {
        if (Input.GetKey(KeyCode.U))
        {
            fishingRodController.UpdateWinch();
            ReelingIn();

            if (!windingAudioSource.isPlaying)
            {
                windingAudioSource.time = 0;
                windingAudioSource.Play();
            }
        }
        else if (Input.GetKeyUp(KeyCode.U))
        {
            fishingRodController.UpdateWinch();
            fishingAmountTarget = 0.5f; // Mengatur target animasi

            if (windingAudioSource.isPlaying)
            {
                windingAudioSource.Stop();
            }
        }

        float currentFishingAmount = animator.GetFloat("fishingAmount");
        float smoothFishingAmount = Mathf.SmoothDamp(currentFishingAmount, fishingAmountTarget, ref fishingAmountVelocity, fishingAmountSmoothTime);

        animator.SetFloat("fishingAmount", smoothFishingAmount);

        if (fishingRodController.GetIsReady())
        {
            if (gameState != GameState.isReady)
            {
                gameState = GameState.isReady;
                SetIsReadyAll(true);
            }
            else
            {
                gameState = GameState.isReady; // Set gameState ke noCollect jika tidak siap
            }
        }
    }

    public void ReelingIn()
    {
        fishingAmountTarget = 0f;
        float currentFishingAmount = animator.GetFloat("fishingAmount");
        float smoothFishingAmount = Mathf.SmoothDamp(currentFishingAmount, fishingAmountTarget, ref fishingAmountVelocity, fishingAmountSmoothTime);

        animator.SetFloat("fishingAmount", smoothFishingAmount);
    }

    private void SetFloaterDepth()
    {
        if (Input.GetKey(KeyCode.P))
        {

            floaterController.UpdateFloaterDepth();
        }
        else if (Input.GetKey(KeyCode.O))
        {
            floaterController.UpdateFloaterDepth();
        }
    }


    private void CastFishingRod()
    {
        if (Input.GetMouseButton(0))  // 0 berarti tombol kiri mouse
        {
            currentCastPower += factorCastPower * Time.deltaTime;
            if (currentCastPower > 100f)
                currentCastPower = 100f;

            castUIController.SetCastPowerSlider(currentCastPower);
            castUIController.SetCastSliderActive(true); // Menampilkan slider
            gameState = GameState.isCast;

        }

        else if (Input.GetMouseButtonUp(0))  // 0 berarti tombol kiri mouse
        {
            castUIController.SetCastSliderActive(false);
            cameraController.Casting();

            animator.SetBool("isCasting", true);

            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
            }
            audioCoroutine = AudioDelayed();
            StartCoroutine(audioCoroutine);


            if (castCoroutine != null)
            {
                StopCoroutine(castCoroutine);
            }
            castCoroutine = CastFishingRodDelayed();
            StartCoroutine(castCoroutine);


            gameState = GameState.isWater;

        }
    }

    private IEnumerator AudioDelayed()
    {
        yield return new WaitForSeconds(soundDelay);

        if (!castingAudioSource.isPlaying)
        {
            castingAudioSource.Play();
        }
        splashAudioSource.Stop(); // Hentikan audio yang sedang diputar
        splashAudioSource.time = 0; // Mulai dari awal audio
        splashAudioSource.Play();

    }

    private IEnumerator CastFishingRodDelayed()
    {
        yield return new WaitForSeconds(castDelay);
        animator.SetBool("isCasting", false);
        animator.SetBool("isFishing", true);
        animator.SetFloat("fishingAmount", 0f);

        fishingRodController.CastFishingRod(currentCastPower, transform.forward);
        currentCastPower = 0f;

        gameState = GameState.isWater;

    }



    private void SetIsReadyAll(bool isReady)
    {
        if (fishingRodController != null)
        {
            fishingRodController.SetIsReadyFishingRod(isReady);
        }
        if (floaterController != null)
        {
            floaterController.SetIsReadyFishingRod(isReady);
        }
        if (cargoController != null)
        {
            cargoController.SetIsReadyFishingRod(isReady);
        }
        if (hookController != null)
        {
            hookController.SetIsReadyFishingRod(isReady);
        }
    }




    public AudioSource footstepAudioSource;
    public float timeBetweenStepsWalking = 1f;
    public float timeBetweenStepsRunning = 0.8f;
    private float stepTimer;

    public void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Hitung moveAmount
        float moveAmount = Mathf.Abs(h) + Mathf.Abs(v);
        moveAmount = Mathf.Clamp(moveAmount, 0f, 1f); // Batasi agar tidak lebih dari 1

        var moveInput = new Vector3(h, 0, v).normalized;
        var moveDir = cameraController.PlanarRotation * moveInput;

        GroundCheck();
        if (isGrounded)
        {
            ySpeed = -0.05f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        float currentMoveSpeed = IsRunning() ? runSpeed : moveSpeed;
        var velocity = moveDir * currentMoveSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            animator.SetFloat("moveAmount", IsRunning() ? 1f : 0.2f, 0.1f, Time.deltaTime);
            // Memainkan suara footstep
            if (isGrounded && stepTimer <= 0f)
            {
                footstepAudioSource.Play();
                stepTimer = IsRunning() ? timeBetweenStepsRunning : timeBetweenStepsWalking; // Atur jeda langkah
            }
        }
        else
        {
            animator.SetFloat("moveAmount", 0f, 0.1f, Time.deltaTime); // Idle
        }
        // Mengurangi timer langkah
        if (stepTimer > 0f)
        {
            stepTimer -= Time.deltaTime;
        }
    }

    public void HandleRotation()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            var moveDir = cameraController.PlanarRotation * new Vector3(h, 0, v).normalized;
            targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public bool IsRunning()
    {
        return Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
    }

    public void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }


    private void ShowInventory()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Tab pressed");
            if (inventoryUIController.ShowInventoryPanel(inventoryController))
            {
                gameState = GameState.isInventory;
                Cursor.visible = true;
                cameraController.CheckCursor();
            }
            else
            {
                gameState = GameState.isReady;
                Cursor.visible = false;
            }
            cameraController.CheckCursor();
        }
    }


    private void ShowShop()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (shopController.ShowShopPanel())
            {
                Cursor.visible = true;
            }
            else
            {
                gameState = GameState.isReady;
                Cursor.visible = false;

            }
        }
    }

}

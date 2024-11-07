using System;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public float speed = 2f;
    public float turnSpeed = 2f;
    public Transform body;
    public Transform mouth;
    public Transform tail;
    public float movementLimit = 10f;
    public Transform bait;
    public PlayerController playerController;
    public float baitProximityThreshold = 1f;

    private Vector3 startingPosition;
    private Vector3 direction;
    private Vector3 targetDirection;
    private bool isReversing = false;
    private bool isChasingBait = false;
    public bool isStickingToBait = false;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private FishInfo fishInfo;
    [SerializeField] private float fishCatchZoneDistance = 5f; // Zona jarak ambil ikan
    [SerializeField] private Transform player; // Referensi ke transform pemain
    private bool isInCatchZone = false; // Menyimpan status apakah ikan sudah di zona ambil
    [SerializeField] private float zigZagAmplitude = 0.5f; // Amplitudo gerakan zig-zag (jarak samping)
    [SerializeField] private float zigZagFrequency = 2.0f; // Frekuensi gerakan zig-zag (seberapa cepat ikan berzig-zag)
    public bool isInWater = true; // Status apakah ikan berada di area bertag water
    [SerializeField] private float waterSurfaceY = 0.0f;
    public AudioSource hitting;

    void Start()
    {
        startingPosition = transform.position;
        direction = mouth.forward;
        targetDirection = direction;
    }

    void Update()
    {
        UpdateGameState();

        if (playerController.currentGameState == PlayerController.GameState.isWater && bait != null)
        {
            isChasingBait = true;

        }
        else
        {
            isChasingBait = false;

        }

        if (isStickingToBait)
        {
            StickToBait();
            playerController.gameState = PlayerController.GameState.isWater;
        }
        else if (isChasingBait)
        {
            ChaseBait();
        }
        else
        {
            SwimNormally();
        }

        CheckFishZone();

        if (Input.GetKeyDown(KeyCode.F))
        {
            ReleaseFish();
        }

        AdjustMouthAndTail();
    }

    private void CheckFishZone()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= fishCatchZoneDistance)
        {
            isInCatchZone = true;
            Debug.Log("Ikan sudah di zona ambil");
        }
        else
        {
            isInCatchZone = false;
        }
    }

    private void UpdateGameState()
    {
        if (bait == null || playerController == null) return;

        if (bait.position.y > 0)
        {
            playerController.currentGameState = PlayerController.GameState.isReady;
        }
        else
        {
            playerController.currentGameState = PlayerController.GameState.isWater;
        }
    }

    private void SwimNormally()
    {
        if (!isInWater) // Jika tidak berada di dalam area bertag water, hentikan perlawanan
        {
            transform.position = bait.position;
            return;
        }

        float distanceTraveled = Vector3.Distance(startingPosition, transform.position);

        if (distanceTraveled >= movementLimit)
        {
            targetDirection = direction * -1;
            startingPosition = transform.position;
            isReversing = true;
        }
        else
        {
            isReversing = false;
        }

        direction = Vector3.Slerp(direction, targetDirection, turnSpeed * Time.deltaTime);
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        body.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void ChaseBait()
    {
        Vector3 baitDirection = (bait.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, bait.position) <= baitProximityThreshold)
        {
            isStickingToBait = true;
            hitting.Stop(); // Hentikan audio yang sedang diputar
            hitting.time = 0; // Mulai dari awal audio
            hitting.Play();
            isChasingBait = false;
        }
        else
        {
            direction = Vector3.Slerp(direction, baitDirection, turnSpeed * Time.deltaTime);
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
            body.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }

    public float someThreshold = 5f;
    private void StickToBait()
    {


        if (transform.position.y > waterSurfaceY)
        {
            isInWater = false; // Ikan berada di luar air
        }
        else
        {
            isInWater = true; // Ikan berada di dalam air
        }

        if (!isInWater) // Jika tidak berada di dalam air, hentikan perlawanan
        {
            transform.position = bait.position;
            return;
        }

        // Tentukan posisi ikan mengikuti umpan
        transform.position = bait.position;

        // Rotasi tubuh ikan menghadap ke arah umpan
        body.rotation = Quaternion.LookRotation(bait.forward, Vector3.up);

        // Dapatkan posisi pemain
        Vector3 playerPosition = player.transform.position; // Pastikan player sudah dideklarasikan dan dirujuk dengan benar
        Vector3 directionToPlayer = transform.position - playerPosition; // Arah dari ikan ke pemain

        // Hitung jarak antara ikan dan pemain
        float distanceToPlayer = directionToPlayer.magnitude;

        // Jika ikan terlalu dekat dengan pemain, berikan perlawanan
        if (distanceToPlayer < someThreshold) // ganti someThreshold dengan nilai jarak yang diinginkan
        {
            // Normalisasi arah untuk bergerak menjauh dari pemain
            directionToPlayer.Normalize();

            // Hitung target direction berdasarkan arah menjauh dari pemain
            targetDirection = directionToPlayer;

            // Rotasi tubuh ikan menghadap ke arah menjauh dari pemain
            body.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        }
        else
        {
            targetDirection = bait.forward; // Kembali ke arah umpan
        }

        float distanceTraveled = Vector3.Distance(startingPosition, transform.position);

        // Tentukan batas pergerakan sebelum ikan mencoba untuk membalik arah
        if (distanceTraveled >= movementLimit)
        {
            // Balik arah untuk perlawanan
            targetDirection = direction * -1;
            startingPosition = transform.position;
            isReversing = true;
        }
        else
        {
            isReversing = false;
        }

        // Mengubah arah pergerakan ikan menggunakan Slerp untuk membuat gerakan halus
        direction = Vector3.Slerp(direction, targetDirection, turnSpeed * Time.deltaTime);



        // Gerakan perlawanan di sepanjang arah horizontal dan zigzag
        transform.Translate(direction * speed * 2 * Time.deltaTime, Space.World);

        // Update rotasi tubuh ikan agar tetap sejajar dengan arah gerakan horizontal
        body.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }




    private void ReleaseFish()
    {
        if (isStickingToBait)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {

                if (isInCatchZone)
                {
                    playerController.Victory();

                    isStickingToBait = false;

                    if (inventoryController != null && fishInfo != null)
                    {
                        Debug.Log("Menambahkan ikan ke inventory.");
                        inventoryController.AddItem(Item.ItemType.Fish, fishInfo);
                        Debug.Log("Ikan berhasil ditambahkan ke inventory!");
                        Destroy(gameObject);
                        inventoryController.RemoveBaitItems();

                    }
                    else
                    {
                        Debug.LogWarning("InventoryController atau FishInfo tidak terpasang.");
                    }
                }
                else
                {
                    Debug.Log("Ikan lepas, belum di zona ambil!");
                    isStickingToBait = false;
                    isChasingBait = true;
                    startingPosition = transform.position;
                    direction = mouth.forward;

                    transform.Translate(Vector3.back * 2f, Space.World);
                }
            }
        }
    }





    private void AdjustMouthAndTail()
    {
        mouth.position = transform.position + direction * (transform.localScale.z / 2f);
        tail.position = transform.position - direction * (transform.localScale.z / 2f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] float distance = 5;
    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxVerticalAngle = 45;
    [SerializeField] Vector2 framingOffset;
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;
    [SerializeField] float zoomedFOV = 30f; // FOV saat zoom in
    [SerializeField] float defaultFOV = 60f; // FOV normal
    [SerializeField] float zoomSpeed = 2f; // Kecepatan zoom
    private Camera cameraComponent; // Komponen Camera
    [SerializeField] PlayerController playerController;
    private List<Fish> fishes; // Daftar untuk semua objek ikan
    private float rotationX;
    private float rotationY;
    private float invertXVal;
    private float invertYVal;
    private float timer = 0f; // Timer untuk bobbing
    private bool isZoomOutTriggered = false;

    private void Start()
    {
        Cursor.visible = false;
        CheckCursor();
        cameraComponent = GetComponent<Camera>(); // Ambil komponen Camera

        // Menemukan semua objek ikan dan menyimpannya dalam daftar
        fishes = new List<Fish>(FindObjectsOfType<Fish>());
    }

    private void Update()
    {
        invertXVal = (invertX) ? 1 : -1;
        invertYVal = (invertY) ? -1 : 1;

        // Mengatur rotasi kamera
        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;

        Focused();
    }

    private void Focused()
    {
        bool anyFishSticking = false;

        // Memeriksa apakah ada ikan yang terikat pada umpan
        foreach (Fish fish in fishes)
        {
            if (fish.isStickingToBait)
            {
                anyFishSticking = true;

                if (!isZoomOutTriggered)
                {
                    cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, zoomedFOV, Time.deltaTime * zoomSpeed);
                }

                // Efek bobbing
                float bobbingAmount = 0.1f; // Amplitudo bobbing
                float bobbingSpeed = 2f; // Kecepatan bobbing
                float newYPosition = Mathf.Sin(timer * bobbingSpeed) * bobbingAmount;
                transform.position += new Vector3(0, newYPosition, 0);
                timer += Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.H) && !isZoomOutTriggered)
                {
                    StartCoroutine(ZoomOutTemporarily());
                }

                break; // Berhenti jika menemukan satu ikan yang terikat
            }
        }

        if (!anyFishSticking)
        {
            cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
        }
    }

    private IEnumerator ZoomOutTemporarily()
    {
        isZoomOutTriggered = true;
        float targetFOV = defaultFOV + 0.1f; // FOV untuk zoom out, bisa diatur sesuai keinginan

        // Zoom out
        while (cameraComponent.fieldOfView < targetFOV)
        {
            cameraComponent.fieldOfView += zoomSpeed * Time.deltaTime * 30; // Kecepatan zoom out
            yield return null;
        }

        yield return new WaitForSeconds(0f); // Durasi zoom out

        // Zoom in kembali ke zoomedFOV
        while (cameraComponent.fieldOfView > zoomedFOV)
        {
            cameraComponent.fieldOfView -= zoomSpeed * Time.deltaTime * 30; // Kecepatan zoom in
            yield return null;
        }

        isZoomOutTriggered = false;
    }
    public void Casting()
    {
        StartCoroutine(CastingZoomEffect());
    }

    private IEnumerator CastingZoomEffect()
    {
        float targetZoomOutFOV = cameraComponent.fieldOfView + 10f; // Tambah FOV sebesar 10 untuk zoom out
        float zoomOutSmoothTime = 0.2f; // Durasi peredaman untuk zoom out agar lebih lambat
        float zoomInSmoothTime = 0.2f; // Durasi peredaman untuk zoom in agar sama dengan zoom out
        float zoomVelocity = 0f; // Kecepatan sementara untuk SmoothDamp

        // Zoom out dari 0 sampai 2.03 detik
        float elapsedTime = 0f;
        while (elapsedTime < 2.03f)
        {
            cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, targetZoomOutFOV, ref zoomVelocity, zoomOutSmoothTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cameraComponent.fieldOfView = targetZoomOutFOV; // Pastikan FOV tepat pada target setelah 2.03 detik

        // Tunggu sebentar sebelum mulai zoom in
        yield return new WaitForSeconds(0.1f); // Sesuaikan jika diperlukan

        // Zoom in kembali ke defaultFOV dengan transisi halus
        while (Mathf.Abs(cameraComponent.fieldOfView - defaultFOV) > 0.1f) // Berhenti saat hampir mencapai defaultFOV
        {
            cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, defaultFOV, ref zoomVelocity, zoomInSmoothTime);
            yield return null;
        }

        cameraComponent.fieldOfView = defaultFOV; // Pastikan FOV tepat kembali ke nilai default
    }





    public void CheckCursor()
    {
        if (Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
}

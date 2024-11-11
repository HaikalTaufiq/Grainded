using System.Collections;
using UnityEngine;

public class Oldman : MonoBehaviour
{
    public AudioClip npcSound;
    public float soundRadius = 5f;
    public float fadeSpeed = 1f;

    private AudioSource audioSource;
    private Transform playerController;
    public bool isPlayerInRange = false; // Status apakah pemain dalam radius

    private float specialStartTime = 39f; // Waktu mulai untuk bagian khusus
    private float specialPlayDuration = 2f; // Durasi bagian khusus

    void Start()
    {
        // Menambahkan AudioSource untuk memainkan suara
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = npcSound;
        audioSource.loop = false; // Nonaktifkan loop untuk pengendalian manual
        audioSource.volume = 0f;

        // Menambahkan SphereCollider sebagai trigger untuk mendeteksi pemain dalam radius
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = soundRadius;

        // Menemukan objek Player dalam scene
        playerController = GameObject.Find("Player").transform;
    }

    void Update()
    {
        // Memeriksa jarak antara pemain dan NPC untuk memperbarui status isPlayerInRange
        float distanceToPlayer = Vector3.Distance(transform.position, playerController.position);
        bool currentlyInRange = distanceToPlayer <= soundRadius;

        // Jika pemain memasuki jangkauan dan belum berada dalam jangkauan
        if (currentlyInRange && !isPlayerInRange)
        {
            isPlayerInRange = true;
            PlayNormalSound(); // Memainkan suara normal
        }
        // Jika pemain keluar jangkauan dan sebelumnya berada dalam jangkauan
        else if (!currentlyInRange && isPlayerInRange)
        {
            isPlayerInRange = false;
            PlaySpecialSound(); // Memainkan suara khusus
        }
    }

    // Memainkan suara normal saat pemain memasuki jangkauan
    void PlayNormalSound()
    {
        StopAllCoroutines(); // Hentikan fade-out atau suara khusus jika masih aktif
        audioSource.time = 0f; // Mulai dari detik 0
        audioSource.Play(); // Mainkan suara
        StartCoroutine(FadeInSound()); // Lakukan fade-in volume
    }

    // Memainkan bagian suara khusus (detik 39-41) saat pemain keluar dari jangkauan
    void PlaySpecialSound()
    {
        StopAllCoroutines(); // Hentikan fade-in atau suara normal jika masih aktif
        audioSource.Stop(); // Hentikan suara yang sedang diputar
        audioSource.time = specialStartTime; // Mulai dari detik 39
        audioSource.Play(); // Mainkan suara khusus
        StartCoroutine(FadeInSound()); // Lakukan fade-in volume

        // Jalankan coroutine untuk menghentikan suara setelah durasi khusus
        StartCoroutine(StopSoundAfterDuration(specialPlayDuration));
    }

    // Coroutine untuk melakukan fade-in volume suara
    IEnumerator FadeInSound()
    {
        audioSource.volume = 0f;
        while (audioSource.volume < 1f)
        {
            audioSource.volume += Time.deltaTime * fadeSpeed; // Menaikkan volume secara bertahap
            yield return null;
        }
        audioSource.volume = 1f; // Pastikan volume mencapai 1
    }

    // Menangani trigger saat pemain memasuki area jangkauan
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Pastikan objek yang memasuki trigger adalah pemain
        {
            isPlayerInRange = true;
            PlayNormalSound(); // Putar suara normal saat pemain masuk jangkauan
        }
    }

    // Menangani trigger saat pemain keluar dari area jangkauan
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  // Pastikan objek yang keluar trigger adalah pemain
        {
            isPlayerInRange = false;
            PlaySpecialSound(); // Putar suara khusus saat pemain keluar jangkauan
        }
    }

    // Coroutine untuk menghentikan suara setelah durasi tertentu
    IEnumerator StopSoundAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        audioSource.Stop(); // Hentikan suara setelah durasi selesai
    }

    // Menampilkan radius suara dengan Gizmos di editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // Menandai dengan warna hijau
        Gizmos.DrawWireSphere(transform.position, soundRadius); // Gambar sphere untuk menandakan area jangkauan suara
    }
}

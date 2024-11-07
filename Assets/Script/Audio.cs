using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public AudioSource splashAudioSource; // AudioSource untuk suara water splash
    public float detectionDistance = 1f; // Jarak deteksi dari bait ke permukaan air
    public Transform bait; // Referensi untuk objek bait

    private void Update()
    {
        // Melakukan raycast ke bawah untuk mendeteksi air dari posisi bait
        if (bait != null)
        {
            RaycastHit hit;
            // Menggunakan posisi bait untuk raycast
            if (Physics.Raycast(bait.position, Vector3.down, out hit, detectionDistance))
            {
                Debug.Log("Hit detected: " + hit.collider.gameObject.name); // Debug informasi hit
                // Cek apakah objek yang terdeteksi adalah air
                if (hit.collider.CompareTag("Water"))
                {
                    Debug.Log("Water detected!"); // Informasi jika air terdeteksi
                    // Mainkan suara splash jika tidak sedang diputar
                    if (!splashAudioSource.isPlaying)
                    {
                        splashAudioSource.Stop(); // Hentikan audio yang sedang diputar
                        splashAudioSource.time = 0; // Mulai dari awal audio
                        splashAudioSource.Play(); // Mainkan suara splash
                    }
                }
            }
        }
    }
}

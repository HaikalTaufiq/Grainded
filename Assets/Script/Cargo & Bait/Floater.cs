using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class Floater : MonoBehaviour
{
    public Transform pointJoint;
    private bool isAttachedToPoint = false;
    private PlayerController playerController;
    private Rigidbody Rigidbody; // Tambahkan referensi ke Rigidbody

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        Rigidbody = GetComponent<Rigidbody>(); // Dapatkan komponen Rigidbody
    }

    void Update()
    {
        if (playerController != null)
        {
            // Saat gameState bukan isWater, cargo akan menempel pada pointJoint
            if (playerController.gameState != GameState.isWater)
            {
                if (!isAttachedToPoint)
                {
                    SetIsAttachedToPoint(true);
                }
            }
            else
            {
                if (isAttachedToPoint)
                {
                    ResetCargoPosition(); // Reset cargo saat kembali ke isWater
                }
            }
        }

        // Jika cargo sedang menempel, ikuti posisi dan rotasi pointJoint
        if (isAttachedToPoint)
        {
            AttachToPointJoint();
        }
    }

    // Menempelkan cargo pada posisi pointJoint
    public void AttachToPointJoint()
    {
        if (pointJoint != null)
        {
            transform.position = pointJoint.position;
            transform.rotation = pointJoint.rotation;

            // Matikan efek fisika agar cargo menempel lebih baik
            if (Rigidbody != null)
            {
                Rigidbody.velocity = Vector3.zero; // Reset kecepatan
                Rigidbody.isKinematic = true;
                Rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    // Reset status dan posisi cargo
    public void ResetCargoPosition()
    {
        SetIsAttachedToPoint(false);

        // Kembalikan efek fisika untuk casting ulang
        if (Rigidbody != null)
        {
            Rigidbody.isKinematic = false;
        }
    }

    public void SetIsAttachedToPoint(bool isAttached)
    {
        isAttachedToPoint = isAttached;
    }
}

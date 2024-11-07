using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public Rigidbody rb;
    public BoxCollider coll;
    public Transform player, Container, fpsCam;

    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;

    public bool equipped;
    [SerializeField] private PlayerController playerController;

    public BaitInfo baitInfo;
    void Start()
    {
        if (!equipped)
        {
            rb.isKinematic = false;
            coll.isTrigger = false;
        }
        else
        {
            rb.isKinematic = true;
            coll.isTrigger = true;
        }
    }


    void Update()
    {
        Vector3 distanceToPlayer = player.position - transform.position;

        if (!equipped && distanceToPlayer.magnitude <= pickUpRange && Input.GetKeyDown(KeyCode.F))
        {
            PickingUp();
        }

    }

    void FixedUpdate()
    {
        if (equipped)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }


    private void PickingUp()
    {
        equipped = true;

        // Set objek sebagai anak dari gunContainer agar mengikuti pemain
        transform.SetParent(Container);

        // Reset posisi, rotasi, dan skala
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;

        // Nonaktifkan fisika saat diambil
        rb.isKinematic = true;
        coll.isTrigger = true;

        if (InventoryController.Instance != null && baitInfo != null)
        {
            InventoryController.Instance.AddItem(Item.ItemType.Bait, baitInfo);
        }
    }


}
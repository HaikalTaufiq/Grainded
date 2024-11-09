using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equip : MonoBehaviour
{
    // Referensi ke kedua objek yang akan diaktifkan atau dinonaktifkan
    public GameObject object1;
    public GameObject object2;

    // Start is called before the first frame update
    void Start()
    {
        // Pastikan pada awalnya hanya object1 yang aktif
        if (object1 != null && object2 != null)
        {
            object1.SetActive(true);
            object2.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Memeriksa input keyboard dan mengatur objek yang aktif
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Jika angka 1 ditekan
        {
            EquipObject(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Jika angka 2 ditekan
        {
            EquipObject(2);
        }
    }

    // Fungsi untuk mengaktifkan objek berdasarkan nomor
    void EquipObject(int objectNumber)
    {
        if (objectNumber == 1)
        {
            if (object1 != null && object2 != null)
            {
                object1.SetActive(true);  // Mengaktifkan object1
                object2.SetActive(false); // Menonaktifkan object2
            }
        }
        else if (objectNumber == 2)
        {
            if (object1 != null && object2 != null)
            {
                object1.SetActive(false); // Menonaktifkan object1
                object2.SetActive(true);  // Mengaktifkan object2
            }
        }
    }
}

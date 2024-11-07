using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;

    private bool isActive = false;

    void Start()
    {
        shopPanel.SetActive(false);
    }

    public bool ShowShopPanel()
    {

        isActive = !isActive;
        shopPanel.SetActive(isActive);

        return isActive;
    }
}

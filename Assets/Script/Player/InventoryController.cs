using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private List<Item> items = new List<Item>();

    public static InventoryController Instance { get; private set; }

    public event Action<Item> OnItemAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        var all = Resources.LoadAll<FishingRodInfo>("Collections/FishingRod");

        foreach (var item in all)
        {
            AddItem(Item.ItemType.FishingRod, item);
        }

    }


    public void AddItem(Item.ItemType type, object item)
    {
        var newItem = new Item(type, item, 100);
        items.Add(newItem);
        OnItemAdded?.Invoke(newItem);
    }


    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public List<Item> GetItem()
    {
        return items;
    }

    public void RemoveBaitItems()
    {
        // Mencari item bait pertama dalam daftar items
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetItemType() == Item.ItemType.Bait)
            {
                // Menghapus item bait dari inventori
                RemoveItem(items[i]);
                break; // Keluar dari loop setelah menghapus satu bait
            }
        }
    }


}

[System.Serializable]
public class Item
{
    public enum ItemType { FishingRod, Fish, Bait }
    private ItemType type;
    private object item;
    private float durability;

    public Item(ItemType type, object item, float durability)
    {
        this.item = item;
        this.durability = durability;
        this.type = type;
    }

    public float GetDurability()
    {
        return durability;
    }

    public object GetItem()
    {
        return item;
    }

    public ItemType GetItemType()
    {
        return type;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parentTransform;

    private InventoryController inventoryController = null;

    [SerializeField] private List<GameObject> gameObjects = new List<GameObject>();
    private bool isActive = false;

    private void Start()
    {
        inventoryPanel.SetActive(false);

        if (InventoryController.Instance != null)
        {
            inventoryController = InventoryController.Instance;
            inventoryController.OnItemAdded += HandleItemAdded;
        }
    }

    private void OnDestroy()
    {
        if (inventoryController != null)
        {
            inventoryController.OnItemAdded -= HandleItemAdded;
        }
    }


    public bool ShowInventoryPanel(InventoryController inventory)
    {
        if (inventoryController == null)
            inventoryController = inventory;
        isActive = !isActive;
        inventoryPanel.SetActive(isActive);
        ShowItem(inventoryController.GetItem());
        return isActive;
    }


    private void ShowItem(List<Item> items)
    {
        if (isActive)
        {
            foreach (var item in items)
            {
                switch (item.GetItemType())
                {
                    case Item.ItemType.FishingRod:
                        SetItem(
                            item,
                            ((FishingRodInfo)item.GetItem()).image,
                            ((FishingRodInfo)item.GetItem()).Name,
                            "Weight: " + ((FishingRodInfo)item.GetItem()).maxWeight + "Kg",
                            "Length: " + ((FishingRodInfo)item.GetItem()).length + "m"
                        );
                        break;

                    case Item.ItemType.Fish:
                        SetItem(
                            item,
                            ((FishInfo)item.GetItem()).image,
                            ((FishInfo)item.GetItem()).Name,
                            "Weight: " + ((FishInfo)item.GetItem()).Maxweight + "Kg",
                            "Length: " + ((FishInfo)item.GetItem()).length + "m"
                        );
                        break;

                    case Item.ItemType.Bait:
                        SetItem(
                            item,
                            ((BaitInfo)item.GetItem()).image,
                            ((BaitInfo)item.GetItem()).Name,
                            "Weight: " + ((BaitInfo)item.GetItem()).Maxweight + "Kg",
                            "Length: " + ((BaitInfo)item.GetItem()).length + "m"
                        );
                        break;

                    default:
                        break;
                }
            }
        }
        else
        {
            foreach (var item in gameObjects)
            {
                Destroy(item);
            }

            gameObjects = new List<GameObject>();
        }
    }

    private void SetItem(Item item, Sprite sprite, string textName, string textDetail, string textDetail1)
    {
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(parentTransform, false);
        go.SetActive(true);
        go.GetComponentsInChildren<Image>()[1].sprite = sprite;

        Text[] textInfo = go.GetComponentsInChildren<Text>();
        textInfo[0].text = textName;
        textInfo[1].text = textDetail;
        textInfo[2].text = textDetail1;

        go.GetComponentsInChildren<Button>()[0].onClick.AddListener(() =>
        {
            inventoryController.RemoveItem(item);
            Destroy(go);
            gameObjects.Remove(go);
        });

        gameObjects.Add(go);
    }

    private void HandleItemAdded(Item newItem)
    {
        if (isActive)
        {
            switch (newItem.GetItemType())
            {
                case Item.ItemType.FishingRod:
                    SetItem(
                        newItem,
                        ((FishingRodInfo)newItem.GetItem()).image,
                        ((FishingRodInfo)newItem.GetItem()).Name,
                        "Weight: " + ((FishingRodInfo)newItem.GetItem()).maxWeight + "Kg",
                        "Length: " + ((FishingRodInfo)newItem.GetItem()).length + "m"
                    );
                    break;

                case Item.ItemType.Fish:
                    SetItem(
                        newItem,
                        ((FishInfo)newItem.GetItem()).image,
                        ((FishInfo)newItem.GetItem()).Name,
                        "Weight: " + ((FishInfo)newItem.GetItem()).Maxweight + "Kg",
                        "Length: " + ((FishInfo)newItem.GetItem()).length + "m"
                    );
                    break;

                case Item.ItemType.Bait:
                    SetItem(
                      newItem,
                      ((BaitInfo)newItem.GetItem()).image,
                      ((BaitInfo)newItem.GetItem()).Name,
                      "Weight: " + ((BaitInfo)newItem.GetItem()).Maxweight + "Kg",
                      "Length: " + ((BaitInfo)newItem.GetItem()).length + "m"
                  );
                    break;

                default:
                    break;
            }
        }
    }
}

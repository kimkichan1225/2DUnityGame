using UnityEngine;
using UnityEngine.UI;

public class InventorySlots : MonoBehaviour
{
    public GameObject slotPrefab; // The prefab for a single inventory slot
    public Transform slotContainer; // The parent object that will hold the slots
    public int numberOfSlots = 20; // How many slots to create

    void Start()
    {
        CreateSlots();
    }

    void CreateSlots()
    {
        if (slotPrefab == null || slotContainer == null)
        {
            Debug.LogError("Slot Prefab or Slot Container not assigned in the inspector!");
            return;
        }

        // Clear existing slots if any, in case of re-creation
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        // Create the specified number of slots
        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.name = "ItemSlot_" + i;
        }
    }
}

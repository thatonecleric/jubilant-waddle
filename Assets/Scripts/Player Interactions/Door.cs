using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpen = false;
    public bool Interact(Interactor interactor)
    {
        var inventory = interactor.GetComponent<Inventory>();

        if (inventory == null) return false;
        if (isOpen) return false;

        if (inventory.HasKey)
        {
            Debug.Log("Opening door");
            isOpen = true;
            inventory.HasKey = false;
            return true;
        }
        
        Debug.Log("You need a key");
        return false;
    }

    public GameObject GetGameObject()
    {
           return gameObject;
    }
}

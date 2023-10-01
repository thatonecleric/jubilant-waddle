using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    public bool Interact(Interactor interactor)
    {
        var inventory = interactor.GetComponent<Inventory>();

        if (inventory == null) return false;
        if (inventory.HasKey) return false;

        inventory.HasKey = true;

        Debug.Log("Taking key");
        return true;
    }
    
    public GameObject GetGameObject()
    {
           return gameObject;
    }
}

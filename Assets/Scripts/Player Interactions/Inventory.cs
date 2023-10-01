using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] public Image KeyIcon;
    public bool HasKey = false;

    public void Update()
    {
        if (Input.GetKey(KeyCode.Q)) HasKey = false; // Reset key for testing
    }
}
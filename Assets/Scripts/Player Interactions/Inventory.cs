using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public bool HasKey = false;

    public void Update()
    {
        if (Input.GetKey(KeyCode.Q)) HasKey = false; // Reset key for testing
    }
}

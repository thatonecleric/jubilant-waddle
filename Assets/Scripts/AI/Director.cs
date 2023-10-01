using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour
{
    // Singleton
    public static Director instance = null;
    
    // Player Stats


    private void Start()
    {
        instance = this;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public bool isFlashlightOn = false;

    private Light flashlight;

    void Start()
    {
        flashlight = GetComponent<Light>();    
    }

    void Update()
    {
        ProcessInput();
    }

    void ProcessInput()
    {
        ToggleFlashlight();
    }

    void ToggleFlashlight()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFlashlightOn = !isFlashlightOn;
            flashlight.enabled = isFlashlightOn;
        }
    }
}

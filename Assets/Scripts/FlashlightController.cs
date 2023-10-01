using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController instance = null;
    public bool isFlashlightOn = false;
    private Light flashlight;

    public float batteryDrainAmount = 0.2f;
    public float remainingBattery = 10f;

    void Start()
    {
        flashlight = GetComponent<Light>();
    }

    void Update()
    {
        ProcessInput();

        if (isFlashlightOn)
            DrainBattery();
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

    void DrainBattery()
    {
        remainingBattery -= batteryDrainAmount * Time.deltaTime;
    }
}

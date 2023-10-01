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

    public AudioSource flashlightOnSFX;
    public AudioSource flashlightOffSFX;

    void Start()
    {
        instance = this;
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

            (isFlashlightOn ? flashlightOnSFX : flashlightOffSFX).Play();
        }
    }

    void DrainBattery()
    {
        remainingBattery -= batteryDrainAmount * Time.deltaTime;
    }
}

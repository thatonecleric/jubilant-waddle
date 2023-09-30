using UnityEngine;
using UnityEngine.UI;

public enum SliderAssignment {
    BGM,
    SFX,
    MouseSens
}

public class UpdateSettingsSlider : MonoBehaviour
{
    public SliderAssignment assignment;
    public Slider slider;

    public void OnValueChanged()
    {
        switch(assignment)
        {
            case SliderAssignment.BGM:
                Settings.BGMVolume = slider.value;
                break;
            case SliderAssignment.SFX:
                Settings.SFXVolume = slider.value;
                break;
            case SliderAssignment.MouseSens:
                Settings.MouseSensitivity = (int)slider.value;
                break;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ValueImporter : MonoBehaviour
{
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider mouseSensitivitySlider;

    private void Start()
    {
        // Set default min/max values.
        bgmVolumeSlider.minValue = Settings.BGMVolumeMin;
        sfxVolumeSlider.minValue = Settings.SFXVolumeMin;
        mouseSensitivitySlider.minValue = Settings.MouseSensitivityMin;

        bgmVolumeSlider.maxValue = Settings.BGMVolumeMax;
        sfxVolumeSlider.maxValue = Settings.SFXVolumeMax;
        mouseSensitivitySlider.maxValue = Settings.MouseSensitivityMax;

        // Set default values.
        bgmVolumeSlider.value = Settings.BGMVolume;
        sfxVolumeSlider.value = Settings.SFXVolume;
        mouseSensitivitySlider.value = Settings.MouseSensitivity;
    }
}

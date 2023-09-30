using UnityEngine;

public class Settings : MonoBehaviour
{
    private static float _BGMVolumeMin = 0f;
    private static float _BGMVolumeMax = 1f;

    private static float _SFXVolumeMin = 0f;
    private static float _SFXVolumeMax = 1f;

    private static int _MouseSensitivityMin = 0;
    private static int _MouseSensitivityMax = 1000;

    private static float _BGMVolume = 0.5f;
    private static float _SFXVolume = 0.5f;
    private static int _MouseSensitivity = 400;

    public static float BGMVolumeMin { get { return _BGMVolumeMin; } private set { _BGMVolumeMin = value; } }
    public static float BGMVolumeMax { get { return _BGMVolumeMax; } private set { _BGMVolumeMax = value; } }
    public static float SFXVolumeMin { get { return _SFXVolumeMin; } private set { _SFXVolumeMin = value; } }
    public static float SFXVolumeMax { get { return _SFXVolumeMax; } private set { _SFXVolumeMax = value; } }
    public static int MouseSensitivityMin { get { return _MouseSensitivityMin; } private set { _MouseSensitivityMin = value; } }
    public static int MouseSensitivityMax { get { return _MouseSensitivityMax; } private set { _MouseSensitivityMax = value; } }

    public static float BGMVolume { get { return _BGMVolume; } set { _BGMVolume = Mathf.Clamp(value, BGMVolumeMin, BGMVolumeMax); } }
    public static float SFXVolume { get { return _SFXVolume; } set { _SFXVolume = Mathf.Clamp(value, SFXVolumeMin, SFXVolumeMax); } }
    public static int MouseSensitivity { get { return _MouseSensitivity; } set { _MouseSensitivity = Mathf.Clamp(value, MouseSensitivityMin, MouseSensitivityMax); } }

    public static Settings Instance = null;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

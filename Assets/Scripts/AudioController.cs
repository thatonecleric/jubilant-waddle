using System.Collections;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance = null;

    public GameObject bgmAmbienceParent;

    private AudioSource currentAmbientMusic;
    private Coroutine ambientMusicFade = null;

    public GameObject sfxWaterWalkingParent;
    public GameObject sfxWaterRunningParent;

    public GameObject sfxStoneWalkingParent;
    public GameObject sfxStoneRunningParent;

    private AudioSource[] bgmAmbience;

    private AudioSource[] playerStoneWalkingSources;
    private AudioSource[] playerWaterWalkingSources;

    private AudioSource[] playerStoneRunningSources;
    private AudioSource[] playerWaterRunningSources;

    private bool isStoneWalkingAudioPlaying = false;
    private Coroutine stoneWalkingAudioFade = null;

    private bool isWaterWalkingAudioPlaying = false;
    private Coroutine waterWalkingAudioFade = null;


    private bool isStoneRunningAudioPlaying = false;
    private Coroutine stoneRunningAudioFade = null;

    private bool isWaterRunningAudioPlaying = false;
    private Coroutine waterRunningAudioFade = null;

    private void Start()
    {
        instance = this;

        // Get all audio sources from parent gameobjects.
        bgmAmbience = bgmAmbienceParent.GetComponents<AudioSource>();

        playerWaterWalkingSources = sfxWaterWalkingParent.GetComponents<AudioSource>();
        playerWaterRunningSources = sfxWaterRunningParent.GetComponents<AudioSource>();

        playerStoneWalkingSources = sfxStoneWalkingParent.GetComponents<AudioSource>();
        playerStoneRunningSources = sfxStoneRunningParent.GetComponents<AudioSource>();

        currentAmbientMusic = PlayRandomSound(bgmAmbience, Settings.BGMVolume);
    }

    void Update()
    {
        MaybePlayPlayerMovementSound();
    }

    void MaybePlayPlayerMovementSound()
    {
        Vector3 playerVelocity = PlayerController.instance.GetPlayerVelocity();
        bool isPlayerMoving = Mathf.Abs(playerVelocity.x) > 0.01f || Mathf.Abs(playerVelocity.z) > 0.01f;
        bool isPlayerSprinting = PlayerController.instance.isPlayerSprinting;
        bool isPlayerInWater = PlayerController.instance.isPlayerInWater;

        if (isPlayerMoving && !isPlayerSprinting && isPlayerInWater && !isWaterWalkingAudioPlaying)
        {
            AudioSource waterWalkingClip = PlayRandomSound(playerWaterWalkingSources, 0f);
            isWaterWalkingAudioPlaying = true;
            if (waterWalkingAudioFade == null)
                waterWalkingAudioFade = StartCoroutine(FadeWaterWalkingAudioIn(waterWalkingClip, 1f));
        }
        else if (isPlayerMoving && isPlayerSprinting && isPlayerInWater && !isWaterRunningAudioPlaying)
        {
            AudioSource waterRunningClip = PlayRandomSound(playerWaterRunningSources, 0f);
            isWaterRunningAudioPlaying = true;
            if (waterRunningAudioFade == null)
                waterRunningAudioFade = StartCoroutine(FadeWaterRunningAudioIn(waterRunningClip, 1f));
        }
        else if (isPlayerMoving && !isPlayerSprinting && !isPlayerInWater && !isStoneWalkingAudioPlaying)
        {
            AudioSource walkingClip = PlayRandomSound(playerStoneWalkingSources, 0f);
            isStoneWalkingAudioPlaying = true;
            if (stoneWalkingAudioFade == null)
                stoneWalkingAudioFade = StartCoroutine(FadeWalkingAudioIn(walkingClip, 1f));

        }
        else if (isPlayerMoving && isPlayerSprinting && !isStoneRunningAudioPlaying)
        {
            AudioSource runningClip = PlayRandomSound(playerStoneRunningSources, 0f);
            isStoneRunningAudioPlaying = true;
            if (stoneRunningAudioFade == null)
                stoneRunningAudioFade = StartCoroutine(FadeRunningAudioIn(runningClip, 1f));
        }

        // Stop stone walking sounds immediately if player is no longer on stone.
        if (isPlayerInWater && isStoneWalkingAudioPlaying)
        {
            foreach (AudioSource audio in playerStoneWalkingSources)
            {
                if (audio.isPlaying) audio.Stop();
                isStoneWalkingAudioPlaying = false;
            }
        }

        // Stop water walking sounds immediately if player is no longer in water.
        if (!isPlayerInWater && isWaterWalkingAudioPlaying)
        {
            foreach (AudioSource audio in playerWaterWalkingSources)
            {
                if (audio.isPlaying) audio.Stop();
                isWaterWalkingAudioPlaying = false;
            }
        }
        
        // Stop stone running sounds immediately if the player is no longer on stone.
        if (isPlayerInWater && isStoneRunningAudioPlaying)
        {
            foreach (AudioSource audio in playerStoneRunningSources)
            {
                if (audio.isPlaying) audio.Stop();
                isStoneRunningAudioPlaying = false;
            }
        }

        // Stop water running sounds immediately if the player is no longer in water.
        if (!isPlayerInWater && isWaterRunningAudioPlaying)
        {
            foreach (AudioSource audio in playerWaterRunningSources)
            {
                if (audio.isPlaying) audio.Stop();
                isWaterRunningAudioPlaying = false;
            }
        }

        if (!isPlayerMoving)
        {
            if (isStoneWalkingAudioPlaying)
            {
                foreach (AudioSource audio in playerStoneWalkingSources)
                {
                    if (audio.isPlaying) audio.Stop();
                    isStoneWalkingAudioPlaying = false;
                }
            }

            if (isWaterWalkingAudioPlaying)
            {
                foreach (AudioSource audio in playerWaterWalkingSources)
                {
                    if (audio.isPlaying) audio.Stop();
                    isWaterWalkingAudioPlaying = false;
                }
            }
        }

        if (!isPlayerSprinting && isWaterRunningAudioPlaying)
        {
            foreach (AudioSource audio in playerWaterRunningSources)
            {
                if (audio.isPlaying) audio.Stop();
                isWaterRunningAudioPlaying = false;
            }
        }

        if (!isPlayerSprinting && isStoneRunningAudioPlaying)
        {
            foreach (AudioSource audio in playerStoneRunningSources)
            {
                if (audio.isPlaying) audio.Stop();
                isStoneRunningAudioPlaying = false;
            }
        }
    }

    AudioSource PlayRandomSound(AudioSource[] variations, float startingVolume)
    {
        if (variations.Length == 0) return null;
        int variantIdx = Random.Range(0, variations.Length - 1);
        variations[variantIdx].volume = startingVolume;
        variations[variantIdx].Play();
        return variations[variantIdx];
    }

    public void RequestAmbienceStop(float fadeOutTime = 0f)
    {
        // Execute any additional logic needed here before stopping the current BGM.
        // Note: This could get called multiple times.

        // Fade out music.
        if (ambientMusicFade == null)
            ambientMusicFade = StartCoroutine(FadeOutAmbientMusic(currentAmbientMusic, fadeOutTime));
    }

    IEnumerator FadeWalkingAudioIn(AudioSource audio, float totalFadeTime)
    {
        float t = 0;
        while (t < Settings.SFXVolume)
        {
            t += Time.deltaTime;
            audio.volume += (Time.deltaTime / totalFadeTime);
            yield return null;
        }
        stoneWalkingAudioFade = null;
    }

    IEnumerator FadeWaterWalkingAudioIn(AudioSource audio, float totalFadeTime)
    {
        float t = 0;
        while (t < Settings.SFXVolume)
        {
            t += Time.deltaTime;
            audio.volume += (Time.deltaTime / totalFadeTime);
            yield return null;
        }
        waterWalkingAudioFade = null;
    }

    IEnumerator FadeRunningAudioIn(AudioSource audio, float totalFadeTime)
    {
        float t = 0;
        while (t < Settings.SFXVolume)
        {
            t += Time.deltaTime;
            audio.volume += (Time.deltaTime / totalFadeTime);
            yield return null;
        }
        stoneRunningAudioFade = null;
    }

    IEnumerator FadeWaterRunningAudioIn(AudioSource audio, float totalFadeTime)
    {
        float t = 0;
        while (t < Settings.SFXVolume)
        {
            t += Time.deltaTime;
            audio.volume += (Time.deltaTime / totalFadeTime);
            yield return null;
        }
        waterRunningAudioFade = null;
    }

    IEnumerator FadeOutAmbientMusic(AudioSource audio, float totalFadeTime)
    {
        if (totalFadeTime == 0)
        {
            audio.volume = 0f;
        }
        else
        {
            float t = 0;
            while (t < Settings.BGMVolume)
            {
                t += Time.deltaTime;
                audio.volume -= (Time.deltaTime / totalFadeTime);
                yield return null;
            }
        }
        audio.Stop();
        ambientMusicFade = null;
    }
}

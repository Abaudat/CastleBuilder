using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip buttonClickClip, cancelClip, selectClip, buildClip, destroyClip, rotateClip, linkClip, unlinkClip, electricitySelectClip, electricityUnselectClip, layerUpSound, layerDownSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        CubeGrid.ElementReplaced += ElementReplacedHandler;
        CubeGrid.ElementRotated += ElementRotatedHandler;
        CubeGrid.ElementConsumerAdded += ElementConsumerAddedHandler;
        CubeGrid.ElementConsumerRemoved += ElementConsumerRemovedHandler;
        EditLayerManager.LayerChanged += LayerChangedHandler;
        CubeGridEditor.SelectEditModeStarted += SelectEditModeStartedHandler;
        CubeGridEditor.SignalEditModeStarted += SelectEditModeStartedHandler;
    }

    private void ElementReplacedHandler(object sender, CubeGrid.ElementEventArgs elementEventArgs)
    {
        if (elementEventArgs.newElement.prefabIndex == 0)
        {
            PlayDestroySound();
        }
        else
        {
            PlayBuildSound();
        }
    }

    private void ElementRotatedHandler(object sender, CubeGrid.ElementEventArgs elementEventArgs)
    {
        PlayRotateSound();
    }

    private void ElementConsumerAddedHandler(object sender, CubeGrid.ElementConsumerModifiedEventArgs elementConsumerModifiedEventArgs)
    {
        PlayElectricityLinkSound();
    }

    private void ElementConsumerRemovedHandler(object sender, CubeGrid.ElementConsumerModifiedEventArgs elementConsumerModifiedEventArgs)
    {
        PlayElectricityUnlinkSound();
    }

    private void LayerChangedHandler(object sender, EditLayerManager.LayerChangedEventArgs layerChangedEventArgs)
    {
        if (layerChangedEventArgs.newHeight > layerChangedEventArgs.oldHeight)
        {
            PlayLayerUpSound();
        }
        else
        {
            PlayLayerDownSound();
        }
    }

    private void SelectEditModeStartedHandler(object sender, EventArgs eventArgs)
    {
        PlaySelectClip();
    }

    public void PlayButtonClickClip()
    {
        Play(buttonClickClip);
    }

    public void PlayCancelClip()
    {
        Play(cancelClip);
    }

    public void PlaySelectClip()
    {
        Play(selectClip);
    }

    public void PlayBuildSound()
    {
        Play(buildClip);
    }

    public void PlayDestroySound()
    {
        Play(destroyClip);
    }

    public void PlayRotateSound()
    {
        Play(rotateClip);
    }

    public void PlayElectricityLinkSound()
    {
        Play(linkClip);
    }

    public void PlayElectricityUnlinkSound()
    {
        Play(unlinkClip);
    }

    public void PlayElectricitySelectSound()
    {
        Play(electricitySelectClip);
    }

    public void PlayElectricityUnselectSound()
    {
        Play(electricityUnselectClip);
    }

    public void PlayLayerUpSound()
    {
        Play(layerUpSound);
    }

    public void PlayLayerDownSound()
    {
        Play(layerDownSound);
    }

    private void Play(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}

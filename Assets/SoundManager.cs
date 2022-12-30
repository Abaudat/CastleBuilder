using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip buttonClickClip, cancelClip, selectClip, buildClip, destroyClip, rotateClip, linkClip, unlinkClip, electricitySelectClip, electricityUnselectClip, layerUpSound, layerDownSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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

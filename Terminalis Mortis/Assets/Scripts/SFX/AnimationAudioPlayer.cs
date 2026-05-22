using UnityEngine;
using UnityEngine.Audio;
public class AnimationAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void PlayClip(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}

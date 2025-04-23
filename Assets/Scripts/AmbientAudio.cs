using System.Collections;
using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script plays ambient audio clips randomly on a loop
 */

public class AmbientAudio : MonoBehaviour
{
    [SerializeField]
    private AudioSource ambientAudio;

    [SerializeField]
    private AudioClip[] ambientClips;

    [SerializeField]
    private float delay = 3f;

    void Start()
    {
        StartCoroutine(PlayAudio(5)); // Start with a random delay before the first audio clip
    }

    /*
     * A recursive coroutine handling the audio loop.
     */
    private IEnumerator PlayAudio(float audioLength)
    {
        yield return new WaitForSeconds(audioLength + Random.value * audioLength * delay);
        ambientAudio.clip = ambientClips[Random.Range(0, ambientClips.Length)];
        ambientAudio.Play();
        StartCoroutine(PlayAudio(ambientAudio.clip.length));
    }
}

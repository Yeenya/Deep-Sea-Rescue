using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudio : MonoBehaviour
{
    [SerializeField]
    private AudioSource ambientAudio;

    [SerializeField]
    private AudioClip[] ambientClips;

    private float delay = 3f;

    void Start()
    {
        StartCoroutine(PlayAudio(5));
    }

    private IEnumerator PlayAudio(float audioLength)
    {
        yield return new WaitForSeconds(audioLength + Random.value * audioLength * delay);
        ambientAudio.clip = ambientClips[Random.Range(0, ambientClips.Length)];
        ambientAudio.Play();
        StartCoroutine(PlayAudio(ambientAudio.clip.length));
    }
}

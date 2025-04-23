using DG.Tweening;
using System.Collections;
using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the diver behavior.
 */
public class Diver : MonoBehaviour
{
    AudioSource sonarSound;
    private GameObject player;
    private CameraController cameraController;

    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateSound();
    }

    /*
     * Update the sound pitch based on the distance to the player.
     */
    public void UpdateSound()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // If the player is outside, no sound is emitted.
        if (distance <= sonarSound.maxDistance && cameraController.GetInsideOrOutside()) sonarSound.volume = 1;
        else sonarSound.volume = 0;

        // Modify the pitch based on the distance to the player combined with easeInSine function used as easing.
        float nonModifiedPitch = (sonarSound.maxDistance - distance) / sonarSound.maxDistance;
        sonarSound.pitch = (1 - Mathf.Cos(nonModifiedPitch * Mathf.PI / 2)) * 2 + 1; //easeInSine from https://easings.net/#easeInSine
    }

    /*
     * Setup the sound and get correct references
     */
    public void Init()
    {
        sonarSound = GetComponent<AudioSource>();
        sonarSound.Play();
        player = GameObject.FindGameObjectWithTag("Player");
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    /*
     * If diver is in range of the submarine, play running animation (which looks like jumping) and smoothly move to the submarine.
     */
    public void GetSaved(Vector3 submarinePosition)
    {
        GetComponent<Animator>().SetBool("isRunning", true);
        transform.DOMove(submarinePosition, 1);
        StartCoroutine(GetSavedCoroutine());
    }

    /*
     * Coroutine to destroy the diver after 1 second delay.
     */
    private IEnumerator GetSavedCoroutine()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}

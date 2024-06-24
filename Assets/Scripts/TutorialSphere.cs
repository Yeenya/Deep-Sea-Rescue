using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSphere : MonoBehaviour
{
    private GameObject player;
    private AudioSource sonarSound;
    private CameraController cameraController;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sonarSound = GetComponent<AudioSource>();
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= sonarSound.maxDistance && cameraController.GetInsideOrOutside()) sonarSound.volume = 1;
        else sonarSound.volume = 0;
        float nonModifiedPitch = (sonarSound.maxDistance - distance) / sonarSound.maxDistance;
        sonarSound.pitch = (1 - Mathf.Cos(nonModifiedPitch * Mathf.PI / 2)) * 2 + 1; //easeInSine from https://easings.net/#easeInSine
    }
}

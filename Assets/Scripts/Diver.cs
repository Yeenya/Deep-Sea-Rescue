using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diver : MonoBehaviour
{
    AudioSource sonarSound;
    private GameObject player;

    void Start()
    {
        sonarSound = GetComponent<AudioSource>();
        sonarSound.Play();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > sonarSound.maxDistance) sonarSound.volume = 0;
        else sonarSound.volume = 1;
        float nonModifiedPitch = (sonarSound.maxDistance - distance) / sonarSound.maxDistance;
        sonarSound.pitch = Mathf.Pow(2, 10 * nonModifiedPitch - 10) * 2 + 1;
    }
}

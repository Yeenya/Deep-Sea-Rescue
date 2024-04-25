using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diver : MonoBehaviour
{
    AudioSource sonarSound;
    private GameObject player;
    private CameraController cameraController;

    void Start()
    {
        sonarSound = GetComponent<AudioSource>();
        sonarSound.Play();
        player = GameObject.FindGameObjectWithTag("Player");
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (player.GetComponent<Player>().state != Player.State.TUTORIAL)
        {
            if (distance <= sonarSound.maxDistance && cameraController.GetInsideOrOutside()) sonarSound.volume = 1;
            else sonarSound.volume = 0;
        }
        float nonModifiedPitch = (sonarSound.maxDistance - distance) / sonarSound.maxDistance;
        sonarSound.pitch = (1 - Mathf.Cos(nonModifiedPitch * Mathf.PI / 2)) * 2 + 1; //easeInSine from https://easings.net/#easeInSine
    }

    public void GetSaved(Vector3 submarinePosition)
    {
        GetComponent<Animator>().SetBool("isRunning", true);
        transform.DOMove(submarinePosition, 1);
        StartCoroutine(GetSavedCoroutine());
    }

    private IEnumerator GetSavedCoroutine()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}

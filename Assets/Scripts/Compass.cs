using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    private Transform needlePoint;
    private GameObject player;

    void Start()
    {
        needlePoint = transform.GetChild(0);
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Keep the needle aiming to the north
        needlePoint.transform.localRotation = Quaternion.Euler(0, -player.transform.rotation.eulerAngles.y, 0);
    }
}

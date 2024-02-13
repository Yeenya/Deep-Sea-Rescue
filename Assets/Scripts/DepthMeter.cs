using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthMeter : MonoBehaviour
{
    private Material depthMaterial;
    private GameObject player;
    private readonly float depthInterval = 150f;
    private Color upperBoundColor;

    void Start()
    {
        depthMaterial = GetComponent<MeshRenderer>().material;
        upperBoundColor = depthMaterial.color;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        float currentDepth = Mathf.Clamp(player.transform.position.y, 0, depthInterval);
        float clampedDepth = currentDepth / depthInterval * 0.8f - 0.4f;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, clampedDepth);

        Color currentColor = depthMaterial.color;
        //Color.RGBToHSV(currentColor, out float currentHue, out float currentSaturation, out float currentValue);
        //depthMaterial.color = Color.HSVToRGB(currentHue, currentSaturation, currentDepth / depthInterval);
        currentColor.r = (1 - currentDepth / depthInterval);
        currentColor.g = upperBoundColor.g * (currentDepth / depthInterval);
        currentColor.b = upperBoundColor.b * (currentDepth / depthInterval);
        depthMaterial.color = currentColor;
        print(currentDepth / depthInterval);
    }
}

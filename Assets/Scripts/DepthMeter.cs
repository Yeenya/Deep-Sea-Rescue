using System.Collections;
using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the depth meter bar which works in a similar way to the electricity bar.
 */
public class DepthMeter : MonoBehaviour
{
    private Material depthMaterial;
    private GameObject player;
    private readonly float depthInterval = 180f;
    private Color upperBoundColor;
    private bool emissionOn = false;

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
        if (currentDepth <= 150)
        {
            currentColor.r = (1 - currentDepth / (depthInterval - 30));
            currentColor.g = upperBoundColor.g * (currentDepth / (depthInterval - 30));
            currentColor.b = upperBoundColor.b * (currentDepth / (depthInterval - 30));
        }
        else
        {
            currentColor.r = (150 - currentDepth) / (150 - depthInterval);
            currentColor.g = upperBoundColor.g * (1 - ((150 - currentDepth) / (150 - depthInterval)));
            currentColor.b = upperBoundColor.b * (1 - ((150 - currentDepth) / (150 - depthInterval)));
        }
        depthMaterial.color = currentColor;

        // Flash if the player gets too high (where nothing is)
        if (currentDepth > 170)
        {
            if (!GetComponent<AudioSource>().isPlaying && !player.GetComponent<Player>().gameOver) GetComponent<AudioSource>().Play();
            if (!emissionOn)
            {
                depthMaterial.EnableKeyword("_EMISSION");
                emissionOn = true;
                StartCoroutine(Blink());
            }
        }
        else
        {
            depthMaterial.DisableKeyword("_EMISSION");
            emissionOn = false;
        }
    }

    /*
     * A simple coroutine recursively enabling and disabling emission
     */
    private IEnumerator Blink()
    {
        yield return new WaitForSeconds(0.5f);
        if (emissionOn)
        {
            if (depthMaterial.IsKeywordEnabled("_EMISSION")) depthMaterial.DisableKeyword("_EMISSION");
            else depthMaterial.EnableKeyword("_EMISSION");
            StartCoroutine(Blink());
        }
    }
}

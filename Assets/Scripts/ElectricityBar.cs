using System.Collections;
using UnityEngine;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script controls the electrcity bar behaviour in the cabin of the submarine.
 */
public class ElectricityBar : MonoBehaviour
{
    private Player player;
    private float fullElectricityScale;
    private Material electricityMaterial;
    private Color fullElectricityColor;
    private bool emissionOn = false;
    private bool notifiedLowElectricity = false;

    void Start()
    {
        // Get necessary references
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        fullElectricityScale = transform.localScale.y;
        electricityMaterial = GetComponent<MeshRenderer>().material;
        fullElectricityColor = electricityMaterial.color;
    }

    void Update()
    {
        float currentElectricityClamped = (player.GetElectricity() / player.maxElectricity);

        // Adjust the scale of the colored rectangle based on the current electricity level
        Vector3 scale = transform.localScale;
        scale.y = fullElectricityScale * currentElectricityClamped;
        transform.localScale = scale;
        transform.localPosition = new Vector3(transform.localPosition.x, (scale.y - fullElectricityScale) / 2, transform.localPosition.z);

        // Adjust the color of the electricity bar based on the current electricity level
        Color currentColor = electricityMaterial.color;
        currentColor.g = fullElectricityColor.g * currentElectricityClamped;
        currentColor.b = fullElectricityColor.b * currentElectricityClamped;
        electricityMaterial.color = currentColor;

        // Handle low electricity warning sound and bar blinking
        if (currentElectricityClamped <= 0.15f)
        {
            if (!notifiedLowElectricity)
            {
                GetComponent<AudioSource>().Play();
                notifiedLowElectricity = true;
            }
            if (!emissionOn)
            {
                electricityMaterial.EnableKeyword("_EMISSION");
                emissionOn = true;
                StartCoroutine(Blink());
            }
        }
    }

    /*
     * A simple Coroutine which recursively makes the electricity bar blink.
     */
    private IEnumerator Blink()
    {
        yield return new WaitForSeconds(0.5f);
        if (emissionOn)
        {
            if (electricityMaterial.IsKeywordEnabled("_EMISSION")) electricityMaterial.DisableKeyword("_EMISSION");
            else electricityMaterial.EnableKeyword("_EMISSION");
            StartCoroutine(Blink());
        }
    }
}

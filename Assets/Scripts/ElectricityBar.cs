using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricityBar : MonoBehaviour
{
    private Player player;
    private float fullElectricityScale;
    private Material electricityMaterial;
    private Color fullElectricityColor;
    private bool emissionOn = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        fullElectricityScale = transform.localScale.y;
        electricityMaterial = GetComponent<MeshRenderer>().material;
        fullElectricityColor = electricityMaterial.color;
    }

    void Update()
    {
        float currentElectricityClamped = (player.GetElectricity() / player.maxElectricity);

        Vector3 scale = transform.localScale;
        scale.y = fullElectricityScale * currentElectricityClamped;
        transform.localScale = scale;
        transform.localPosition = new Vector3(transform.localPosition.x, (scale.y - fullElectricityScale) / 2, transform.localPosition.z);

        Color currentColor = electricityMaterial.color;
        //currentColor.r = 1 - currentElectricityClamped; leave it
        currentColor.g = fullElectricityColor.g * currentElectricityClamped;
        currentColor.b = fullElectricityColor.b * currentElectricityClamped;
        electricityMaterial.color = currentColor;

        if (currentElectricityClamped <= 0.15f)
        {
            if (!GetComponent<AudioSource>().isPlaying && !player.gameOver) GetComponent<AudioSource>().Play();
            if (!emissionOn)
            {
                electricityMaterial.EnableKeyword("_EMISSION");
                emissionOn = true;
                StartCoroutine(Blink());
            }
        }
    }

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

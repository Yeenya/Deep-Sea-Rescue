using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using System.IO;
using System;
using System.Text;
using System.Linq;
using System.Globalization;

public class ReplayPlayer : MonoBehaviour
{
    private Light periscopeLight;
    private Light leftLight;
    private Light rightLight;
    private readonly float periscopeLightMaxIntensity = 4f;
    private readonly float additionalLightsMaxIntensity = 2f;

    private readonly float maxElectricity = 600f;
    private float electricity;
    private bool mainLightOn = false;
    private bool leftLightOn = false;
    private bool rightLightOn = false;

    private StreamReader reader;

    [SerializeField]
    private AudioSource rotorSound;

    [SerializeField]
    private ParticleSystem terrainParticles;

    [SerializeField]
    private AudioSource terrainDragSound;

    [SerializeField]
    private GameObject tiltModel;

    [SerializeField]
    private GameObject insideSpot;

    void Start()
    {
        periscopeLight = GameObject.FindGameObjectWithTag("Periscope").GetComponentInChildren<Light>();
        leftLight = GameObject.FindGameObjectWithTag("Left Light").GetComponent<Light>();
        rightLight = GameObject.FindGameObjectWithTag("Right Light").GetComponent<Light>();

        periscopeLight.intensity = 0;
        leftLight.intensity = 0;
        rightLight.intensity = 0;

        electricity = maxElectricity;

        tiltModel = GameObject.FindGameObjectWithTag("TiltModel");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();
    }

    void FixedUpdate()
    {
        string currentLine = reader.ReadLine();
        if (currentLine == null) return;
        string[] currentValues = currentLine.Split(',');
        if (currentValues.Length != 18) return;

        Vector3 position;
        position.x = float.Parse(currentValues[0], CultureInfo.InvariantCulture);
        position.y = float.Parse(currentValues[1], CultureInfo.InvariantCulture);
        position.z = float.Parse(currentValues[2], CultureInfo.InvariantCulture);
        transform.position = position;

        Quaternion submarineRotation;
        submarineRotation.x = float.Parse(currentValues[5], CultureInfo.InvariantCulture);
        submarineRotation.y = float.Parse(currentValues[6], CultureInfo.InvariantCulture);
        submarineRotation.z = float.Parse(currentValues[7], CultureInfo.InvariantCulture);
        submarineRotation.w = float.Parse(currentValues[8], CultureInfo.InvariantCulture);
        transform.rotation = submarineRotation;

        Quaternion cameraRotation;
        cameraRotation.x = float.Parse(currentValues[9], CultureInfo.InvariantCulture);
        cameraRotation.y = float.Parse(currentValues[10], CultureInfo.InvariantCulture);
        cameraRotation.z = float.Parse(currentValues[11], CultureInfo.InvariantCulture);
        cameraRotation.w = float.Parse(currentValues[12], CultureInfo.InvariantCulture);
        if (Camera.main.GetComponent<CameraController>().GetInsideOrOutside())
        {
            Camera.main.transform.localRotation = cameraRotation;
            Camera.main.transform.position = insideSpot.transform.position;
        }

        if (mainLightOn != bool.Parse(currentValues[13])) ChangeMainLight();
        if (leftLightOn != bool.Parse(currentValues[14])) ChangeLeftLight();
        if (rightLightOn != bool.Parse(currentValues[15])) ChangeRightLight();

        tiltModel.transform.localRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, 0, 0));
    }

    private Vector3 StringToVector3(string str)
    {
        Vector3 result;
        string[] axes = str.Split(',');
        result.x = float.Parse(axes[0]);
        result.y = float.Parse(axes[1]);
        result.z = float.Parse(axes[2]);
        return result;
    }

    private Quaternion StringToQuaternion(string str)
    {
        Quaternion quaternion;
        string[] values = str.Split(",");
        quaternion.x = float.Parse(values[0]);
        quaternion.y = float.Parse(values[1]);
        quaternion.z = float.Parse(values[2]);
        quaternion.w = float.Parse(values[3]);
        return quaternion;
    }

    private void ChangeMainLight()
    {
        if (periscopeLight.intensity == 0)
        {
            periscopeLight.DOIntensity(periscopeLightMaxIntensity, 0.5f);
            periscopeLight.transform.parent.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        }
        else
        {
            periscopeLight.DOIntensity(0, 0.5f);
            periscopeLight.transform.parent.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
        }

        mainLightOn = !mainLightOn;
    }

    private void ChangeLeftLight()
    {
        if (leftLight.intensity == 0) leftLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else leftLight.DOIntensity(0, 0.5f);
        leftLightOn = !leftLightOn;
    }

    private void ChangeRightLight()
    {
        if (rightLight.intensity == 0) rightLight.DOIntensity(additionalLightsMaxIntensity, 0.5f);
        else rightLight.DOIntensity(0, 0.5f);
        rightLightOn = !rightLightOn;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            terrainParticles.Play();
            terrainDragSound.DOFade(1, 1);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            terrainParticles.Stop();
            terrainDragSound.DOFade(0, 1);
        }
    }

    public void SetStreamReader(string file)
    {
        reader = new StreamReader(file);
        reader.ReadLine();
    }
}

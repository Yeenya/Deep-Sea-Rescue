using DG.Tweening;
using System.Globalization;
using System.IO;
using UnityEngine;

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

    public int replaySpeed = 1;

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
        if (Input.GetKeyDown(KeyCode.KeypadPlus) && replaySpeed < 10) replaySpeed++;
        if (Input.GetKeyDown(KeyCode.KeypadMinus) && replaySpeed > 1) replaySpeed--;
    }

    void FixedUpdate()
    {
        for (int i = 1; i < replaySpeed; i++) reader.ReadLine();
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
        GameObject markers = GameObject.Find("Markers");
        if (markers != null) Destroy(markers);
        markers = new GameObject("Markers");
        StreamReader markerReader = new(file);
        markerReader.ReadLine();
        while (!markerReader.EndOfStream)
        {
            for (int i = 0; i < 49; i++) markerReader.ReadLine();
            string currentLine = markerReader.ReadLine();
            if (currentLine == null) break;
            string[] currentValues = currentLine.Split(',');
            if (currentValues.Length != 18) break;

            Vector3 position;
            position.x = float.Parse(currentValues[0], CultureInfo.InvariantCulture);
            position.y = float.Parse(currentValues[1], CultureInfo.InvariantCulture);
            position.z = float.Parse(currentValues[2], CultureInfo.InvariantCulture);

            Quaternion submarineRotation;
            submarineRotation.x = float.Parse(currentValues[5], CultureInfo.InvariantCulture);
            submarineRotation.y = float.Parse(currentValues[6], CultureInfo.InvariantCulture);
            submarineRotation.z = float.Parse(currentValues[7], CultureInfo.InvariantCulture);
            submarineRotation.w = float.Parse(currentValues[8], CultureInfo.InvariantCulture);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.SetPositionAndRotation(position, submarineRotation);
            marker.transform.Rotate(-90, 0, 0);
            marker.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);

            marker.transform.parent = markers.transform;
        }
        markerReader.Close();
        reader = new(file);
        reader.ReadLine();
    }

    public void SetSettings(string file)
    {
        StreamReader settingsReader = new(file);
        settingsReader.ReadLine();
        string[] settings = settingsReader.ReadLine().Split(',');
        Vector3 submarinePosition;
        submarinePosition.x = float.Parse(settings[0], CultureInfo.InvariantCulture);
        submarinePosition.y = float.Parse(settings[1], CultureInfo.InvariantCulture);
        submarinePosition.z = float.Parse(settings[2], CultureInfo.InvariantCulture);
        transform.position = submarinePosition;

        Vector3 diver1Position;
        diver1Position.x = float.Parse(settings[3], CultureInfo.InvariantCulture);
        diver1Position.y = float.Parse(settings[4], CultureInfo.InvariantCulture);
        diver1Position.z = float.Parse(settings[5], CultureInfo.InvariantCulture);
        GameObject.Find("Diver1").transform.position = diver1Position;

        Vector3 diver2Position;
        diver2Position.x = float.Parse(settings[6], CultureInfo.InvariantCulture);
        diver2Position.y = float.Parse(settings[7], CultureInfo.InvariantCulture);
        diver2Position.z = float.Parse(settings[8], CultureInfo.InvariantCulture);
        GameObject.Find("Diver2").transform.position = diver2Position;

        Vector3 diver3Position;
        diver3Position.x = float.Parse(settings[9], CultureInfo.InvariantCulture);
        diver3Position.y = float.Parse(settings[10], CultureInfo.InvariantCulture);
        diver3Position.z = float.Parse(settings[11], CultureInfo.InvariantCulture);
        GameObject.Find("Diver3").transform.position = diver3Position;

        Vector3 diver4Position;
        diver4Position.x = float.Parse(settings[12], CultureInfo.InvariantCulture);
        diver4Position.y = float.Parse(settings[13], CultureInfo.InvariantCulture);
        diver4Position.z = float.Parse(settings[14], CultureInfo.InvariantCulture);
        GameObject.Find("Diver4").transform.position = diver4Position;

        Vector3 diver5Position;
        diver5Position.x = float.Parse(settings[15], CultureInfo.InvariantCulture);
        diver5Position.y = float.Parse(settings[16], CultureInfo.InvariantCulture);
        diver5Position.z = float.Parse(settings[17], CultureInfo.InvariantCulture);
        GameObject.Find("Diver5").transform.position = diver5Position;

        Vector3 testingDiverPosition;
        testingDiverPosition.x = float.Parse(settings[18], CultureInfo.InvariantCulture);
        testingDiverPosition.y = float.Parse(settings[19], CultureInfo.InvariantCulture);
        testingDiverPosition.z = float.Parse(settings[20], CultureInfo.InvariantCulture);
        GameObject.Find("TestingDiver").transform.position = testingDiverPosition;
    }
}

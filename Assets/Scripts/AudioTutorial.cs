using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTutorial : MonoBehaviour
{
    private Player player;
    private Vector3 startingPosition = new(207.893082f, 131.025192f, 189.029999f);

    [SerializeField]
    private GameObject moveSphere;

    [SerializeField]
    private GameObject tiltSphere;

    void Start()
    {
        player = GetComponent<Player>();
    }

    public IEnumerator Tutorial()
    {
        transform.SetPositionAndRotation(startingPosition, Quaternion.identity);
        if (!Camera.main.GetComponent<CameraController>().GetInsideOrOutside()) Camera.main.GetComponent<CameraController>().ChangeCameraPosition();
        GameObject[] divers = GameObject.FindGameObjectsWithTag("Diver");
        foreach (GameObject diver in divers)
        {
            diver.GetComponent<AudioSource>().volume = 0f;
            diver.GetComponent<AudioSource>().Stop();
        }

        //      ADD SUCCESS VOICEOVERS AFTER EVERY TASK

        //UNDOCK
        //Add voiceover for undocking here
        yield return new WaitUntil(() => player.state == Player.State.FREE);
        print("Undocked, get to sphere");

        //INTRODUCE SONAR SOUND
        //Add voiceover for sonar introduction here
        moveSphere.GetComponent<AudioSource>().Play();

        //MOVE
        //Add voiceover for moving here
        yield return new WaitUntil(() => Vector3.Distance(moveSphere.transform.position, transform.position) <= 8f);
        print("Got move sphere, get to tilt sphere");
        Destroy(moveSphere);

        //TILT + height warning
        tiltSphere.GetComponent<AudioSource>().Play();
        //Add voiceover for tilting here
        yield return new WaitUntil(() => Vector3.Distance(tiltSphere.transform.position, transform.position) <= 8f);
        print("Got tilt sphere, change view");
        Destroy(tiltSphere);

        //CHANGE VIEW
        //Add voiceover for changing view here
        yield return new WaitUntil(() => Camera.main.GetComponent<CameraController>().GetInsideOrOutside() == false);
        print("Changed view, change view again");
        yield return new WaitUntil(() => Camera.main.GetComponent<CameraController>().GetInsideOrOutside() == true);
        print("Changed view again, turn on all lights");

        //LIGHTS
        //Add voiceover for lights here
        yield return new WaitUntil(() => player.GetLightsOn());
        print("Turned on all lights, listen to diver sounds");

        //FIND + RESCUE DIVER
        //Add voiceover for finding + rescuing here
        GameObject tutorialDiver = Instantiate(GetComponent<Player>().GetDiverPrefab(), transform.position - transform.right * 10, Quaternion.identity);

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position - transform.right * 50;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position - transform.right * 100;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position + transform.forward * 10;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position + transform.forward * 50;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position + transform.forward * 100;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position + transform.right * 10;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position + transform.right * 50;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position + transform.right * 100;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position - transform.forward * 10;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position - transform.forward * 50;

        yield return new WaitForSeconds(4f);

        tutorialDiver.transform.position = transform.position - transform.forward * 100;

        yield return new WaitForSeconds(4f);

        Destroy(tutorialDiver);

        tutorialDiver = GameObject.Find("TestingDiver");
        tutorialDiver.GetComponent<AudioSource>().volume = 1f;
        tutorialDiver.GetComponent<AudioSource>().Play();

        print("Listened to diver sounds, rescue diver");

        //Add voiceover for finding and rescuing tutorial diver here
        yield return new WaitUntil(() => player.GetSavedDivers() == 1);
        print("Diver rescued, go dock");

        //RETURN TO BASE + DOCK
        //Add voiceover for returning here
        yield return new WaitUntil(() => player.state == Player.State.DOCKED);
        print("Docked, finish");
        //Add final voiceover here

        foreach (GameObject diver in divers)
        {
            if (diver == null) continue;
            diver.GetComponent<AudioSource>().volume = 1f;
            diver.GetComponent<AudioSource>().Play();
        }
    }
}

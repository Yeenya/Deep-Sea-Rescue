using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Developed by Jan Borecký, 2024-2025
 * This script handles the Replay Menu in the game's UI.
 */
public class ReplayMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject replayButtonPrefab;

    private bool replaysLoaded = false;

    /*
     * Load replay files saved in the persistent data path (on Windows: .../Users/your_username/AppData/LocalLow/DefaultCompany/Deep Sea Rescue/Data).
     */
    public void LoadSavedReplays()
    {
        gameObject.SetActive(true);
        if (replaysLoaded) return;

        foreach (string fileName in Directory.GetFiles(Application.persistentDataPath + "/Data"))
        {
            if (fileName.Contains("Settings")) continue; // In UI we do not care about the settings. Those are loaded when a replay is selected and starting to play.
            GameObject replayButton = Instantiate(replayButtonPrefab, transform);
            replayButton.transform.SetSiblingIndex(transform.childCount - 2);
            replayButton.GetComponentInChildren<TextMeshProUGUI>().text = Path.GetFileName(fileName);
            replayButton.GetComponent<Button>().onClick.AddListener(() => LoadReplay(fileName));
        }
        replaysLoaded = true;
    }

    /*
     * Load a specific replay file.
     */
    public void LoadReplay(string replayFileName)
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.state = Player.State.REPLAY;
        player.SetStreamReader(replayFileName);
        player.SetSettings(replayFileName[..^4] + "_Settings");
    }
}

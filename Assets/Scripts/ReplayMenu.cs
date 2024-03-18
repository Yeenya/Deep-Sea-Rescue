using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject replayButtonPrefab;

    private bool replaysLoaded = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSavedReplays()
    {
        gameObject.SetActive(true);
        if (replaysLoaded) return;

        foreach (string fileName in Directory.GetFiles(Application.persistentDataPath + "/Data"))
        {
            if (fileName.Contains("Settings")) continue;
            GameObject replayButton = Instantiate(replayButtonPrefab, transform);
            replayButton.transform.SetSiblingIndex(transform.childCount - 2);
            replayButton.GetComponentInChildren<TextMeshProUGUI>().text = Path.GetFileName(fileName);
            replayButton.GetComponent<Button>().onClick.AddListener(() => LoadReplay(fileName));
        }
        replaysLoaded = true;
    }

    public void LoadReplay(string replayFileName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<Player>().enabled = false;
        player.GetComponent<ReplayPlayer>().SetStreamReader(replayFileName);
        player.GetComponent<ReplayPlayer>().SetSettings(replayFileName[..^4] + "_Settings.csv");
        player.GetComponent<ReplayPlayer>().enabled = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            transform.GetChild(0).gameObject.SetActive(true);
            Time.timeScale = 0;
        }
        print(Time.timeScale);
    }

    public void BackButton()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void NewGameStart()
    {
        SceneManager.LoadScene("DialogueScene");
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }
}

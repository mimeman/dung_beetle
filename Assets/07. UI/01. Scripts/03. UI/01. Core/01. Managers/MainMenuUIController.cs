using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    [SerializeField] private string mainScene;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    void Update()
    {

    }

    public void NewGameStart()
    {
        SceneManager.LoadScene(mainScene);
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }
}

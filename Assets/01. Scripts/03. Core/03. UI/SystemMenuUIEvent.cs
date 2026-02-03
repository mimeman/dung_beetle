using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemMenuButtonEvent : MonoBehaviour
{

    [SerializeField] private string SceneName;
    //SceneName = TitleScene
    [SerializeField] private GameObject SystemMenu;
    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                PauseGame();
            }
        }
    }
    private void OnDisable()
    {
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    private void PauseGame()
    {
        isPaused = true;
        SystemMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    public void OnClick_btnResume()
    {
        Debug.Log("출력: 돌아가기 클릭");
        isPaused = false;
        SystemMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnClick_btnSave()
    {
        Debug.Log("출력: 저장 클릭");
    }
    public void OnClick_btnLoad()
    {
        Debug.Log("출력: 불러오기 클릭");
    }
    public void OnClick_btnOption()
    {
        Debug.Log("출력: 설정 클릭");
    }
    public void OnClick_btnQuitToTIle()
    {
        Debug.Log("출력: " + SceneName + "씬 이동");
        //SceneManager.LoadScene("SceneName");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TItleUIEvent : MonoBehaviour
{
    public string SceneName = null;
    public void OnClick_btnNewGame()
    {
        Debug.Log("출력: " + SceneName + "씬 이동");
        //SceneManager.LoadScene("SceneName");
    }
    public void OnClick_btnLoad()
    {
        Debug.Log("출력: 불러오기 클릭");
    }
    public void OnClick_btnOption()
    {
        Debug.Log("출력: 설정 클릭");
    }
    public void OnClick_btnQuit()
    {
        Debug.Log("출력: 종료 클릭");
        //Application.Quit();
    }
}

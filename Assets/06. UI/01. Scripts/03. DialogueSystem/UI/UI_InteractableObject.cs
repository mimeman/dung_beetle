using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InteractableObject : UIBase
{
    public override void Init()
    {
        base.Init();
        // 필요에 따라 UI 바인딩 추가 (예: GetUI<TextMeshProUGUI>("Text_Title"))
    }

    public void SetData(string title = "", string desc = "")
    {
        // 텍스트/데이터 설정 로직 추가
    }

    public void CloseWindow()
    {
        UIManager.Instance.Hide<UI_InteractableObject>();
    }
}

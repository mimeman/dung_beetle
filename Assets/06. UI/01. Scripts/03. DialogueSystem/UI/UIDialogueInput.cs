using UnityEngine;

public class UIDialogueInput : UIInputLogic<UI_Dialogue>
{
    public override void Init(UI_Dialogue view)
    {
        base.Init(view);
    }

    protected override void Update()
    {
        // 뷰가 없거나 꺼져있으면 입력 받지 마
        if (_view == null || !_view.gameObject.activeSelf) return;

        HandleNext();
        HandleSkip();
    }

    // 다음 넘기기 조건
    private void HandleNext()
    {
        if (
            Input.GetKeyDown(KeyCode.F) ||
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            _view.OnNext(); // UI에게 명령
        }
    }

    // 전체 스킵 조건
    private void HandleSkip()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _view.OnFullSkip(); // UI에게 명령
        }
    }
}
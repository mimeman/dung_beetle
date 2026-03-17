using System;

public interface IRebind
{
    public void StartRebinding(string actionName, int bindingIndex, Action onComplete, Action onCancel = null);

    public void SaveBindings();

    public void LoadBindings();

    // public void StartRebinding(string actionName, int bindingIndex, Action onComplete, Action onCancel = null)
    // {
    //     var action = _inputActions.FindAction(actionName);
    //     action.Disable();

    //     action.PerformInteractiveRebinding(bindingIndex)
    //         .WithCancelingThrough("<Keyboard>/escape") // ESC로 취소
    //         .OnComplete(op =>
    //         {
    //             op.Dispose();
    //             action.Enable();
    //             SaveBindings();
    //             onComplete?.Invoke();
    //         })
    //         .OnCancel(op =>
    //         {
    //             op.Dispose();
    //             action.Enable();
    //             onCancel?.Invoke(); // 취소 콜백
    //         })
    //         .Start();
    // }
    // public void SaveBindings()
    // {
    //     var json = _inputActions.SaveBindingOverridesAsJson();
    //     PlayerPrefs.SetString("InputBindings", json);
    //     PlayerPrefs.Save();
    //     Debug.Log($"[InputReader] 바인딩 저장 완료: {json}");
    // }
    // public void LoadBindings()
    // {
    //     if (PlayerPrefs.HasKey("InputBindings"))
    //     {
    //         var json = PlayerPrefs.GetString("InputBindings");
    //         _inputActions.asset.LoadBindingOverridesFromJson(json);
    //         Debug.Log($"[InputReader] 바인딩 로드 완료: {json}"); // 로드 내용 확인
    //     }
    //     else
    //     {
    //         Debug.Log("[InputReader] 저장된 바인딩 없음");
    //     }
    // }
}

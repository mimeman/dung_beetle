using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Dung.Inputs;
using Michsky.MUIP;

public class RebindUI : MonoBehaviour
{
    [SerializeField] private ModalWindowManager _modalWindow;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private string _actionName;
    [SerializeField] private int _bindingIndex;

    private TMP_Text _label;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _label = GetComponentInChildren<TMP_Text>();

        if (_button == null)
            Debug.LogError($"RebindUI [{gameObject.name}]: Button 컴포넌트를 찾을 수 없습니다.");
        if (_label == null)
            Debug.LogError($"RebindUI [{gameObject.name}]: TMP_Text 컴포넌트를 찾을 수 없습니다.");
    }

    private void Start()
    {
        RefreshLabel();
        _button.onClick.AddListener(StartRebind);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(StartRebind);
    }

    private void StartRebind()
    {
        _label.text = "...";
        _inputReader.StartRebinding(
            _actionName,
            _bindingIndex,
            onComplete: OnRebindComplete,
            onCancel: OnRebindCancelled
        );
    }

    private void OnRebindComplete()
    {
        _modalWindow.Close();
        RefreshLabel();
    }

    private void OnRebindCancelled()
    {
        RefreshLabel(); // 원래 키 이름 복원
    }

    private void RefreshLabel()
    {
        var action = _inputReader.InputActions.FindAction(_actionName);
        if (action == null)
        {
            Debug.LogWarning($"RebindUI [{gameObject.name}]: '{_actionName}' 액션을 찾을 수 없습니다.");
            return;
        }
        _label.text = action.GetBindingDisplayString(_bindingIndex);
    }



    [ContextMenu("Print ActionMap")]
    public void PrintActionMap()
    {
        var asset = _inputReader.InputActions;
        foreach (var action in asset)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                Debug.Log($"Action: {action.name} | Index: {i} | Path: {action.bindings[i].path} | Name: {action.bindings[i].name}");
            }
        }
    }
}

using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    public InteractionType InteractType => InteractionType.Press;

    public string InteractionPrompt { get{return"일기 읽어보기";} }

    public bool CanInteract => true;

    public bool OnInteract(GameObject interactor)
    {
        // 상호작용 가능한 상태인지 확인 후 처리 (CanInteract가 true일 때 등)
        var ui = UIManager.Instance.Show<UI_InteractableObject>();
        
        // 필요 시 ui.SetData(...) 호출
        
        return true;
    }

    public void OnFocus()
    {

    }

    public void OnLoseFocus()
    {

    }
}

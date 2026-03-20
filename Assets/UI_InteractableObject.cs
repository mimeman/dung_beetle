using UnityEngine;

public class UI_InteractableObject : MonoBehaviour, IInteractable
{
    public InteractionType InteractType { get; }

    public string InteractionPrompt { get; }

    public bool CanInteract { get; }

    public bool OnInteract(GameObject interactor)
    {
        return false;
    }

    public void OnFocus()
    {

    }

    public void OnLoseFocus()
    {

    }
}

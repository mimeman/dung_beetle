using UnityEngine;

public class HouseEnterTrigger : MonoBehaviour
{
    public GameObject[] roofObjects;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        CameraManager.Instance.EnterHouse();
        SetRoof(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        CameraManager.Instance.ExitHouse();
        SetRoof(true);
    }

    private void SetRoof(bool visible)
    {
        foreach (var obj in roofObjects)
            if (obj != null) obj.SetActive(visible);
    }
}
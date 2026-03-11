using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    private Transform _cam;

    void Start()
    {
        _cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (_cam == null) return;

        transform.forward = _cam.forward;
    }
}
using UnityEngine;

public class SkyRotator : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 3f;

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, rotateSpeed * Time.deltaTime, 0));
    }
}

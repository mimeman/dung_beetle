using System.Collections;
using UnityEngine;

public class ToadCamouflageSystem : MonoBehaviour
{
    private ToadController toad;
    private new Renderer renderer;
    private float currentAlpha = 1f;

    public void Initialize(ToadController toad)
    {
        this.toad = toad;
        renderer = GetComponentInChildren<Renderer>();
    }

    public void SetCamouflage(bool active)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCamouflage(active ? 0.1f : 1.0f)); // 0.1은 투명, 1.0은 불투명
    }

    private IEnumerator FadeCamouflage(float targetAlpha)
    {
        float startAlpha = currentAlpha;
        float time = 0;

        while (time < toad.ToadConfig.camouflageFadeTime)
        {
            time += Time.deltaTime;
            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / toad.ToadConfig.camouflageFadeTime);

            // Material 프로퍼티 변경 (Shader에 _CamouflageAmount가 있다고 가정)
            renderer.material.SetFloat("_CamouflageAmount", currentAlpha);
            yield return null;
        }
    }
}
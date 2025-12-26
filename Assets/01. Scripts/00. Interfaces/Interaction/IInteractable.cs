using UnityEngine;

#region 설명
/* [설명]
 * 플레이어가 'E'키(상호작용 키)를 눌러서 수행하는 모든 동작의 기본 규약.
 * NPC 대화, 문 열기, 레버 당기기, 밧줄 잡기, 아이템 줍기 등이 포함됨.
 
 * [작동 흐름]
 * 1. PlayerSensor(Raycast)가 감지 -> OnFocus() (외곽선/UI 표시).
 * 2. 플레이어가 입력(E) -> OnInteract() (동작 수행).
 * 3. 시선 벗어남 -> OnLoseFocus() (UI 숨김).
 */
#endregion

public interface IInteractable
{
    //상호작용 시 UI에 띄울 안내 문구 (예: "대화하기", "밧줄 잡기", "줍기")
    string InteractionPrompt { get; }

    // 현재 상호작용이 가능한 상태인지 확인 (잠긴 문, 이미 사용한 레버 등 체크)
    bool CanInteract { get; }

    // 상호작용을 수행합니다. (E키 입력 시 호출)
    /// <param name="interactor">상호작용을 시도한 주체 (플레이어)</param>
    void OnInteract(GameObject interactor);

    // 플레이어의 시선이 닿았을 때 (외곽선, UI 팝업)
    void OnFocus();

    // 플레이어의 시선이 벗어났을 때
    void OnLoseFocus();
}
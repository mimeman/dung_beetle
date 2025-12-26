#region 설명
/* [설명]
 * 게임 오버나 체크포인트 로드 시, 객체를 초기 상태로 되돌리는 규약.
 * 씬 전체를 다시 로드(SceneManager.LoadScene)하는 것보다 훨씬 빠르고 효율적임.
 
 * [작동 흐름]
 * 1. 플레이어 사망 -> GameManager가 등록된 모든 IResettable 객체 호출.
 * 2. 몬스터는 제자리로, 퍼즐은 초기화, 플레이어는 체크포인트로 이동.
 */
#endregion

public interface IResettable
{
    // 객체를 초기 상태(또는 마지막 체크포인트 상태)로 되돌립니다.
    void ResetState();
}
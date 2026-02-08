using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;

public class ToadProceduralJump : MonoBehaviour
{
    [Header("References")]
    public FullBodyBipedIK ik;
    public GrounderFBBIK grounder;

    [Header("Jump Settings")]
    public float jumpDistance = 2.0f; // 앞으로 가는 거리
    public float jumpHeight = 1.0f;   // 점프 높이
    public float jumpDuration = 0.8f; // 점프에 걸리는 시간

    [Header("Curves")]
    // X축: 시간(0~1), Y축: 높이 비율 (0에서 시작해서 1찍고 0으로)
    public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

    // X축: 시간(0~1), Y축: IK 가중치 (1에서 시작해서 0찍고 1로)
    // 점프 중에는 발이 땅에서 떨어져야 하므로 중간에 0이 되어야 함
    public AnimationCurve ikWeightCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.2f, 0), new Keyframe(0.8f, 0), new Keyframe(1, 1));

    private bool isJumping = false;

    void Update()
    {
        // 테스트용: 스페이스바를 누르면 점프
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + transform.forward * jumpDistance;

        float timer = 0f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration; // 0 ~ 1 사이 값

            // 1. 실제 몸체 이동 (Parabola/포물선 이동)
            // 수평 이동
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            // 수직 이동 (Animation Curve 활용)
            currentPos.y += heightCurve.Evaluate(progress) * jumpHeight;

            transform.position = currentPos;

            // 2. Final IK 제어 (절차적 애니메이션 핵심)
            float footWeight = ikWeightCurve.Evaluate(progress);

            // Grounder 끄기/켜기 (점프 중엔 0, 착지하면 1)
            // Grounder가 켜져 있으면 점프해도 발을 땅으로 억지로 끌어내림
            if (grounder != null)
                grounder.weight = footWeight;

            // FBBIK 발 이펙터 가중치 조절
            // 가중치가 0이 되면 발이 뼈(Bone) 위치로 돌아가서 몸을 따라옴
            ik.solver.leftFootEffector.positionWeight = footWeight;
            ik.solver.rightFootEffector.positionWeight = footWeight;
            ik.solver.leftHandEffector.positionWeight = footWeight;
            ik.solver.rightHandEffector.positionWeight = footWeight;

            // [선택사항] 공중에서 다리 모양 잡기
            // 점프 정점일 때 다리를 몸쪽으로 당기거나 앞으로 뻗게 하려면 
            // ik.solver.leftFootEffector.positionOffset 등을 조절해야 함

            yield return null;
        }

        // 3. 착지 후 확실하게 초기화
        if (grounder != null) grounder.weight = 1f;
        ik.solver.leftFootEffector.positionWeight = 1f;
        ik.solver.rightFootEffector.positionWeight = 1f;
        ik.solver.leftHandEffector.positionWeight = 1f;
        ik.solver.rightHandEffector.positionWeight = 1f;

        isJumping = false;
    }
}
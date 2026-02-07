using System.Collections;
using System.Collections.Generic;
using Dung.Data;
using UnityEngine;

public class ToadController : AIController
{
    public ToadConfig ToadConfig;
    // 하위 시스템
    public ToadTongueController Tongue;
    public ToadPullSystem PullSystem;
    public ToadCamouflageSystem Camouflage;

    // StateMachine 래퍼 (BaseState<ToadMonster>를 쓰기 위한 캐스팅 도우미) 
    public new ToadStateMachine StateMachine;

    protected override void Awake()
    {
        base.Awake();
        // 컴포넌트 초기화
        Tongue = GetComponent<ToadTongueController>();
        PullSystem = GetComponent<ToadPullSystem>();
        Camouflage = GetComponent<ToadCamouflageSystem>();

        // 하위 시스템 초기화 주입
        Tongue.Initialize(this);
        PullSystem.Initialize(this);
        Camouflage.Initialize(this);
    }

    protected override void Start()
    {
        // 상태 머신 시작 (Idle로 시작)
        ChangeState(StateMachine.IdleState);
    }

    #region Movement

    public void RotateTowardsTarget()
    {
        if (Target == null) return;
        Vector3 dir = (Target.position - transform.position).normalized;
        dir.y = 0; // Y축 회전 방지
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    #endregion

    #region Animation

    // Animator Hash 캐싱
    public readonly int HashIdle = Animator.StringToHash("Idle");
    public readonly int HashAim = Animator.StringToHash("Aim");
    public readonly int HashSnap = Animator.StringToHash("Snap");
    public readonly int HashPull = Animator.StringToHash("Pull");
    public readonly int HashBite = Animator.StringToHash("Bite");
    public readonly int HashStuck = Animator.StringToHash("Stuck");
    public readonly int HashRecover = Animator.StringToHash("Recover");

    #endregion
}
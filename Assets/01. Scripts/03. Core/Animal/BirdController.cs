using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : AIController
{
    [SerializeField] private float birdScale = 1.0f;
    [SerializeField] private bool collideWithObjects = true;
    [Space(10)]
    [SerializeField] private bool dead = false;
    [SerializeField] private bool flying = false;
    [SerializeField] private bool landing = true;
    [SerializeField] private bool onGround = true;

    public new BirdStateMachine stateMachine;
    private float distanceToTarget = 0;
    private int flyAnimationHash;
    SphereCollider solidCollider;
    Rigidbody rigidbody;

    void Start()
    {
        base.Initialize();
        InitializeAnimationHashes();
        solidCollider = GetComponent<SphereCollider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isHost) return;
        if (CurrentState == null || health.IsDead)
            return;

        // Target update
        target = sensor.Target;

        // 다음 State로 넘어가기 위한 state의 updateState 로직
        BaseState<AIController> nextState = CurrentState.UpdateState(this);
        if (nextState != CurrentState)
            ChangeState(nextState);
    }

    public void MoveForward(float speed)
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void RotateTowards(Vector3 targetPos, float turnSpeed)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion lookRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
    }

    public void MoveTo(Vector3 target)
    {
        StartCoroutine("FlyToTarget", target);
    }

    // 대상 위치로 날아가는 함수
    public IEnumerator FlyToTarget(Vector3 target)
    {
        // 날아가는 sound 재생
        // GetComponent<AudioSource>().PlayOneShot(flyAway1, .1f);

        flying = true;
        landing = false;
        onGround = false;
        rigidbody.isKinematic = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.drag = 0.5f;
        animator.applyRootMotion = false;
        animator.SetBool(animConfig.flyingBoolHash, true);
        animator.SetBool(animConfig.landingBoolHash, false);

        while (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != flyAnimationHash)
        {   // flyAnimationHash가 재생되기 전이라면 대기.
            yield return 0;
        }

        //birds fly up and away from their perch for 1 second before orienting to the next target
        rigidbody.AddForce((transform.forward * 50.0f * birdScale) + (transform.up * 100.0f * birdScale));
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime;
            if (t > .2f && !solidCollider.enabled && collideWithObjects)
            {
                solidCollider.enabled = true;
            }
            yield return 0;
        }
        //start to rotate toward target
        Vector3 vectorDirectionToTarget = (target - transform.position).normalized;
        Quaternion finalRotation = Quaternion.identity;
        Quaternion startingRotation = transform.rotation;
        distanceToTarget = Vector3.Distance(transform.position, target);
        Vector3 forwardStraight;//the forward vector on the xz plane
        RaycastHit hit;
        Vector3 tempTarget = target;
        t = 0.0f;

        //if the target is directly above the bird the bird needs to fly out before going up
        //this should stop them from taking off like a rocket upwards
        if (vectorDirectionToTarget.y > .5f)
        {
            tempTarget = transform.position + (new Vector3(transform.forward.x, .5f, transform.forward.z) * distanceToTarget);

            while (vectorDirectionToTarget.y > .5f)
            {
                //Debug.DrawLine (tempTarget,tempTarget+Vector3.up,Color.red);
                vectorDirectionToTarget = (tempTarget - transform.position).normalized;
                finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
                transform.rotation = Quaternion.Slerp(startingRotation, finalRotation, t);
                animator.SetFloat(animConfig.flyingDirectionHash, FindBankingAngle(transform.forward, vectorDirectionToTarget));
                t += Time.deltaTime * 0.5f;
                rigidbody.AddForce(transform.forward * 70.0f * birdScale * Time.deltaTime);

                //Debug.DrawRay (transform.position,transform.forward,Color.green);

                vectorDirectionToTarget = (target - transform.position).normalized;//reset the variable to reflect the actual target and not the temptarget

                if (Physics.Raycast(transform.position, -Vector3.up, out hit, 0.15f * birdScale) && rigidbody.velocity.y < 0)
                {
                    //if the bird is going to collide with the ground zero out vertical velocity
                    if (!hit.collider.isTrigger)
                    {
                        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
                    }
                }
                if (Physics.Raycast(transform.position, Vector3.up, out hit, 0.15f * birdScale) && rigidbody.velocity.y > 0)
                {
                    //if the bird is going to collide with something overhead zero out vertical velocity
                    if (!hit.collider.isTrigger)
                    {
                        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
                    }
                }
                //check for collisions with non trigger colliders and abort flight if necessary
                if (collideWithObjects)
                {
                    forwardStraight = transform.forward;
                    forwardStraight.y = 0.0f;
                    //Debug.DrawRay (transform.position+(transform.forward*.1f),forwardStraight*.75f,Color.green);
                    if (Physics.Raycast(transform.position + (transform.forward * .15f * birdScale), forwardStraight, out hit, .75f * birdScale))
                    {
                        if (!hit.collider.isTrigger)
                        {
                            AbortFlyToTarget();
                        }
                    }
                }
                yield return null;
            }
        }

        finalRotation = Quaternion.identity;
        startingRotation = transform.rotation;
        distanceToTarget = Vector3.Distance(transform.position, target);

        //rotate the bird toward the target over time
        while (transform.rotation != finalRotation || distanceToTarget >= 1.5f)
        {
            distanceToTarget = Vector3.Distance(transform.position, target);
            vectorDirectionToTarget = (target - transform.position).normalized;
            if (vectorDirectionToTarget == Vector3.zero)
            {
                vectorDirectionToTarget = new Vector3(0.0001f, 0.00001f, 0.00001f);
            }
            finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
            transform.rotation = Quaternion.Slerp(startingRotation, finalRotation, t);
            animator.SetFloat(animConfig.flyingDirectionHash, FindBankingAngle(transform.forward, vectorDirectionToTarget));
            t += Time.deltaTime * 0.5f;
            rigidbody.AddForce(transform.forward * 70.0f * birdScale * Time.deltaTime);
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 0.15f * birdScale) && rigidbody.velocity.y < 0)
            {
                //if the bird is going to collide with the ground zero out vertical velocity
                if (!hit.collider.isTrigger)
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
                }
            }
            if (Physics.Raycast(transform.position, Vector3.up, out hit, 0.15f * birdScale) && rigidbody.velocity.y > 0)
            {
                //if the bird is going to collide with something overhead zero out vertical velocity
                if (!hit.collider.isTrigger)
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
                }
            }

            //check for collisions with non trigger colliders and abort flight if necessary
            if (collideWithObjects)
            {
                forwardStraight = transform.forward;
                forwardStraight.y = 0.0f;
                //Debug.DrawRay (transform.position+(transform.forward*.1f),forwardStraight*.75f,Color.green);
                if (Physics.Raycast(transform.position + (transform.forward * .15f * birdScale), forwardStraight, out hit, .75f * birdScale))
                {
                    if (!hit.collider.isTrigger)
                    {
                        AbortFlyToTarget();
                    }
                }
            }
            yield return 0;
        }

        //keep the bird pointing at the target and move toward it
        float flyingForce = 50.0f * birdScale;
        while (true)
        {
            //do a raycast to see if the bird is going to hit the ground
            if (Physics.Raycast(transform.position, -Vector3.up, 0.15f * birdScale) && rigidbody.velocity.y < 0)
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
            }
            if (Physics.Raycast(transform.position, Vector3.up, out hit, 0.15f * birdScale) && rigidbody.velocity.y > 0)
            {
                //if the bird is going to collide with something overhead zero out vertical velocity
                if (!hit.collider.isTrigger)
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
                }
            }

            //check for collisions with non trigger colliders and abort flight if necessary
            if (collideWithObjects)
            {
                forwardStraight = transform.forward;
                forwardStraight.y = 0.0f;
                //Debug.DrawRay (transform.position+(transform.forward*.1f),forwardStraight*.75f,Color.green);
                if (Physics.Raycast(transform.position + (transform.forward * .15f * birdScale), forwardStraight, out hit, .75f * birdScale))
                {
                    if (!hit.collider.isTrigger)
                    {
                        AbortFlyToTarget();
                    }
                }
            }

            vectorDirectionToTarget = (target - transform.position).normalized;
            finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
            animator.SetFloat(animConfig.flyingDirectionHash, FindBankingAngle(transform.forward, vectorDirectionToTarget));
            transform.rotation = finalRotation;
            rigidbody.AddForce(transform.forward * flyingForce * Time.deltaTime);
            distanceToTarget = Vector3.Distance(transform.position, target);
            if (distanceToTarget <= 1.5f * birdScale)
            {
                solidCollider.enabled = false;
                if (distanceToTarget < 0.5f * birdScale)
                {
                    break;
                }
                else
                {
                    rigidbody.drag = 2.0f;
                    flyingForce = 50.0f * birdScale;
                }
            }
            else if (distanceToTarget <= 5.0f * birdScale)
            {
                rigidbody.drag = 1.0f;
                flyingForce = 50.0f * birdScale;
            }
            yield return 0;
        }

        animator.SetFloat(animConfig.flyingDirectionHash, 0);
        //initiate the landing for the bird to finally reach the target
        Vector3 vel = Vector3.zero;
        flying = false;
        landing = true;
        solidCollider.enabled = false;
        animator.SetBool(animConfig.landingBoolHash, true);
        animator.SetBool(animConfig.flyingBoolHash, false);
        t = 0.0f;
        rigidbody.velocity = Vector3.zero;

        //tell any birds that are in the way to move their butts
        Collider[] hitColliders = Physics.OverlapSphere(target, 0.05f * birdScale);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].tag == "lb_bird" && hitColliders[i].transform != transform)
            {
                hitColliders[i].SendMessage("FlyAway");
            }
        }

        //this while loop will reorient the rotation to vertical and translate the bird exactly to the target
        startingRotation = transform.rotation;
        transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, 0.0f);
        finalRotation = transform.rotation;
        transform.rotation = startingRotation;
        while (distanceToTarget > 0.05f * birdScale)
        {
            transform.rotation = Quaternion.Slerp(startingRotation, finalRotation, t * 4.0f);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, 0.5f);
            t += Time.deltaTime;
            distanceToTarget = Vector3.Distance(transform.position, target);
            if (t > 2.0f)
            {
                break;//failsafe to stop birds from getting stuck
            }
            yield return 0;
        }
        rigidbody.drag = .5f;
        rigidbody.velocity = Vector3.zero;
        animator.SetBool(animConfig.landingBoolHash, false);
        landing = false;
        transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, 0.0f);
        transform.position = target;
        animator.applyRootMotion = true;
        onGround = true;
    }

    //Sets a variable between -1 and 1 to control the left and right banking animation
    float FindBankingAngle(Vector3 birdForward, Vector3 dirToTarget)
    {
        Vector3 cr = Vector3.Cross(birdForward, dirToTarget);
        float ang = Vector3.Dot(cr, Vector3.up);
        return ang;
    }

    void AbortFlyToTarget()
    {
        StopCoroutine("FlyToTarget");
        solidCollider.enabled = false;
        animator.SetBool(animConfig.landingBoolHash, false);
        animator.SetFloat(animConfig.flyingDirectionHash, 0);
        transform.localEulerAngles = new Vector3(
            0.0f,
            transform.localEulerAngles.y,
            0.0f);
        FlyAway();
    }

    void FlyAway()
    {
        if (!dead)
        {
            StopCoroutine("FlyToTarget");
            animator.SetBool(animConfig.landingBoolHash, false);
            // controller.SendMessage("BirdFindTarget", gameObject);
        }
    }
}

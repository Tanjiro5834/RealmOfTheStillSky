using System.Collections;
using StillSky.Core;
using UnityEngine;
using UnityEngine.AI;

namespace StillSky.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyCharacter : Character
    {
        protected enum State { Idle, Chase, Attack, Dead }

        [Header("AI")]
        [SerializeField] protected float detectRange = 10f;
        [SerializeField] protected float attackRange = 1.8f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected float turnSpeed = 8f;
        [Tooltip("Seconds between path recalculations while chasing. SetDestination every frame is wasteful.")]
        [SerializeField] protected float repathInterval = 0.2f;

        [Header("Attack timing")]
        [Tooltip("Telegraph before the hit lands. This is Kael's dodge window.")]
        [SerializeField] protected float windupTime = 0.6f;
        [Tooltip("Keeps turning toward the target for this long, then commits to the swing direction.")]
        [SerializeField] protected float trackTime = 0.35f;
        [Tooltip("Pause after the swing. This is Kael's punish window.")]
        [SerializeField] protected float recoveryTime = 0.5f;
        [Tooltip("Width of the swing in degrees, centered on the enemy's forward.")]
        [SerializeField] protected float hitArc = 100f;
        [Tooltip("Swing reach as a multiple of attackRange, so a target that just stepped back still gets hit.")]
        [SerializeField] protected float reachMultiplier = 1.15f;

        protected State CurrentState { get; private set; } = State.Idle;
        protected Character Target { get; private set; }

        protected NavMeshAgent Agent { get; private set; }
        protected bool IsAttacking { get; private set; }

        private float nextAttackTime;
        private float nextRepathTime;
        private Coroutine attackRoutine;

        protected override void Awake()
        {
            base.Awake(); // Health + stats
            Agent = GetComponent<NavMeshAgent>();
            Agent.speed = stats != null ? stats.moveSpeed : 3.5f;
            Agent.stoppingDistance = attackRange * 0.9f; // stop just inside swing range
            Agent.updateRotation = false;                // we rotate ourselves (FaceTarget / FaceMovement)
        }

        protected virtual void Update()
        {
            if (!IsAlive) return;

            if (Target == null || !Target.IsAlive || !InRange(Target, detectRange))
                Target = FindTarget();

            CurrentState = DecideState();

            switch (CurrentState)
            {
                case State.Idle:   TickIdle();   break;
                case State.Chase:  TickChase();  break;
                case State.Attack: TickAttack(); break;
            }
        }

        protected virtual State DecideState()
        {
            if (IsAttacking) return State.Attack; // a committed swing always finishes
            if (Target == null) return State.Idle;
            return InRange(Target, attackRange) ? State.Attack : State.Chase;
        }

        /// <summary>Default: the closest living hostile character within detect range.</summary>
        protected virtual Character FindTarget()
        {
            Character best = null;
            float bestDist = detectRange;

            foreach (Character c in All)
            {
                if (!c.IsAlive || !IsHostileTo(c)) continue;

                float d = Vector3.Distance(transform.position, c.transform.position);
                if (d <= bestDist)
                {
                    best = c;
                    bestDist = d;
                }
            }
            return best;
        }

        protected virtual void TickIdle()
        {
            StopMoving();
        }

        protected virtual void TickChase()
        {
            if (!Agent.isOnNavMesh) return; // spawned off the mesh: SetDestination would throw

            Agent.isStopped = false;

            if (Time.time >= nextRepathTime)
            {
                Agent.SetDestination(Target.transform.position);
                nextRepathTime = Time.time + repathInterval;
            }

            FaceMovement();
        }

        protected virtual void TickAttack()
        {
            StopMoving();
            if (IsAttacking) return; // the routine controls facing during a swing

            FaceTarget();
            if (Time.time < nextAttackTime) return;

            attackRoutine = StartCoroutine(AttackRoutine(Target));
        }

        /// <summary>Windup (telegraph) -> hit check -> recovery.</summary>
        private IEnumerator AttackRoutine(Character target)
        {
            IsAttacking = true;
            OnWindupStart();

            for (float t = 0f; t < windupTime; t += Time.deltaTime)
            {
                if (t < trackTime && target != null)
                    FaceTowards(target.transform.position);
                yield return null;
            }

            OnWindupEnd();

            // The hit only lands if Kael is still in front and in reach: dodging out works, not just i-frames.
            if (target != null && target.IsAlive && IsInSwing(target))
                PerformAttack(target);

            yield return new WaitForSeconds(recoveryTime);

            IsAttacking = false;
            attackRoutine = null;
            nextAttackTime = Time.time + attackCooldown;
        }

        protected bool IsInSwing(Character target)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > attackRange * reachMultiplier) return false;
            return Vector3.Angle(transform.forward, toTarget) <= hitArc * 0.5f;
        }

        protected void CancelAttack()
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = null;

            if (IsAttacking) OnWindupEnd();
            IsAttacking = false;
        }

        /// <summary>The hit itself: damage, projectile, spell... each enemy type decides.</summary>
        protected abstract void PerformAttack(Character target);

        /// <summary>Telegraph hooks: color, sound, animation. Empty by default.</summary>
        protected virtual void OnWindupStart() { }
        protected virtual void OnWindupEnd() { }

        protected override void OnDeath()
        {
            CurrentState = State.Dead;
            Target = null;
            CancelAttack();
            StopMoving();
            Agent.enabled = false; // corpses don't path or push other agents
            GrantReward();
        }

        private void GrantReward()
        {
            if (stats == null || stats.emberReward <= 0 || LastDamageSource == null) return;

            var wallet = LastDamageSource.GetComponentInParent<EmberWallet>();
            if (wallet != null) wallet.Add(stats.emberReward);
        }

        protected bool InRange(Character c, float range) =>
            Vector3.Distance(transform.position, c.transform.position) <= range;

        protected void FaceTarget() => FaceTowards(Target.transform.position);

        protected void FaceTowards(Vector3 point)
        {
            Vector3 dir = point - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        protected void StopMoving()
        {
            if (Agent.isOnNavMesh && !Agent.isStopped)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }
            nextRepathTime = 0f; // repath immediately when the chase resumes
        }

        /// <summary>Turn toward where the agent is heading (follows the path around corners).</summary>
        protected void FaceMovement()
        {
            Vector3 dir = Agent.desiredVelocity;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) { FaceTarget(); return; }

            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Swing arc edges
            Gizmos.color = new Color(1f, 0.5f, 0f);
            float reach = attackRange * reachMultiplier;
            Vector3 left = Quaternion.Euler(0f, -hitArc * 0.5f, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, hitArc * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + left * reach);
            Gizmos.DrawLine(transform.position, transform.position + right * reach);
        }
    }
}
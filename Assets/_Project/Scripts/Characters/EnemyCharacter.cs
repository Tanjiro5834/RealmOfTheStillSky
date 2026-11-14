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

        protected State CurrentState { get; private set; } = State.Idle;
        protected Character Target { get; private set; }

        protected NavMeshAgent Agent { get; private set; }

        private float nextAttackTime;
        private float nextRepathTime;

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
            FaceTarget();
            if (Time.time < nextAttackTime) return;

            PerformAttack(Target);
            nextAttackTime = Time.time + attackCooldown;
        }

        protected abstract void PerformAttack(Character target);

        protected override void OnDeath()
        {
            CurrentState = State.Dead;
            Target = null;
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

        protected void FaceTarget()
        {
            Vector3 dir = Target.transform.position - transform.position;
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
        }
    }
}
using UnityEngine;

namespace StillSky.Characters
{
    /// <summary>
    /// Base for every enemy: a simple Idle -> Chase -> Attack state machine.
    /// Subclasses only decide HOW they attack (PerformAttack) and can override any Tick.
    /// </summary>
    public abstract class EnemyCharacter : Character
    {
        protected enum State { Idle, Chase, Attack, Dead }

        [Header("AI")]
        [SerializeField] protected float detectRange = 10f;
        [SerializeField] protected float attackRange = 1.8f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected float turnSpeed = 8f;

        protected State CurrentState { get; private set; } = State.Idle;
        protected Character Target { get; private set; }

        private float nextAttackTime;

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

        /// <summary>Default: the closest hostile character within detect range.</summary>
        protected virtual Character FindTarget()
        {
            Character best = null;
            float bestDist = detectRange;

            foreach (Character c in FindObjectsByType<Character>(FindObjectsSortMode.None))
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

        protected virtual void TickIdle() { }

        protected virtual void TickChase()
        {
            FaceTarget();
            float speed = stats != null ? stats.moveSpeed : 3.5f;
            // Straight-line chase for the slice; swap to NavMeshAgent once levels have walls.
            transform.position += transform.forward * (speed * Time.deltaTime);
        }

        protected virtual void TickAttack()
        {
            FaceTarget();
            if (Time.time < nextAttackTime) return;

            PerformAttack(Target);
            nextAttackTime = Time.time + attackCooldown;
        }

        /// <summary>Melee swing, arrow, spell... each enemy type decides.</summary>
        protected abstract void PerformAttack(Character target);

        protected override void OnDeath()
        {
            CurrentState = State.Dead;
            Target = null;
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

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
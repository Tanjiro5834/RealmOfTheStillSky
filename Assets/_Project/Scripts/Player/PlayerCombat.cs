using System;
using System.Collections;
using System.Collections.Generic;
using StillSky.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StillSky.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Serializable]
        public struct AttackData
        {
            public float damage;
            public float staminaCost;
            public float windup;    
            public float active;    
            public float recovery;  
            public float range;     
            public float radius;    
        }

        [Header("Input")]
        [SerializeField] private InputActionReference lightAttackAction;
        [SerializeField] private InputActionReference heavyAttackAction;

        [Header("References")]
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private Stamina stamina;
        [SerializeField] private Transform attackOrigin; 
        [SerializeField] private LayerMask hitMask = ~0; 

        [Header("Attacks")]
        [SerializeField] private AttackData[] lightCombo =
        {
            new AttackData { damage = 12f, staminaCost = 12f, windup = 0.12f, active = 0.10f, recovery = 0.25f, range = 1.4f, radius = 0.7f },
            new AttackData { damage = 14f, staminaCost = 12f, windup = 0.10f, active = 0.10f, recovery = 0.25f, range = 1.4f, radius = 0.7f },
            new AttackData { damage = 20f, staminaCost = 16f, windup = 0.18f, active = 0.12f, recovery = 0.40f, range = 1.6f, radius = 0.8f },
        };
        [SerializeField] private AttackData heavyAttack =
            new AttackData { damage = 35f, staminaCost = 30f, windup = 0.45f, active = 0.15f, recovery = 0.55f, range = 1.7f, radius = 0.9f };
        [SerializeField] private float comboWindow = 0.5f;

        public bool IsAttacking { get; private set; }
        public event Action<int> LightAttackStarted;
        public event Action HeavyAttackStarted;

        private int comboIndex;
        private float lastAttackEndTime = float.NegativeInfinity;
        private bool lightQueued;

        private void OnEnable()
        {
            lightAttackAction.action.Enable();
            heavyAttackAction.action.Enable();
            lightAttackAction.action.performed += OnLightAttack;
            heavyAttackAction.action.performed += OnHeavyAttack;
        }

        private void OnDisable()
        {
            lightAttackAction.action.performed -= OnLightAttack;
            heavyAttackAction.action.performed -= OnHeavyAttack;
            lightAttackAction.action.Disable();
            heavyAttackAction.action.Disable();

            StopAllCoroutines();
            IsAttacking = false;
            if (motor != null) motor.InputLocked = false;
        }

        private void OnLightAttack(InputAction.CallbackContext _)
        {
            if (dodge != null && dodge.IsDodging) return;

            if (IsAttacking)
            {
                lightQueued = true;
                return;
            }

            StartLightAttack();
        }

        private void OnHeavyAttack(InputAction.CallbackContext _)
        {
            if (IsAttacking || (dodge != null && dodge.IsDodging)) return;
            if (stamina != null && !stamina.TryUse(heavyAttack.staminaCost)) return;

            comboIndex = 0;
            HeavyAttackStarted?.Invoke();
            StartCoroutine(AttackRoutine(heavyAttack));
        }

        private void StartLightAttack()
        {
            if (Time.time - lastAttackEndTime > comboWindow)
                comboIndex = 0;

            AttackData data = lightCombo[comboIndex];
            if (stamina != null && !stamina.TryUse(data.staminaCost)) return;

            LightAttackStarted?.Invoke(comboIndex);
            comboIndex = (comboIndex + 1) % lightCombo.Length;
            StartCoroutine(AttackRoutine(data));
        }

        private IEnumerator AttackRoutine(AttackData data)
        {
            IsAttacking = true;
            lightQueued = false;
            if (motor != null) motor.InputLocked = true;

            yield return new WaitForSeconds(data.windup);

            var alreadyHit = new HashSet<IDamageable>();
            for (float t = 0f; t < data.active; t += Time.deltaTime)
            {
                HitCheck(data, alreadyHit);
                yield return null;
            }

            yield return new WaitForSeconds(data.recovery);

            IsAttacking = false;
            if (motor != null) motor.InputLocked = false;
            lastAttackEndTime = Time.time;

            if (lightQueued)
            {
                lightQueued = false;
                StartLightAttack();
            }
        }

        private void HitCheck(AttackData data, HashSet<IDamageable> alreadyHit)
        {
            Vector3 center = attackOrigin.position + attackOrigin.forward * data.range;
            Collider[] hits = Physics.OverlapSphere(center, data.radius, hitMask, QueryTriggerInteraction.Ignore);

            foreach (Collider col in hits)
            {
                if (col.transform.root == transform.root) continue;

                var target = col.GetComponentInParent<IDamageable>();
                if (target == null || !alreadyHit.Add(target)) continue;

                target.TakeDamage(data.damage, gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackOrigin == null || lightCombo == null || lightCombo.Length == 0) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position + attackOrigin.forward * lightCombo[0].range, lightCombo[0].radius);
        }
    }
}
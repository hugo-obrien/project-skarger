using System;
using UnityEngine;

namespace _Project.Scripts.Units.Components
{
    /// <summary>
    /// Responsibility: solely tracking unit health (SRP). 
    /// It is unaware of animation, physics, or UI—it triggers events
    /// that other components respond to. 
    /// </summary>
    public class UnitHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;

        private float currentHealth;
        private bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        public event Action Dying;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void Initialize(float newMaxHealth)
        {
            if (isDead || newMaxHealth <= 0f) return;

            float previousMax = maxHealth;
            maxHealth = newMaxHealth;

            if (previousMax > 0f)
            {
                currentHealth = Mathf.Clamp(currentHealth / previousMax * maxHealth, 0f, maxHealth);
            }
            else
            {
                currentHealth = maxHealth;
            }
        }

        public void TakeDamage(float amount, Vector3? impactDirection = null)
        {
            if (isDead || amount <= 0f) return;

            currentHealth -= amount;
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die(impactDirection);
            }
        }

        public void Heal(float amount)
        {
            if (isDead || amount <= 0f) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        private void Die(Vector3? impactDirection)
        {
            if (isDead) return;
            isDead = true;
            Dying?.Invoke();
        }
    }
}
using System;
using UnityEngine;

namespace _Project.Scripts.Units {

    /// <summary>
    /// Ответственность: только учёт здоровья юнита (SRP).
    /// Не знает ни об анимации, ни о физике, ни об UI — пробуждает события,
    /// на которые реагируют другие компоненты.
    /// </summary>
    public class UnitHealth : MonoBehaviour {

        [SerializeField] private float maxHealth = 100f;

        private float currentHealth;
        private bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        /// <summary>Вызывается при переходе в мёртвое состояние (до применения урона, убившего юнита).</summary>
        public event Action Dying;

        private void Awake() {
            currentHealth = maxHealth;
        }

        /// <summary>Инициализация из конфигурации юнита. Вызывается один раз из Unit.</summary>
        public void Initialize(float newMaxHealth) {
            if (isDead || newMaxHealth <= 0f) return;

            float previousMax = maxHealth;
            maxHealth = newMaxHealth;

            // Пропорционально сохраняем текущий процент здоровья.
            if (previousMax > 0f) {
                currentHealth = Mathf.Clamp(currentHealth / previousMax * maxHealth, 0f, maxHealth);
            } else {
                currentHealth = maxHealth;
            }
        }

        public void TakeDamage(float amount, Vector3? impactDirection = null) {
            if (isDead || amount <= 0f) return;

            currentHealth -= amount;
            if (currentHealth <= 0f) {
                currentHealth = 0f;
                Die(impactDirection);
            }
        }

        public void Heal(float amount) {
            if (isDead || amount <= 0f) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        private void Die(Vector3? impactDirection) {
            if (isDead) return;
            isDead = true;

            Dying?.Invoke();
        }
    }
}

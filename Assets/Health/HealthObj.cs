using System;
using UnityEngine;

namespace Health
{
    [RequireComponent(typeof(HealthObj))]
    public class HealthObj : MonoBehaviour
    {
        private HealthBar healthBar;
        public GameObject healthBarPrefab;
        public bool IsImmortal = false;
        public void Start()
        {
            CurrentHealthPoints = maxHealthPoints;
            healthBar = Instantiate(healthBarPrefab, transform).GetComponent<HealthBar>();
            healthBar.transform.position += Vector3.down;
            healthBar.SetUp(this);
        
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler OnHealthChanged;
        public event EventHandler OnDeath;

        public int CurrentHealthPoints;
        public int maxHealthPoints;

        public float GetHealthPercentage()
        {
            if (maxHealthPoints == 0)
                return 0;
            return (float) CurrentHealthPoints / maxHealthPoints;
        }

        public virtual void Damage(int points)
        {
            if (IsImmortal)
                return;
            CurrentHealthPoints = ToHealthInBounds(CurrentHealthPoints - Math.Abs(points));
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Heal(int points)
        {
            CurrentHealthPoints = ToHealthInBounds(CurrentHealthPoints + Math.Abs(points));
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
        }

        private int ToHealthInBounds(int healthPoints)
        {
            if (healthPoints <= 0)
            {
                OnDeath?.Invoke(this, EventArgs.Empty);
                return 0;
            }

            return healthPoints > maxHealthPoints ? maxHealthPoints : healthPoints;
        }
    }
}

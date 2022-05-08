using System;
using Unity.Mathematics;
using UnityEngine;
using Weapon;

namespace Player
{
	public class Character : MonoBehaviour
	{
		public int currentHealth = 0;
		//public GameObject healthObjPrefab;
		public Weapon.Weapon weapon;
		public HealthObj health;
		public Rigidbody2D rb;
		public SpriteRenderer sprite;
		[SerializeField] protected GameObject weaponPrefab;
		[SerializeField] public Collider2D characterCollider;
		public PlayerState State { get; set; }
        
		private void Awake()
		{
			//health = Instantiate(healthObjPrefab, transform).GetComponent<HealthObj>();
			State = PlayerState.Normal;
			sprite = rb.GetComponent<SpriteRenderer>();
			grid = GameObject.Find("GridActualUnity").GetComponent<GridObj>();
		}
		public void Update()
		{
			if (health == null)
				return;
			if (health.Health.CurrentHealthPoints <= 0) 
				Die();
			currentHealth = health.Health.CurrentHealthPoints;
			UpdateEyeDirection();
		}

		private void FixedUpdate()
		{
			UpdateGrid();
		}

		private void Die()
		{
			State = PlayerState.Dead;
		}
        
		private void UpdateEyeDirection()
		{
			sprite.flipX = (int) Mathf.Sign(-rb.velocity.x) == 1;
		}
		
		
		public GridObj grid;
		public int2 gridPosition;


		public void UpdateGrid()
		{
			var newGridPosition = grid.WorldToGridPosition(transform.position);
			if (newGridPosition.x == gridPosition.x && newGridPosition.y == gridPosition.y) return;
			grid.UnFillCell(gridPosition);
			grid.FillCell(newGridPosition);
			gridPosition = newGridPosition;
			grid.PlayerPosition = transform.position;
		}

	}
}
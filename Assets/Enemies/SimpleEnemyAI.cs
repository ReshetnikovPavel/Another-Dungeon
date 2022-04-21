using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions;
using GridTools;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleEnemyAI : MonoBehaviour
{
    public HealthObj Health;
    public Rigidbody2D Rb;
    public Weapon.Weapon Weapon;
    private float latestAimAngle;
    private Stage currentStage;

    public GridObj Grid;
    private PathFinding pathFinder;
    private Vector3 startingPosition;
    private Vector3 roamPosition;
    private Vector3 nextTarget;
    private int nextTargetIndex;
    private Vector3 direction;

    private List<int2> path;

    private const float pauseTime = 1f;
    private float pauseStart;

    private float moveSpeed;

    private enum Stage
    {
        None,
        SearchingPath,
        Moving,
        Pause
    }

    private void Start()
    {
        pathFinder = new PathFinding();
        Health = gameObject.AddComponent<HealthObj>();
       
        currentStage = Stage.None;
        moveSpeed = 5f;
    }

    private void FixedUpdate()
    {
        if (Health.Health.CurrentHealthPoints <= 0)
            Die();
        switch (currentStage)
        {
            case Stage.None:
                currentStage = Stage.SearchingPath;
                StartSearchNextTarget();
                break;
            case Stage.SearchingPath:
                break;
            case Stage.Moving:
                MoveToNextTarget();
                break;
            case Stage.Pause:
                var difference = Time.time - pauseStart;
                if (difference >= pauseTime)
                    currentStage = Stage.None;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Task<List<int2>> FindPath(int2 startGridPosition, int2 endGridPosition)
    {
        var task = new Task<List<int2>>(() => 
            pathFinder.FindPathAStar(Grid.Grid, startGridPosition, endGridPosition));

        task.Start();
        return task;
    }

    private async void StartSearchNextTarget()
    {
        UpdateTarget();

        var startGridPosition = Grid.WorldToGridPosition(startingPosition);
        var endGridPosition = Grid.WorldToGridPosition(roamPosition);

        var originalPath = await FindPath(startGridPosition, endGridPosition);

        if (originalPath is null)
            currentStage = Stage.None;
        else
        {
            path = PathFinding.GetClearPath(originalPath);
            Grid.AddPathsToDraw(path);

            nextTargetIndex = 0;
            UpdateNextTarget();
        }
    }

    private void MoveToNextTarget()
    {
        UpdateAim();
        var distanceToNextTarget = transform.position.DistanceTo(nextTarget);

        Rb.velocity = direction * moveSpeed;
        
        if (distanceToNextTarget >= moveSpeed * Time.fixedDeltaTime)
            return;
        
        if (nextTargetIndex == path.Count - 1)
        {
            Rb.velocity = Vector2.zero;
            currentStage = Stage.Pause;
            pauseStart = Time.time;
        }

        UpdateNextTarget();
    }

    private void UpdateNextTarget()
    {
        for (var i = path.Count - 1; i > nextTargetIndex; i--)
        {
            var target = Grid.GridToWorldPosition(path[i]).ToVector3();
            var distance = transform.position.DistanceTo(target);
            var currentDirection = (target - transform.position).normalized;

            var ray = Physics2D.Raycast(transform.position.ToVector2(), currentDirection.ToVector2(), distance, LayerMask.GetMask("Walls"));

            if (ray.collider != null)
                continue;
            
            nextTargetIndex = i;
            nextTarget = target;
            currentStage = Stage.Moving;
            break;
        }
    }

    private void UpdateTarget()
    {
        startingPosition = transform.position;
        roamPosition = GetRandomPosition();
        direction = (roamPosition - startingPosition).normalized;
    }

    private void UpdateAim()
    {
        direction = (nextTarget - transform.position).normalized;
        var aimAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Weapon.weaponPrefab.transform.RotateAround(Rb.position, Vector3.forward, aimAngle - latestAimAngle);
        latestAimAngle = aimAngle;
    }

    private Vector3 GetRandomPosition()
        => startingPosition + Tools.GetRandomDir() * Random.Range(10f, 70f);
    
    private void Die()
        => Destroy(gameObject);
}

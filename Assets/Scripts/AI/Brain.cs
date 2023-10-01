using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public enum Phase
{
    Patrol,
    Paranoia,
    Stalk,
    Pursuit,
    Cooldown
}

public class Brain : MonoBehaviour
{
    public Camera playerCamera;
    public Transform player;
    public NavMeshObstacle playerObstacle;
    private NavMeshAgent monster;

    public GameObject enemyPatrol;
    private List<Transform> patrolPoints;

    public float MaxViewDistanceWithFlashlight = 8.5f;
    public float MaxViewDistanceWithoutFlashlight = 2.5f;

    private float maxViewDistance = 5f;

    [SerializeField]
    private Phase phase;

    private bool isMonsterWaiting = false;
    private bool isMonsterSeen = false;

    private bool isRunningAwayFromPlayer = false;
    private bool isAttackingPlayer = false;

    private bool hasMonsterBeenSeenBefore = false;

    void Start()
    {
        monster = GetComponent<NavMeshAgent>();

        // Get all patrol points from parent enemy patrol object.
        patrolPoints = new List<Transform>();
        foreach (Transform destination in enemyPatrol.transform)
            patrolPoints.Add(destination);

        phase = Phase.Patrol;
        monster.destination = patrolPoints[0].position;
    }

    // 5 Phases

    /** 1. Patrol Phase
     *  Enemy will slowly move between various points in the level.
     *  If the player sees it, it will turn towards the player, scream at them, then run away, (disappear after being out of sight).
     */

    /** 2. Paranoia Phase
     *  After the player sees the monster in the patrol phase, the enemy transitions into the paranoia phase.
     *  In this phase, the enemy will be randomly placed in a room the player is not already in.
     *  The enemy will typically be hidden in some pre-planned spots.
     *  After the player shines the light on the enemy, they will scurry away out of the room.
     */

    /** 3. Stalk Phase
     * After the player sees the monster, the monster will start to stalk the player.
     * This is where the flashlight becomes important for the player.
     * 
     * While the player has their flashlight on, the enemy will stalk them.
     * 
     * Nothing happens except for the occasional ambient noise behind the player.
     */

    /** 4. Active Pursuit Phase
     * 
     * While the player has their flashlight off, the enemy will look to engage with the player. 
     * If they have their flashlight off for too long (1 minute), the enemy will start to pursue the player,
     * ignoring whether the flashlight is on/off.
     * A chase ensues, and the player need to use their stamina bar to outrun the enemy.
     * If the player runs into a candle-light area, the enemy runs away, going to a 1 minute cooldown phase.
     * Once the enemy is manually out-ran, it will go back to a 3 minute cooldown phase.
     */

    /** 5. Cooldown Phase
     * The enemy does not appear anywhere for 2 minutes.
     * After the 2 minutes is up, it goes back to the paranoia phase.
     */

    void Update()
    {
        PerformPhase();
        UpdateEnemySeen();
    }

    void PerformPhase()
    {
        if (isRunningAwayFromPlayer)
        {
            float currentViewDistance = FlashlightController.instance.isFlashlightOn ? MaxViewDistanceWithFlashlight : MaxViewDistanceWithoutFlashlight;
            if (!PerformSeenCheck(currentViewDistance + 4f))
            {
                monster.ResetPath();
                playerObstacle.enabled = false;
                isRunningAwayFromPlayer = false;
                Debug.Log("No longer seen, no longer running away.");
                return;
            }

            if (monster.remainingDistance <= monster.stoppingDistance + 0.1f)
                RunAwayOrAttackPlayer();
            return;
        }

        if (isAttackingPlayer)
        {
            if (monster.remainingDistance <= monster.stoppingDistance + 0.1f)
            {
                monster.ResetPath();
                isAttackingPlayer = false;
            }

            return;
        }

        switch (phase)
        {
            case Phase.Patrol:
                Patrol();
                break;
            case Phase.Paranoia:
                Paranoia();
                break;
            case Phase.Stalk:
                Stalk();
                break;
            case Phase.Pursuit:
                Pursuit();
                break;
            case Phase.Cooldown:
                CooldownPhase();
                break;
            default:
                break;
        }
    }

    void UpdateEnemySeen()
    {
        float currentViewDistance = FlashlightController.instance.isFlashlightOn ? MaxViewDistanceWithFlashlight : MaxViewDistanceWithoutFlashlight;
        isMonsterSeen = PerformSeenCheck(currentViewDistance);
        if (isMonsterSeen && !hasMonsterBeenSeenBefore)
        {
            hasMonsterBeenSeenBefore = true;
            
            // Stop the monster
            monster.destination = transform.position;

            RunAwayOrAttackPlayer();

            // Transition to Paranoia Phase
            phase = Phase.Paranoia;
        }
    }

    bool PerformSeenCheck(float maxViewDistance)
    {
        // Viewport Check
        Vector3 viewPos = playerCamera.WorldToViewportPoint(gameObject.transform.position);
        bool isInViewport = viewPos.x >= 0.1f && viewPos.x <= 0.9f && viewPos.y >= 0.1f && viewPos.y <= 0.9f && viewPos.z > 0.1f;

        // Perform the raycast
        RaycastHit hitInfo;
        Vector3 direction = (gameObject.transform.position - playerCamera.transform.position).normalized;

        bool debug = false;
        if (debug)
        {
            Debug.DrawRay(playerCamera.transform.position, direction * maxViewDistance, Color.red, 0.1f);
            Debug.Log(isMonsterSeen ? "Player can see enemy." : "Player CANNOT see enemy.");
        }

        // Seen Check
        return
            isInViewport &&
            Physics.Raycast(playerCamera.transform.position, direction, out hitInfo, maxViewDistance) &&
            hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy");
    }

    // Navigate Monster to nearest patrol point which is out of sight from the player.
    void RunAwayOrAttackPlayer()
    {
        // Make it sprint away
        monster.speed = 50f;

        // Sort by ascending order of patrol points
        SortPatrolPoints();

        // Set the destination to be the farthest point away from the player.
        bool isValidPathForRunningAway = monster.SetDestination(patrolPoints[patrolPoints.Count - 1].position);
        if (!isValidPathForRunningAway)
        {
            isRunningAwayFromPlayer = false;
            isAttackingPlayer = true;

            playerObstacle.enabled = false;
            monster.destination = player.transform.position;
        }
        else
        {
            isRunningAwayFromPlayer = true;
            isAttackingPlayer = false;

            // Enable player as an obstacle
            playerObstacle.enabled = true;
        }
    }

    void Patrol()
    {
        // Bounds Check
        if (monster.remainingDistance <= monster.stoppingDistance + 0.1f)
        {
            PickNextPatrolDestination();
        }
    }

    // Picks a new patrol point based on the 
    private void PickNextPatrolDestination()
    {
        if (isMonsterWaiting) return;

        SortPatrolPoints();

        // 70% of the time, we pick the next closest point.
        Vector3 newDestination = patrolPoints[Random.Range(0, 4)].position;

        // But 20% of the time, we pick a new destination really far away from the enemy.
        if (Random.Range(1, 10) <= 2)
        {
            newDestination = patrolPoints[Random.Range(patrolPoints.Count - 5, patrolPoints.Count - 1)].position;
            Debug.Log("Moving to FARAWAY position.");
        }

        // 10% of the time, we tell the monster to wait in a position for 5 seconds.
        if (Random.Range(1, 10) == 1)
        {
            isMonsterWaiting = true;
            newDestination = monster.destination;
            StartCoroutine(PatrolWait());
        }

        monster.SetDestination(newDestination);
        Debug.Log(monster.destination == newDestination ? "Staying in the same place." : "Moving to new position.");
    }

    IEnumerator PatrolWait()
    {
        int randomWaitTime = Random.Range(2, 6);
        Debug.Log("Waiting for " + " seconds.");
        yield return new WaitForSecondsRealtime(randomWaitTime);
        isMonsterWaiting = false;
    }

    void SortPatrolPoints()
    {
        patrolPoints.Sort((a, b) =>
        {
            float distance1 = Vector3.Distance(a.position, gameObject.transform.position);
            float distance2 = Vector3.Distance(b.position, gameObject.transform.position);

            if (distance1 < distance2)
                return -1;
            else if (distance1 > distance2)
                return 1;
            else
                return 0;
        });
    }

    void Paranoia()
    {
        Debug.Log("In paranoid phase...");
    }
    void Stalk()
    {

    }
    void Pursuit()
    {

    }
    void CooldownPhase()
    {

    }
}

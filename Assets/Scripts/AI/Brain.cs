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

public enum CooldownReason
{
    None,
    LostPlayerAfterPursuit,
    SuccessfullyAttackedPlayer,
}

public class Brain : MonoBehaviour
{
    public Camera playerCamera;
    public Transform player;
    public MeshRenderer monsterRenderer;
    private NavMeshAgent monsterAgent;

    public GameObject enemyPatrol;
    public GameObject enemyParanoia;
    private List<Transform> patrolPoints;
    private List<Transform> paranoiaPoints;

    public float MaxViewDistanceWithFlashlight = 8.5f;
    public float MaxViewDistanceWithoutFlashlight = 2.5f;

    public float defaultSpeed = 3f;
    public float defaultAcceleration = 8f;

    private float maxViewDistance = 5f;

    [SerializeField]
    private Phase phase;

    private bool isMonsterWaiting = false;
    private bool isMonsterSeen = false;
    private bool paranoiaSet = false;

    private bool isRunningAwayFromPlayer = false;
    private bool isFadingIntoWall = false;
    private Coroutine fadeIntoWallCoroutine = null;

    private bool hasMonsterBeenSeenBefore = false;
    private bool hasMonsterBeenSeenBefore2 = false;

    public GameObject stalkAmbienceObject;
    private AudioSource[] stalkAmbience;
    private Coroutine ambientMonsterNoiseCoroutine = null;

    public GameObject farScreamObject;
    private AudioSource[] farScreamSources;

    public GameObject closeScreamObject;
    private AudioSource[] closeScreamSources;

    public AudioSource monsterPresence;


    private float timeLeftToPursuitMax = 10f;
    private float timeLeftToPursuit;

    private bool hasPursuitStarted = false;
    private bool hasFoundPlayerInPursuit = false;
    private float currentPursuitTime = 0f;
    private CooldownReason latestCooldownReason = CooldownReason.None;

    private Coroutine cooldownStarted = null;

    void Start()
    {
        monsterAgent = GetComponent<NavMeshAgent>();

        stalkAmbience = stalkAmbienceObject.GetComponents<AudioSource>();
        farScreamSources = farScreamObject.GetComponents<AudioSource>();
        closeScreamSources = closeScreamObject.GetComponents<AudioSource>();

        timeLeftToPursuit = timeLeftToPursuitMax;

        // Get all patrol points from parent enemy patrol object.
        patrolPoints = new List<Transform>();
        paranoiaPoints = new List<Transform>();
        foreach (Transform destination in enemyPatrol.transform)
            patrolPoints.Add(destination);
        foreach (Transform destination in enemyParanoia.transform)
            paranoiaPoints.Add(destination);

        phase = Phase.Patrol;
        monsterAgent.destination = patrolPoints[0].position;
        monsterPresence.Play();
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
                monsterAgent.ResetPath();
                isRunningAwayFromPlayer = false;

                monsterAgent.enabled = false;
                monsterAgent.speed = defaultSpeed;
                monsterAgent.acceleration = defaultAcceleration;
                monsterAgent.velocity = Vector3.zero;

                monsterPresence.Stop();

                Debug.Log("No longer seen, no longer running away.");
                return;
            }

            if (monsterAgent.remainingDistance <= monsterAgent.stoppingDistance + 0.1f)
                RunAwayFromPlayer();
            return;
        }

        if (isFadingIntoWall)
        {
            if (fadeIntoWallCoroutine == null)
                fadeIntoWallCoroutine = StartCoroutine(FadeOpacity());
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
            
            // Stop the monsterf
            monsterAgent.destination = transform.position;

            RunAwayFromPlayer();

            // Transition to Paranoia Phase
            phase = Phase.Paranoia;
            return;
        }

        if (isMonsterSeen && hasMonsterBeenSeenBefore && !isRunningAwayFromPlayer && !hasMonsterBeenSeenBefore2)
        {
            hasMonsterBeenSeenBefore2 = true;

            // Play a loud violin noise, fade the monster into the wall.
            closeScreamSources[Random.Range(0, closeScreamSources.Length - 1)].Play();

            isFadingIntoWall = true;
        }
    }

    bool PerformSeenCheck(float maxViewDistance)
    {
        // Viewport Check
        Vector3 viewPos = playerCamera.WorldToViewportPoint(gameObject.transform.position);
        bool isInViewport = viewPos.x >= 0.1f && viewPos.x <= 0.9f && viewPos.y >= 0.1f && viewPos.y <= 0.9f && viewPos.z > 0.1f;

        Vector3[] directions = {
            (gameObject.transform.position - new Vector3(-0.8f, 1f, -0.8f) - playerCamera.transform.position).normalized,
            (gameObject.transform.position - new Vector3(-0.8f, 1f, 0.8f) - playerCamera.transform.position).normalized,
            (gameObject.transform.position - new Vector3(0.8f, 1f, -0.8f) - playerCamera.transform.position).normalized,
            (gameObject.transform.position - new Vector3(0.8f, 1f, 0.8f) - playerCamera.transform.position).normalized,

            (gameObject.transform.position - new Vector3(-0.8f, -1f, -0.8f) - playerCamera.transform.position).normalized,
            (gameObject.transform.position - new Vector3(-0.8f, -1f, 0.8f) - playerCamera.transform.position).normalized,
            (gameObject.transform.position - new Vector3(0.8f, -1f, -0.8f) - playerCamera.transform.position).normalized,
            (gameObject.transform.position - new Vector3(0.8f, -1f, 0.8f) - playerCamera.transform.position).normalized,
        };

        // Perform the raycast
        RaycastHit hitInfo;

        bool debug = false;
        if (debug)
        {
            foreach (Vector3 dir in directions)
                Debug.DrawRay(playerCamera.transform.position, dir * maxViewDistance, Color.red, 0.1f);
        }

        // Seen Check
        if (isInViewport)
        {
            foreach (Vector3 dir in directions)
            {
                if (Physics.Raycast(playerCamera.transform.position, dir, out hitInfo, maxViewDistance)
                    && hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Navigate Monster to nearest patrol point which is out of sight from the player.
    void RunAwayFromPlayer()
    {
        // Make it sprint away
        monsterAgent.speed = 50f;
        monsterAgent.acceleration = 16f;

        // Sort by ascending order of patrol points
        SortPatrolPoints();

        // Set the destination to be the farthest point away from the player.
        monsterAgent.SetDestination(patrolPoints[patrolPoints.Count - 1].position);
        isRunningAwayFromPlayer = true;

        Debug.Log("Running Away.");
    }
    IEnumerator FadeOpacity()
    {
        float maxTime = 2f;
        float time = 0;
        float a = 1;
        float r = monsterRenderer.material.color.r;
        float g = monsterRenderer.material.color.g;
        float b = monsterRenderer.material.color.b;
        Debug.Log("Fading Opacity.");

        while (time < maxTime)
        {
            monsterRenderer.material.color = new Color(r, g, b, a -= (Time.deltaTime / maxTime));
            time += Time.deltaTime;
            yield return null;
        }
        monsterRenderer.material.color = new Color(r, g, b, 0);
        transform.position = new Vector3(0, -1000000, 0);
        phase = Phase.Stalk;
        isFadingIntoWall = false;
        fadeIntoWallCoroutine = null;
    }

    void Patrol()
    {
        // Bounds Check
        if (monsterAgent.remainingDistance <= monsterAgent.stoppingDistance + 0.1f)
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
            newDestination = monsterAgent.destination;
            StartCoroutine(PatrolWait());
        }

        monsterAgent.SetDestination(newDestination);
        Debug.Log(monsterAgent.destination == newDestination ? "Staying in the same place." : "Moving to new position.");
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

    void SortPoints(List<Transform> points, Vector3 startingPosition)
    {
        points.Sort((a, b) =>
        {
            float distance1 = Vector3.Distance(a.position, startingPosition);
            float distance2 = Vector3.Distance(b.position, startingPosition);

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
        if (!paranoiaSet)
        {
            int randomParanoiaPoint = Random.Range(0, paranoiaPoints.Count - 1);
            transform.position = paranoiaPoints[randomParanoiaPoint].transform.position;
            paranoiaSet = true;
            Debug.Log("Paranoia Set to: ");
            Debug.Log(paranoiaPoints[randomParanoiaPoint].transform.position);
        }
    }
    void Stalk()
    {
        Debug.Log("Stalking.");
        if (FlashlightController.instance.isFlashlightOn)
        {
            // Regenerate Sanity 3x faster then you lose it.
            if (timeLeftToPursuit + Time.deltaTime < timeLeftToPursuitMax)
                timeLeftToPursuit += Time.deltaTime * 3f;

            if (ambientMonsterNoiseCoroutine == null)
                ambientMonsterNoiseCoroutine = StartCoroutine(MaybePlayAmbientMonsterNoises());
            return;
        }

        // If the player is off, we stop playing random ambient noises.
        if (ambientMonsterNoiseCoroutine != null)
            StopCoroutine(ambientMonsterNoiseCoroutine);

        // Flashlight is off, start counting down the time.
        if (timeLeftToPursuit <= 0f)
        {
            AudioController.instance.RequestAmbienceStop(5f);
            Debug.Log("Pursuing Player.");
            phase = Phase.Pursuit;
            timeLeftToPursuit = timeLeftToPursuitMax;
        }
        else
        {
            timeLeftToPursuit -= Time.deltaTime;
            Debug.Log("Current time left to pursuit: " + timeLeftToPursuit);
        }
    }

    IEnumerator MaybePlayAmbientMonsterNoises()
    {
        while (true)
        {
            // Play an random ambient noise while the flashlight is on, approximately once a minute.
            if (Random.Range(0, 5) == 0)
            {
                stalkAmbience[Random.Range(0, stalkAmbience.Length - 1)].Play();
            }
            yield return new WaitForSecondsRealtime(10);
        }
    }

    void Pursuit()
    {
        // Reset the material color if transparent.
        if (monsterRenderer.material.color.a == 0)
        {
            monsterRenderer.material.color = new Color(
               monsterRenderer.material.color.r,
               monsterRenderer.material.color.g,
               monsterRenderer.material.color.b,
               1f);
        }

        // Start the presence audio.
        if (!hasPursuitStarted)
        {
            monsterPresence.Play();

            // Play Blood Curdling Scream to let the player know to ... RUN.
            farScreamSources[Random.Range(0, farScreamSources.Length - 1)].Play();

            // Request Pursuit BGM.
            AudioController.instance.RequestPursuitStart(2f);

            // Request the player to start heavily breathing.
            AudioController.instance.RequestPlayerBreathingLongStart();

            // Spawn Monster.
            if (!monsterAgent.enabled)
                SpawnInMonsterForPursuit();

            hasPursuitStarted = true;
        }

        float remainingMonsterDistance = GetPathRemainingDistance();

        // Check if monster has found player yet
        if (!monsterAgent.pathPending && remainingMonsterDistance < MaxViewDistanceWithFlashlight)
        {
            hasFoundPlayerInPursuit = true;

            // We don't want the chase to end if the monster is about to
            // catch the player. Therefore if the monster is within 8.5f of
            // the player, reset the pursuit time to 15 seconds until
            // they are out of the range.
            if (currentPursuitTime < 15f)
                currentPursuitTime = 15f;
        }

        // Attack if the monster catches up to the player
        if (!monsterAgent.pathPending && remainingMonsterDistance < 1f)
        {
            // Play Attack Sound
            Debug.Log("Attacking player and playing sound.");
            AudioController.instance.RequestPlayerHitSound();

            // Run away after attacking.
            Debug.Log("Running away after attacking.");
            AudioController.instance.RequestPlayerBreathingLongStop(1.5f);
            latestCooldownReason = CooldownReason.SuccessfullyAttackedPlayer;
            phase = Phase.Cooldown;
            return;
        }

        // Run away if the monster is too far away from the player after catching up with them once.
        if (hasFoundPlayerInPursuit)
        {
            if (!monsterAgent.pathPending && remainingMonsterDistance > 20f)
            {
                Debug.Log("Running away after losing player.");
                AudioController.instance.RequestPlayerBreathingLongStop(3f);
                latestCooldownReason = CooldownReason.LostPlayerAfterPursuit;
                phase = Phase.Cooldown;
                return;
            }
        }

        if (currentPursuitTime > 30f)
        {
            Debug.Log("Pursuit timed out after lasting 30 seconds.");
            AudioController.instance.RequestPlayerBreathingLongStop(3f);
            latestCooldownReason = CooldownReason.LostPlayerAfterPursuit;
            phase = Phase.Cooldown;
            return;
        }

        monsterAgent.SetDestination(player.transform.position);
        currentPursuitTime += Time.deltaTime;
    }

    void SpawnInMonsterForPursuit()
    {
        List<Transform> possibleSpawnLocations = new List<Transform>();

        // Filter out all possible spawns based on whether they are in the viewport.
        Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        foreach (Transform point in patrolPoints)
        {
            Bounds pointBounds = new Bounds(point.position, new Vector3(1, 1, 1));
            if (!GeometryUtility.TestPlanesAABB(cameraFrustum, pointBounds))
            {
                possibleSpawnLocations.Add(point);
            }
        }

        // Sort all points by distance from player.
        SortPoints(possibleSpawnLocations, player.transform.position);

        // Pick the one a middle distance away.
        if (possibleSpawnLocations.Count > 0)
        {
            transform.position = possibleSpawnLocations[(possibleSpawnLocations.Count / 2) - 1].position;
            monsterAgent.enabled = true;
            monsterAgent.SetDestination(player.transform.position);
        }
        else
        {
            Debug.Log("Fatal Error: No Spawn Points for Pursuit Phase.");
        }
    }

    private float GetPathRemainingDistance()
    {
        if (monsterAgent.pathPending ||
            monsterAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
            monsterAgent.path.corners.Length == 0)
            return -1f;

        float distance = 0.0f;
        for (int i = 0; i < monsterAgent.path.corners.Length - 1; ++i)
            distance += Vector3.Distance(monsterAgent.path.corners[i], monsterAgent.path.corners[i + 1]);

        return distance;
    }

    void CooldownPhase()
    {
        if (cooldownStarted == null)
        {
            Debug.Log("In Cooldown Phase");

            // Reset for Stalk Phase
            hasPursuitStarted = false;
            hasFoundPlayerInPursuit = false;
            timeLeftToPursuit = timeLeftToPursuitMax;
            currentPursuitTime = 0f;

            monsterAgent.enabled = false;
            monsterAgent.speed = defaultSpeed;
            monsterAgent.acceleration = defaultAcceleration;
            monsterAgent.velocity = Vector3.zero;

            transform.position = new Vector3(0, -1000000, 0);


            if (latestCooldownReason == CooldownReason.LostPlayerAfterPursuit)
            {
                cooldownStarted = StartCoroutine(MonsterCooldown(5));
            }
            else if (latestCooldownReason == CooldownReason.SuccessfullyAttackedPlayer)
            {
                cooldownStarted = StartCoroutine(MonsterCooldown(10));
            }
        }
    }

    IEnumerator MonsterCooldown(float cooldownTimeSeconds)
    {
        yield return new WaitForSeconds(cooldownTimeSeconds);
        phase = Phase.Stalk;
        cooldownStarted = null;
        Debug.Log("Cooldown Phase Ended.");
    }
}

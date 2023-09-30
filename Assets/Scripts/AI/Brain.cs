using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Brain : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent monster;

    void Start()
    {
        monster = GetComponent<NavMeshAgent>();
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
        monster.destination = player.position;
    }
}

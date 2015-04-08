using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Character/AI/AI Control" )]
[RequireComponent( typeof( CharacterMovement ) )]
[RequireComponent( typeof( CharacterAttack ) )]
public class AIControl : MonoBehaviour
{
    private CharacterStatus characterStatus;

    // Attack timers
    public float MinAttackTime;
    public float MaxAttackTime;
    private float attackTime;
    private float attackTimer;
    private List<GameObject> enemiesInRange;

    // The AI moves left and right towards its target until it reaches a certain "close in"
    // range. Then it just moves straight in.
    public float CloseInRange;

    // When the AI gets to within range, it will stop and try to attack.
    private bool stopMovement;

    // Target fields.
    public GameObject Target { get; private set; }
    private bool hasTarget;
    private GameObject lastAttacker;


    // Initialization.
    void Start()
    {
        characterStatus = GetComponent<CharacterStatus>();
        attackTime = Random.Range( MinAttackTime, MaxAttackTime );
        attackTimer = 0f;
        enemiesInRange = new List<GameObject>();
        stopMovement = false;
        hasTarget = false;
    }

    public void SelectNewTarget()
    {
        // Collect potential targets.
        var availableTargets = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Player" ) );
        availableTargets.RemoveAll( t => t.layer != LayerMask.NameToLayer( "Player" ) );

        if( availableTargets.Count > 0 )
        {
            // Select a random target. This is a matter of a coin flip at the moment.
            var newtarget = availableTargets[Random.Range( 0, availableTargets.Count )];
            SelectNewTarget( newtarget );
        }
        else
        {
            Debug.Log( name + " cannot find a target!" );
            SendMessage( "Freeze", -1f );
        }
    }

    public void SelectNewTarget( GameObject newTarget )
    {
        Target = newTarget;
        lastAttacker = Target;
        hasTarget = true;
    }


    // AI.
    public void AIStopMovement( bool stop )
    {
        stopMovement = stop;
    }

    public void AddEnemyInRange( GameObject enemy )
    {
        if( !enemiesInRange.Contains( enemy ) )
        {
            enemiesInRange.Add( enemy );

            // Take targets of opportunity!
            if( !enemiesInRange.Contains( Target ) )
                SelectNewTarget( enemy );
        }
    }

    public void RemoveEnemyFromRange( GameObject enemy )
    {
        enemiesInRange.RemoveAll( e => e == enemy );

        // If that's the last enemy, stop attacking.
        if( enemiesInRange.Count == 0 )
        {
            attackTime = Random.Range( MinAttackTime, MaxAttackTime );
            attackTimer = 0f;
        }
    }

    public void OnCharacterDeath( GameObject character )
    {
        RemoveEnemyFromRange( character );
        if( Target == character )
        {
            Target = null;
            hasTarget = false;
        }
    }

    void Update()
    {
        if( !characterStatus.GamePaused )
        {
            // First, find a new target if we don't have one.
            if( !hasTarget || Target.GetComponent<CharacterStatus>().Dying )
            {
                SelectNewTarget();
                return;
            }
            else
            {
                // If we've been attacked by someone we should focus on them.
                if( Target != lastAttacker )
                    SelectNewTarget( lastAttacker );

                // ** MOVEMENT
                // First, find distance.
                var distance = Vector3.Distance( transform.position, Target.transform.position );

                // Change our approach based on distance.
                // Only move along the X-axis if we're still far away. This will give the player
                // a little extra time to to react.
                if( distance > CloseInRange )
                {
                    if( transform.position.x < Target.transform.position.x )
                        SendMessage( "SetInput", Vector3.right );
                    else
                        SendMessage( "SetInput", Vector3.left );
                }
                // Once we get close, move in for the kill.
                else if( !stopMovement )
                {
                    // Find vector to target.
                    var vectorToTarget = transform.position - Target.transform.position;
                    vectorToTarget.Normalize();

                    // Swap axes
                    vectorToTarget.y = vectorToTarget.z;
                    vectorToTarget.z = 0f;
                    vectorToTarget *= -1f;

                    // Send vector to movement script.
                    SendMessage( "SetInput", vectorToTarget );
                }
                else
                    SendMessage( "SetInput", Vector3.zero );
                // Now we're up close and personal.

                // ** ATTACK
                if( enemiesInRange.Count > 0 && !characterStatus.Attacking )
                {
                    attackTimer += Time.deltaTime;
                    if( attackTimer >= attackTime )
                    {
                        SendMessage( "IssueAttack", "Punch" );
                        attackTime = Random.Range( MinAttackTime, MaxAttackTime );
                        attackTimer = 0f;
                    }
                }
            }
        }
    }
}
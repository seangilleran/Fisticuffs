using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Character/Character Attack" )]
[RequireComponent( typeof( CharacterStatus ) )]
public class CharacterAttack : MonoBehaviour
{
    private CharacterStatus characterStatus;

    // Attack fields.
    public GameObject PunchAttack;
    public GameObject KickAttack;
    public GameObject[] Combos;
    public float ComboResetTime;
    private float comboResetTimer;
    private bool recentAttack;

    private List<GameObject> enemiesInRange;
    private List<GameObject> recentAttacks;


    // Initialization methods.
    void Awake()
    {
        characterStatus = GetComponent<CharacterStatus>();
    }

    void Start()
    {
        comboResetTimer = 0f;
        recentAttack = false;
        enemiesInRange = new List<GameObject>();
        recentAttacks = new List<GameObject>();
    }


    // Range methods.
    public void AddEnemyInRange( GameObject enemy )
    {
        if( !enemiesInRange.Contains( enemy ) )
            enemiesInRange.Add( enemy );
    }

    public void RemoveEnemyFromRange( GameObject enemy )
    {
        enemiesInRange.RemoveAll( e => e == enemy );
    }

    public void OnCharacterDeath( GameObject character )
    {
        RemoveEnemyFromRange( character );
    }


    // Attack methods
    public void IssueAttack( string attackType )
    {
        if( !characterStatus.Frozen )
        {
            // Set attack.
            GameObject attack = attackType == "Punch" ? PunchAttack : KickAttack;

            // Combo logic only applies if we're a player and there's an enemy in range.
            if( characterStatus.ID != -1 && enemiesInRange.Count > 0 )
            {
                // Recent attack. Keep this separate since the recentAttacks list will clear from time
                // to time even though the counter should keep going.
                recentAttack = true;
                recentAttacks.Add( attack );
                characterStatus.ComboCount++;
                comboResetTimer = 0f;

                if( recentAttacks.Count == 3 )
                {
                    // Cycle through each combo. If there is not a match, keep looking. If one is found,
                    // set the upcoming attack to be the combo attack.
                    foreach( GameObject combo in Combos )
                    {
                        var comboDetail = combo.GetComponent<ComboDetail>();
                        bool performCombo = true;

                        for( int i = 0; i < 3; i++ )
                        {
                            if( comboDetail.AttackList[i] != recentAttacks[i] )
                            {
                                performCombo = false;
                                break; // End for loop.
                            }
                        }

                        if( performCombo )
                        {
                            attack = comboDetail.ComboAttack;
                            break; // End foreach loop.
                        }
                    }

                    // Remove the first attack in the list.
                    recentAttacks.RemoveAt( 0 );
                }
            }
            // Reset combo counter if we didn't hit anything
            else if( enemiesInRange.Count == 0 )
                ComboReset();

            var attackDetail = attack.GetComponent<AttackDetail>();

            // Delay the attacker.
            characterStatus.Attacking = true;
            SendMessage( "Freeze", attackDetail.AttackDelay );

            // Send attack to all enemies in range.
            foreach( GameObject enemy in enemiesInRange )
            {
                // Tell the enemy who hit them.
                enemy.SendMessage( "SetLastAttacker", gameObject );
                // Give us some points.
                SendMessage( "AddScore", attackDetail.ScoreValue );

                if( !enemy.GetComponent<CharacterStatus>().Invulnerable )
                {
                    // Take damage.
                    if( attackDetail.HPDamage > 0 )
                        enemy.SendMessage( "TakeDamage", attackDetail.HPDamage );

                    // Issue status effects.
                    if( attackDetail.FreezeTime > 0f )
                        enemy.SendMessage( "Freeze", attackDetail.FreezeTime );
                    if( attackDetail.KnockbackForce > 0f )
                    {
                        var force = transform.position - enemy.transform.position;
                        force.Normalize();
                        enemy.SendMessage( "Knockback", force * attackDetail.KnockbackForce );
                    }
                    if( attackDetail.TargetInvulnerabilityTime > 0f )
                        enemy.SendMessage( "SetInvulnerable", attackDetail.TargetInvulnerabilityTime );
                }
            }

            // Set animation.
            BroadcastMessage( "SetAttackAnimation", attack );
        }
    }


    // Combo maintenance.
    public void ComboReset()
    {
        recentAttacks.Clear();
        recentAttack = false;
        comboResetTimer = 0f;
        characterStatus.ComboCount = 0;
    }

    void Update()
    {
        if( !characterStatus.GamePaused )
        {
            // Don't bother with the combo timer if we're an AI.
            if( characterStatus.ID != -1 && recentAttack )
            {
                comboResetTimer += Time.deltaTime;
                if( comboResetTimer >= ComboResetTime )
                    ComboReset();
            }
        }
    }
}
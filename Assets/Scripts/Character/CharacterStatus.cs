using UnityEngine;
using System.Collections;

[AddComponentMenu( "Character/Character Status" )]
public class CharacterStatus : MonoBehaviour
{
    private GameObject gameController;

    // ID.
    public int ID { get; private set; }
    private bool IDSet;

    public bool GamePaused { get; private set; }

    // Basic Stats.
    public int MaxHP;
    public int CurrentHP { get; set; }
    public int Score { get; set; }
    public int ScoreValue;  // Points you get for killing this character.
    public int ComboCount { get; set; }

    // Status Effects.
    public bool Invulnerable;
    private bool useInvulnerabilityTimer;
    private float invulnerabilityTime;
    private float invulnerabilityTimer;
    private bool invulnerabilitySet;
    public bool Jumping { get; set; }
    public bool Attacking { get; set; }
    public GameObject LastAttacker { get; set; }
    public bool Frozen;
    private bool useFreezeTimer;    // Don't unfreeze if we've frozen them in the inspector.
    private float freezeTimer;
    private bool freezeSet;
    public float DeathTime;
    public bool Dying { get; private set; }


    // Initialization methods.
    void Awake()
    {
        // ID setup has to be done as early as possible since so many other functionalities
        // rely on it.
        ID = -1;
        IDSet = false;
    }

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag( "GameController" );
        GamePaused = false;
        CurrentHP = MaxHP;
        ComboCount = 0;
        useInvulnerabilityTimer = false;
        invulnerabilityTimer = 0f;
        invulnerabilitySet = false;
        Jumping = ID == -1 ? false : true;  // Player characters start in the air!
        Attacking = false;
        useFreezeTimer = false;
        freezeTimer = 0f;
        freezeSet = false;
        Dying = false;
    }

    public void SetCharacterID( int n )
    {
        // Only do this once.
        if( !IDSet )
        {
            ID = n;
            name += " (ID: " + ID + ")";
            IDSet = true;
        }
    }


    // Pause messages.
    public void OnPauseGame()
    {
        GamePaused = true;
    }

    public void OnUnpauseGame()
    {
        GamePaused = false;
    }


    // Status methods.
    public void AddScore( int points )
    {
        if( ID != -1 )
            Score += points * ComboCount;
    }

    public void StopJump()
    {
        Jumping = false;
    }

    public void Freeze( float freezeTime )
    {
        Frozen = true;

        // Freeze indefinitely if freezeTime is -1.
        useFreezeTimer = freezeTime == -1f ? false : true;
        freezeTimer = freezeTime;
    }

    public void UnFreeze()
    {
        if( Attacking )
            Attacking = false;

        Frozen = false;
        useFreezeTimer = false;
        freezeTimer = 0f;
    }

    public void SetInvulnerable( float time )
    {
        Invulnerable = true;

        // Make indefinite if time is -1.
        if( time == -1 )
            useInvulnerabilityTimer = false;
        else
        {
            invulnerabilityTime = time;
            invulnerabilityTimer = 0f;
            useInvulnerabilityTimer = true;
        }
    }


    // HP messages.
    public void TakeDamage( int n )
    {
        CurrentHP -= n;
        if( CurrentHP < 0 )
            CurrentHP = 0;
    }

    public void RestoreHP( int n )
    {
        CurrentHP += n;
        if( CurrentHP > MaxHP )
            CurrentHP = MaxHP;
    }


    // Target messages.
    public void SetLastAttacker( GameObject character )
    {
        LastAttacker = character;
    }

    public void OnCharacterDeath( GameObject character )
    {
        if( LastAttacker == character )
            SetLastAttacker( null );
    }


    void Update()
    {
        // Set animation if it hasn't been done yet.
        // These can be applied in the editor even if we're paused so do it here.
        if( Frozen && !Attacking && !Dying && !freezeSet )
        {
            BroadcastMessage( "SetAnimation", "Idle" );
            freezeSet = true;
        }
        else if( !Frozen && freezeSet )
            freezeSet = false;
        if( Invulnerable && !invulnerabilitySet )
        {
            SetLayer( LayerMask.NameToLayer( "Invulnerable" ) );
            BroadcastMessage( "SetAnimation", "Flashing" );
            invulnerabilitySet = true;
        }
        else if( !Invulnerable && invulnerabilitySet )
        {
            BroadcastMessage( "SetAnimation", "StopFlashing" );
            invulnerabilitySet = false;
            if( !Dying )
                SetLayer( LayerMask.NameToLayer( ID == -1 ? "Enemy" : "Player" ) );
        }

        if( !GamePaused )
        {
            // Run the freeze timer if we're frozen.
            if( Frozen && useFreezeTimer )
            {
                freezeTimer -= Time.deltaTime;
                if( freezeTimer <= 0f )
                    UnFreeze();
            }
            // Invulnerability timer.
            if( Invulnerable && useInvulnerabilityTimer )
            {
                invulnerabilityTimer += Time.deltaTime;
                if( invulnerabilityTimer >= invulnerabilityTime )
                {
                    Invulnerable = false;
                    useInvulnerabilityTimer = false;
                }
            }

            if( CurrentHP <= 0 )
            {
                if( !Dying )
                {
                    SetLayer( LayerMask.NameToLayer( "Dying" ) );
                    LastAttacker.SendMessage( "AddScore", ScoreValue );

                    // Notify the rest of the game we're on the way out.
                    foreach( GameObject obj in FindObjectsOfType<GameObject>() )
                        obj.SendMessage( "OnCharacterDeath", gameObject, SendMessageOptions.DontRequireReceiver );

                    SendMessage( "Freeze", DeathTime );
                    BroadcastMessage( "SetAnimation", "Dying" );
                    Dying = true;
                }
                else
                {
                    // The dying timer is up - it is time to move on.
                    if( !Frozen )
                        gameController.SendMessage( "DestroyCharacter", gameObject );
                }
            }
        }
    }


    // Helper messages: change our layer.
    public void SetLayer( int layer )
    {
        gameObject.layer = layer;
        foreach( Transform child in transform )
            child.gameObject.layer = layer;
    }
}
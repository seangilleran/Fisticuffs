using UnityEngine;
using System.Collections;

[AddComponentMenu( "Character/Character Movement" )]
[RequireComponent( typeof( CharacterStatus ) )]
[RequireComponent( typeof( Rigidbody ) )]
public class CharacterMovement : MonoBehaviour
{
    // Keep local copy of the character status.
    private CharacterStatus characterStatus;

    // Movement fields.
    public float Speed;
    private Vector3 input;
    private Vector3 lastInput;

    public float JumpForce;

    private bool unFrozenThisFrame;


    // Initialization methods.
    void Awake()
    {
        characterStatus = GetComponent<CharacterStatus>();
    }

    void Start()
    {
        input = Vector3.zero;
        lastInput = Vector3.zero;
        unFrozenThisFrame = false;
    }


    // Movement methods.
    public void SetInput( Vector3 newInput )
    {
        lastInput = input;
        input = newInput;
    }

    public void StartJump()
    {
        if( !characterStatus.Jumping )
        {
            var movement = GetComponent<Rigidbody>().velocity;
            movement.y += JumpForce;
            GetComponent<Rigidbody>().velocity = movement;

            // We would usually let CharacterStatus handle this logic, but because we
            // cannot control the order in which messages are received by components,
            // we have to set it ourselves here.
            characterStatus.Jumping = true;
        }
    }

    public void Knockback( Vector3 force )
    {
        GetComponent<Rigidbody>().velocity -= force;
    }


    // Update and move the character.
    void FixedUpdate()
    {
        if( !characterStatus.GamePaused )
        {
            var movement = GetComponent<Rigidbody>().velocity;
            if( !characterStatus.Frozen )
            {
                // If we're frozen, don't allow left/right/up/down movement.
                // Obviously, we can still fall, so the y axis is unaffected.
                movement.x = input.x * Speed;
                movement.z = input.y * Speed;
            }
            GetComponent<Rigidbody>().velocity = movement;
        }
    }

    void Update()
    {
        if( !characterStatus.GamePaused )
        {
            if( !characterStatus.Frozen )
            {
                // Set facing for the sprite.
                if( input.x > 0f )
                    BroadcastMessage( "SetFacing", "Right" );
                else if( input.x < 0f )
                    BroadcastMessage( "SetFacing", "Left" );

                // Set Animation.
                if( ( input.x != 0f || input.z != 0f ) && ( ( lastInput.x == 0f && lastInput.z == 0f ) || unFrozenThisFrame ) )
                    BroadcastMessage( "SetAnimation", "Walk" );
                else if( input.x == 0f && input.z == 0f && ( ( lastInput.x != 0f || lastInput.z != 0f ) || unFrozenThisFrame ) )
                    BroadcastMessage( "SetAnimation", "Idle" );

                if( unFrozenThisFrame )
                    unFrozenThisFrame = false;
            }
            else
            {
                if( !unFrozenThisFrame )
                    unFrozenThisFrame = true;
            }
        }
    }
}
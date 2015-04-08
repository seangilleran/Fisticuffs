using UnityEngine;
using System.Collections;

[AddComponentMenu( "Character/Player/Player Control" )]
[RequireComponent( typeof( CharacterStatus ) )]
[RequireComponent( typeof( CharacterMovement ) )]
[RequireComponent( typeof( CharacterAttack ) )]
public class PlayerControl : MonoBehaviour
{
    private GameObject gameController;
    private CharacterStatus characterStatus;
    private bool lastPauser;


    // Initialization.
    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag( "GameController" );
        characterStatus = GetComponent<CharacterStatus>();
        lastPauser = false;
    }

    public void OnUnpauseGame()
    {
        // Reset for next pause.
        lastPauser = false;
    }


    // Poll controller.
    void Update()
    {
        if( !characterStatus.GamePaused )
        {
            // Attack controls.
            if( Input.GetButtonDown( "P" + characterStatus.ID + "_Punch" ) )
                SendMessage( "IssueAttack", "Punch" );
            else if( Input.GetButtonDown( "P" + characterStatus.ID + "_Kick" ) )
                SendMessage( "IssueAttack", "Kick" );
            else if( Input.GetButtonDown( "P" + characterStatus.ID + "_Jump" ) )
                SendMessage( "StartJump" );

            // Movement control.
            var input = Vector3.zero;
            input.x = Input.GetAxis( "P" + characterStatus.ID + "_Horizontal" );
            input.y = Input.GetAxis( "P" + characterStatus.ID + "_Vertical" );
            SendMessage( "SetInput", input );

            // Pause control.
            if( Input.GetButtonDown( "P" + characterStatus.ID + "_Start" ) )
            {
                lastPauser = true;
                gameController.SendMessage( "Pause", true );
            }
        }
        else
        {
            // Only let the pauser unpause.
            if( Input.GetButtonDown( "P" + characterStatus.ID + "_Start" ) && lastPauser )
                gameController.SendMessage( "Pause", false );
        }
    }
}
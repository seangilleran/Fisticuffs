using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Camera/Camera Stop Trigger" )]
[RequireComponent( typeof( Collider ) )]
public class CameraStopTrigger : MonoBehaviour
{
    // Minor bugfix: if a player enters this trigger we should stop the camera from moving.
    private List<GameObject> playerList;

    void Start()
    {
        playerList = new List<GameObject>();
    }

    void OnTriggerEnter( Collider other )
    {
        if( other.tag == "Player" )
        {
            if( playerList.Count == 0 )
                Camera.main.SendMessage( "PauseMovement", true );
            playerList.Add( other.gameObject );
        }
    }

    void OnTriggerExit( Collider other )
    {
        playerList.RemoveAll( p => p == other.gameObject );
        if( playerList.Count == 0 )
            Camera.main.SendMessage( "PauseMovement", false );
    }

    public void OnCharacterDeath( GameObject character )
    {
        playerList.RemoveAll( c => c == character );
        if( playerList.Count == 0 )
            Camera.main.SendMessage( "PauseMovement", false );
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Character/AI/AI Movement Stop Trigger" )]
[RequireComponent( typeof( Collider ) )]
public class AIMovementStopTrigger : MonoBehaviour
{
    // Tell the AI that we're next to a player.
    private List<GameObject> nearbyPlayers;

    void Start()
    {
        nearbyPlayers = new List<GameObject>();
    }

    void OnTriggerEnter( Collider other )
    {
        if( other.gameObject.tag == "Player" )
        {
            if( !nearbyPlayers.Contains( other.gameObject ) )
            {
                if( nearbyPlayers.Count == 0 )
                    SendMessageUpwards( "AIStopMovement", true );
                nearbyPlayers.Add( other.gameObject );

                // If we've bumped into another player, we should probably
                // make them our target.
                if( other.gameObject != GetComponentInParent<AIControl>().Target )
                    SendMessageUpwards( "SelectNewTarget", other.gameObject );
            }
        }
    }

    void OnTriggerExit( Collider other )
    {
        nearbyPlayers.RemoveAll( p => p == other.gameObject );
        if( nearbyPlayers.Count == 0 )
            SendMessageUpwards( "AIStopMovement", false );
    }

    public void OnCharacterDeath( GameObject character )
    {
        nearbyPlayers.RemoveAll( p => p == character );
        if( nearbyPlayers.Count == 0 )
            SendMessageUpwards( "AIStopMovement", false );
    }
}
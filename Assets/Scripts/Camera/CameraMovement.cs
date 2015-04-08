using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Camera/Camera Movement" )]
[RequireComponent( typeof( Camera ) )]
public class CameraMovement : MonoBehaviour
{
    private GameObject gameController;

    // Since floating point math is pretty imprecise, let's just call it
    // good when we're within this distance of the target.
    public float SnapTolerance;

    // Target node. The camera will stop moving when it reaches this point.
    private GameObject targetNode;
    private bool lockedToNode;

    // Target player. The camera will follow this player.
    private List<GameObject> playerList;
    private GameObject targetPlayer;
    private float highestPlayerX;
    private float lastPlayerX;
    private bool targetPlayerInitialized;

    // If the player gets too close to the edge of the screen, we need to
    // stop moving until they back off a bit.
    public bool movementPaused;


    // Initialization.
    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag( "GameController" );
        lockedToNode = true;
        playerList = new List<GameObject>();
        highestPlayerX = 0f;
        lastPlayerX = 0f;
        targetPlayerInitialized = false;
        movementPaused = false;
    }

    public void OnSpawnPlayer( GameObject player )
    {
        playerList.Add( player );
        targetPlayerInitialized = false;
    }

    public void OnCharacterDeath( GameObject character )
    {
        playerList.RemoveAll( c => c == character );
    }


    // Camera movement.
    public void SetTargetNode( GameObject newTargetNode )
    {
        lockedToNode = false;
        targetNode = newTargetNode;
        targetPlayerInitialized = false;
        movementPaused = false;
    }


    void Update()
    {
        if( !lockedToNode )
        {
            // Check and see if we're at the target node.
            var distance = targetNode.transform.position.x - transform.position.x;
            if( distance < SnapTolerance )
            {
                // Snap the camera in place.
                var pos = transform.position;
                pos.x = targetNode.transform.position.x;
                transform.position = pos;
                lockedToNode = true;

                // Tell the game we've arrived.
                gameController.SendMessage( "ActivatePlayNode" );
            }
            // Move the camera
            else if( !movementPaused )
            {
                // Find the player who is farthest to the right.
                foreach( GameObject player in playerList )
                    if( player.transform.position.x > highestPlayerX )
                        targetPlayer = player;
                highestPlayerX = targetPlayer.transform.position.x;

                if( targetPlayerInitialized )
                {
                    // If that player moves right, move the camera with them.
                    var newX = highestPlayerX - lastPlayerX;
                    if( newX > 0 )
                    {
                        var pos = transform.position;
                        pos.x += newX;
                        transform.position = pos;
                    }
                }

                lastPlayerX = highestPlayerX;

                // Run this once to get player x values before actually moving the camera to prevent
                // a sudden jump cut.
                if( !targetPlayerInitialized )
                    targetPlayerInitialized = true;
            }
        }
    }

    public void PauseMovement( bool value )
    {
        movementPaused = value;
        
        // We will need to reinitialze the player positions.
        targetPlayerInitialized = false;
    }
}
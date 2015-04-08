using UnityEngine;
using System.Collections;

[AddComponentMenu( "Character/Attack/Attack Detail" )]
[RequireComponent( typeof( Collider ) )]
public class AttackTrigger : MonoBehaviour
{
    // We need the character ID to know if we're a player or an AI. This prevents
    // friendly targets from getting added to the list.
    private string enemyTag;
    
    // Record the local position of the trigger in the editor and flip it if the
    // character changes facing.
    private float initialPosition;


    // Initialize.
    void Start()
    {
        // Use "Player" as our enemy tag if we're an AI and "Enemy" if we're a player.
        enemyTag = GetComponentInParent<CharacterStatus>().ID == -1 ? "Player" : "Enemy";
        initialPosition = transform.localPosition.x;
    }


    // Facing.
    public void SetFacing( string direction )
    {
        var pos = transform.localPosition;
        if( direction == "Right" )
            pos.x = initialPosition;
        else if( direction == "Left" )
            pos.x = initialPosition * -1;
        else
            Debug.LogError( transform.parent.gameObject.name + " says that \"" + direction + "\" is not a proper direction!" );
        transform.localPosition = pos;
    }


    // Trigger messages.
    void OnTriggerEnter( Collider other )
    {
        if( other.tag == enemyTag )
            SendMessageUpwards( "AddEnemyInRange", other.gameObject );
    }

    void OnTriggerExit( Collider other )
    {
        if( other.tag == enemyTag )
            SendMessageUpwards( "RemoveEnemyFromRange", other.gameObject );
    }
}
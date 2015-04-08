using UnityEngine;
using System.Collections;

[AddComponentMenu( "Items/Powerup" )]
public class Powerup : MonoBehaviour
{
    public int HPToRestore;

    void OnTriggerEnter( Collider other )
    {
        if( other.tag == "Player" )
        {
            // Restore some HP.
            other.SendMessage( "RestoreHP", HPToRestore );
            
            // Destroy the powerup.
            name += " (Picked Up)";
            gameObject.SetActive( false );
        }
    }
}
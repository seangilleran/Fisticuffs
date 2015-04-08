using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Character/Player/Jump Reset Trigger" )]
[RequireComponent( typeof( Collider ) )]
public class JumpResetTrigger : MonoBehaviour
{
    private CharacterStatus characterStatus;

    void Start()
    {
        characterStatus = GetComponentInParent<CharacterStatus>();
    }

    void OnTriggerEnter( Collider other )
    {
        if( characterStatus.Jumping )
            SendMessageUpwards( "StopJump" );
    }
}
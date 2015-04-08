using UnityEngine;
using System.Collections;

[AddComponentMenu( "Character/Attack/Attack Detail" )]
public class AttackDetail : MonoBehaviour
{
    public int HPDamage;
    public float AttackDelay;
    public float FreezeTime;
    public float KnockbackForce;
    public float TargetInvulnerabilityTime;
    public int ScoreValue;
    public string AnimationName;
}
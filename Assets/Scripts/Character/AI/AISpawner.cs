using UnityEngine;
using System.Collections;

[AddComponentMenu( "Character/AI/AI Spawner" )]
[RequireComponent( typeof( AIControl ) )]
public class AISpawner : MonoBehaviour
{
    public GameObject SpawnPoint;

    // Will not spawn until TimeDelay seconds have passed since the last
    // spawn. Set to 0 to spawn immediately.
    public float TimeDelay;
    private float timer;

    // Will not spawn until the number of enemies on screen is less than
    // CountDelay.
    public bool UseCountDelay;
    public int CountDelay;

    void Start()
    {
        gameObject.SetActive( false );
    }
}
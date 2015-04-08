using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[AddComponentMenu( "Play Node Controller" )]
public class PlayNodeController : MonoBehaviour
{
    private GameObject gameController;

    private GameObject[] spawnOrder;
    private int spawnTicket;

    private float timer;


    // Initialization
    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag( "GameController" );

        // Fill the spawn order list.
        var spawnList = new List<GameObject>();
        foreach( Transform child in transform )
            if( child.tag == "Enemy" )
                spawnList.Add( child.gameObject );
        spawnOrder = spawnList.OrderBy( e => Int32.Parse( e.name.Replace( "Enemy ", "" ) ) ).ToArray();

        spawnTicket = 0;
        timer = 0f;

        // Wait until the game controller tells us to start.
        gameObject.SetActive( false );
    }


    // Spawn logic
    void Update()
    {
        if( !gameController.GetComponent<GameController>().Paused )
        {
            var enemyCount = gameController.GetComponent<GameController>().EnemyCount;

            if( spawnTicket < spawnOrder.Length )
            {
                timer += Time.deltaTime;
                var onDeck = spawnOrder[spawnTicket].GetComponent<AISpawner>();
                if( timer >= onDeck.TimeDelay && ( !onDeck.UseCountDelay || enemyCount <= onDeck.CountDelay ) )
                {
                    gameController.SendMessage( "SpawnEnemy", spawnOrder[spawnTicket] );
                    spawnTicket++;
                    timer = 0f;
                }
            }
            else
            {
                if( enemyCount == 0 )
                    gameController.SendMessage( "PlayNodeExhausted" );
            }
        }
    }
}
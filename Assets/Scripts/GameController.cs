using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[AddComponentMenu( "Game Controller" )]
public class GameController : MonoBehaviour
{
    private GameObject uiController;

    // Player fields.
    public GameObject PlayerPrefab;
    public GameObject PlayerSpawnPoint;
    public int PlayerCount { get; private set; }
    private bool[] playerIDs;   // Records which controllers are in use.

    // Enemy fields.
    public int EnemyCount;

    // Game control fields.
    private bool gameStarted;
    public bool Paused { get; private set; }
    private GameObject[] playNodes;
    private int nodeTicket;
    public float NodeStartDelay;
    private bool nodeStarting;
    private float nodeStartTimer;


    // Initialization.
    void Awake()
    {
        // Force window for build version.
        //Screen.SetResolution( 1066, 600, false );
    }

    void Start()
    {
        uiController = GameObject.FindGameObjectWithTag( "UIController" );

        playerIDs = new bool[] { false, false, false, false, false, false };
        PlayerCount = 0;
        EnemyCount = 0;

        gameStarted = false;
        Paused = false;

        // Build play node list.
        var nodeList = new List<GameObject>();
        foreach( Transform child in transform )
            if( child.tag == "PlayNode" )
                nodeList.Add( child.gameObject );
        playNodes = nodeList.OrderBy( n => Int32.Parse( n.name.Replace( "PlayNode ", "" ) ) ).ToArray();

        nodeTicket = 0;
        nodeStarting = false;
        nodeStartTimer = 0f;
    }


    // Application control.
    public void Pause( bool value )
    {
        Paused = value;

        if( Paused )
        {
            foreach( GameObject obj in FindObjectsOfType<GameObject>() )
                obj.SendMessage( "OnPauseGame", SendMessageOptions.DontRequireReceiver );
            Time.timeScale = 0f;
        }
        else
        {
            foreach( GameObject obj in FindObjectsOfType<GameObject>() )
                obj.SendMessage( "OnUnpauseGame", SendMessageOptions.DontRequireReceiver );
            Time.timeScale = 1f;
        }
    }

    public void Reset()
    {
        Application.LoadLevel( "Level_1" );
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OnGameOver()
    {
        foreach( Transform child in transform )
            child.BroadcastMessage( "OnPauseGame", SendMessageOptions.DontRequireReceiver );
        uiController.SendMessage( "OnGameOver" );
    }


    // Create players when they press the start button.
    private void SpawnPlayer( int n )
    {
        playerIDs[n - 1] = true;
        var newPlayer = (GameObject)Instantiate( PlayerPrefab, PlayerSpawnPoint.transform.position, Quaternion.identity );
        newPlayer.SendMessage( "SetCharacterID", n );
        newPlayer.transform.parent = transform;
        PlayerCount++;

        // Tell other objects in the scene that a new player has been created.
        foreach( GameObject obj in FindObjectsOfType<GameObject>() )
            obj.SendMessage( "OnSpawnPlayer", newPlayer, SendMessageOptions.DontRequireReceiver );

        // Start the game
        if( !gameStarted )
        {
            ActivatePlayNode();
            Pause( false );
            gameStarted = true;
        }
    }

    public void SpawnEnemy( GameObject enemy )
    {
        var spawnDetails = enemy.GetComponent<AISpawner>();
        enemy.transform.position = spawnDetails.SpawnPoint.transform.position;
        enemy.name += " (Spawned)";
        enemy.SetActive( true );
        EnemyCount++;
    }

    // Remove enemies and players; handle game over logic.
    public void OnCharacterDeath( GameObject character )
    {
        // Handle count logic before players are totally removed from the game.
        // They are still visible but we don't want to take them into AI calculations.
        if( character.GetComponent<CharacterStatus>().ID != -1 )
            PlayerCount--;
    }

    public void DestroyCharacter( GameObject character )
    {
        var characterStatus = character.GetComponent<CharacterStatus>();
        character.name += " (Dead)";
        character.SetActive( false );

        if( characterStatus.ID == -1 )  // AI
            EnemyCount--;
        else  // Player
        {
            playerIDs[characterStatus.ID - 1] = false;
            uiController.SendMessage( "OnKillPlayer", character );

            if( PlayerCount <= 0 )
                OnGameOver();
        }
    }


    // Play node control.
    public void ActivatePlayNode()
    {
        nodeStarting = true;
        nodeStartTimer = 0f;
        uiController.SendMessage( "UINextNode", false );
        uiController.SendMessage( "UIReadyMessage" );
    }

    public void PlayNodeExhausted()
    {
        // Exhaust the old node.
        Debug.Log( "Node " + nodeTicket + " exhausted" );
        playNodes[nodeTicket].SetActive( false );

        // See if we're done with the game.
        nodeTicket++;
        if( nodeTicket >= playNodes.Length )
        {
            Debug.Log( "Last node exhausted - game complete!" );
            OnGameOver();
        }
        // Otherwise unlock the camera and ready the next node.
        else
        {
            Camera.main.SendMessage( "SetTargetNode", playNodes[nodeTicket] );
            uiController.SendMessage( "UINextNode", true );
        }
    }


    void Update()
    {
        // Pause game on ESC
        if( gameStarted && Input.GetKeyUp( KeyCode.Escape ) )
            Pause( !Paused );

        if( !Paused )
        {
            // Add new players.
            if( PlayerCount < 2 && Input.GetButtonDown( "Any_Start" ) )
            {
                for( int i = 1; i <= 6; i++ )
                    if( Input.GetButton( "P" + i + "_Start" ) && !playerIDs[i - 1] )
                        SpawnPlayer( i );
            }

            // See if a new node is starting up.
            if( nodeStarting )
            {
                nodeStartTimer += Time.deltaTime;
                if( nodeStartTimer >= NodeStartDelay )
                {
                    playNodes[nodeTicket].SetActive( true );
                    Debug.Log( "Node " + nodeTicket + " activated." );
                    uiController.SendMessage( "UIFightMessage", NodeStartDelay );
                    nodeStarting = false;
                }
            }
        }
    }
}
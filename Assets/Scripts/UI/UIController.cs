using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "UI/UI Controller" )]
[RequireComponent( typeof( Canvas ) )]
public class UIController : MonoBehaviour
{
    public GameObject UIPauseMenu;
    public GameObject UIGameOverMenu;

    // Player panels.
    public GameObject[] UIStartNotices;
    public GameObject[] UIPlayerPanels;
    private List<GameObject> playerList;
    private int[] playerIDs;

    // Stage indicators.
    public GameObject UINextNodePanel;
    public GameObject UIReadyPanel;
    public GameObject UIFightPanel;
    private float fightPanelTime;
    private float fightPanelTimer;


    // Initialization.
    void Start()
    {
        UIGameOverMenu.SetActive( false );

        for( var i = 0; i < 2; i++ )
        {
            UIPlayerPanels[i].SetActive( false );
            UIStartNotices[i].SetActive( true );
        }
        playerList = new List<GameObject>();

        UINextNodePanel.SetActive( false );
        UIReadyPanel.SetActive( false );
        UIFightPanel.SetActive( false );
        fightPanelTime = 0f;
        fightPanelTimer = 0f;
    }


    // Spawn message.
    public void OnSpawnPlayer( GameObject player )
    {
        playerList.Add( player );
        
        // Find and activate an empty player panel.
        for( var i = 0; i < 2; i++ )
        {
            if( !UIPlayerPanels[i].activeInHierarchy )
            {
                UIStartNotices[i].SetActive( false );
                UIPlayerPanels[i].SetActive( true );
                UIPlayerPanels[i].SendMessage( "SetPlayer", player );
                break;
            }
        }
    }

    public void OnKillPlayer( GameObject player )
    {
        for( var i = 0; i < 2; i++ )
        {
            if( UIPlayerPanels[i].GetComponent<UIPlayerPanel>().Player == player )
            {
                UIPlayerPanels[i].SetActive( false );
                UIStartNotices[i].SetActive( true );
                break;
            }
        }
    }


    // Pause menu.
    public void OnPauseGame()
    {
        UIPauseMenu.SetActive( true );
    }

    public void OnUnpauseGame()
    {
        UIPauseMenu.SetActive( false );
    }

    public void OnGameOver()
    {
        UIPauseMenu.SetActive( false );
        UIGameOverMenu.SetActive( true );
    }


    // Indicator panels.
    public void UINextNode( bool value )
    {
        UINextNodePanel.SetActive( value );
    }

    public void UIReadyMessage()
    {
        UIReadyPanel.SetActive( true );
    }

    public void UIFightMessage( float time )
    {
        UIReadyPanel.SetActive( false );
        UIFightPanel.SetActive( true );
        fightPanelTime = time;
    }

    void Update()
    {
        if( UIFightPanel.activeInHierarchy )
        {
            fightPanelTimer += Time.deltaTime;
            if( fightPanelTimer > fightPanelTime )
            {
                UIFightPanel.SetActive( false );
                fightPanelTimer = 0f;
            }
        }
    }
}
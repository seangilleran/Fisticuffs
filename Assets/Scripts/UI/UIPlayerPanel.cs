using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[AddComponentMenu( "UI/Player Panel Controller" )]
public class UIPlayerPanel : MonoBehaviour
{
    // References to UI elements
    public GameObject Portrait;
    public GameObject HealthBar;
    public GameObject Score;
    public GameObject ComboBox;

    // Reference to player.
    public GameObject Player { get; private set; }
    CharacterStatus playerStatus;


    // Initialization.
    public void SetPlayer( GameObject newPlayer )
    {
        Player = newPlayer;
        playerStatus = Player.GetComponent<CharacterStatus>();
    }


    void Update()
    {
        // Health.
        var maxHealth = playerStatus.MaxHP;
        var curHealth = playerStatus.CurrentHP;
        float barScale = (float)curHealth / (float)maxHealth;
        HealthBar.GetComponent<Image>().rectTransform.localScale = new Vector3( barScale, 1f, 1f );

        // Combo box.
        var comboBoxText = ComboBox.GetComponent<Text>();
        var comboCount = playerStatus.ComboCount;
        if( comboCount > 1 )
            comboBoxText.text = comboCount + "x";
        else
            comboBoxText.text = "";

        // Score.
        var scoreBoxText = Score.GetComponent<Text>();
        scoreBoxText.text = playerStatus.Score.ToString();
    }
}
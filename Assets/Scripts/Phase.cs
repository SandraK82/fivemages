using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class Phase : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;


    private void Awake()
    {
        
    }

    private void OnClick()
    {
        GameManager.GameStateContinue();
    }

    private void OnEnable()
    {
        GameManager.OnGameState += GameManager_OnGameState;
    }

    private void OnDestroy()
    {
        GameManager.OnGameState -= GameManager_OnGameState;
        text = null;
    }

    private void GameManager_OnGameState(object sender, GameManager.GameStateArgs e)
    {
        if (text == null) return;
        switch(e.state)
        {
            case GameManager.GameState.PREPARE:
                text.text = "Build Phase " + e.level;
                text.gameObject.SetActive(true);
                break;
            case GameManager.GameState.PLAY:
                text.text = "Wave " + e.level;
                text.gameObject.SetActive(true);
                break;
            default:
                text.gameObject.SetActive(false);
                break;
        }
    }
}

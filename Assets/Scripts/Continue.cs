using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class Continue : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI text;


    private void Awake()
    {
        continueButton.onClick.AddListener(OnClick);
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
        continueButton = null;
        text = null;
    }

    private void GameManager_OnGameState(object sender, GameManager.GameStateArgs e)
    {
        if (text == null) return;
        if (continueButton == null) return;
        switch(e.state)
        {
            case GameManager.GameState.PREPARE:
                text.text = "Continue with Wave " + e.level;
                continueButton.gameObject.SetActive(true);
                break;
            default:
                continueButton.gameObject.SetActive(false);
                break;
        }
    }
}

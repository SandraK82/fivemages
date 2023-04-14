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
    }

    private void GameManager_OnGameState(object sender, GameManager.GameStateArgs e)
    {
        switch(e.state)
        {
            case GameManager.GameState.PREPARE:
                Debug.Log("Enable continue");
                text.text = "Continue with Wave " + e.level;
                continueButton.gameObject.SetActive(true);
                break;
            default:
                Debug.Log("Disable continue");
                continueButton.gameObject.SetActive(false);
                break;
        }
    }
}

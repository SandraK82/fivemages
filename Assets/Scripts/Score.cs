using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using static GameManager;

public class Score : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        GameManager.OnScore += OnScore;
    }

    private void OnScore(object sender, ScoreEventArgs e)
    {
        string s = (e.score / 7) + " x 7";
        if(e.score==0)
        {
            s = "0";
        }
        score.SetText(s);
        animator.SetTrigger("Wiggle");
    }
}

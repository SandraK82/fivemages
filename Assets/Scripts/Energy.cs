using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();

        GameManager.OnEnergy += OnEnergy;
    }

    private void OnEnergy(object sender, GameManager.EnergyEventArgs e)
    {
        image.fillAmount = e.energy / e.maxEnergy;
    }
}

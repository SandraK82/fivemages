using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class Build : MonoBehaviour
{
    [SerializeField] private Button buildButton;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI warning;

    private Mage mage = null;
    private Tower tower = null;
    private float energy;
    private float time;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip click;
    
    private void Awake()
    {
        buildButton.onClick.AddListener(OnClick);
        warning.gameObject.SetActive(false);
        buildButton.gameObject.SetActive(false);
        GameManager.OnSelected += GameManager_OnSelected;
        GameManager.OnEnergy += GameManager_OnEnergy;
    }

    private void OnClick()
    {
        source.PlayOneShot(click);
        if (tower != null)
        {
            Mage amage = GameManager.GetMage(tower.GetMagic());
            if (amage != null && amage.GetState() == Mage.MageState.IDLE)
            {
                //only check if not yet conencted
                if (tower.GetConnectedMultiplier() == 1f)
                {
                    Vector3 p = tower.transform.position;
                    float r = 7f * (tower.GetLevel() + 1) * Tower.radiusFactor * 0.5f;
                    foreach (Tower other in GameManager.GetTowers())
                    {
                        if (other == tower) continue;

                        Vector3 tp = other.transform.position;
                        float tr = 7f * other.GetLevel() * Tower.radiusFactor * 0.5f;
                        if ((tp - p).magnitude < r + tr)
                        {
                            GameManager.WillOverlap();
                            return;
                        }
                    }
                }
                tower.Upgrade();
            } else
            {
                time = 2f;
                warning.text = "No correct mage available!";
                warning.gameObject.SetActive(true);
            }
        } else if(mage != null)
        {
            if (GameManager.IsBuildMode())
            {
                GameManager.SetBuildMode(false);
                text.text = "Build Tower";
            }
            else
            {
                GameManager.SetBuildMode(true);
                text.text = "Cancel Tower Building";
            }
        }
    }

    private void GameManager_OnEnergy(object sender, GameManager.EnergyEventArgs e)
    {
        energy = e.energy;
    }

    private void Update()
    {
        if(time >= 0f)
        {
            time -= Time.deltaTime;
            if (time <= 0f) warning.gameObject.SetActive(false);
        }

        if(mage!=null && mage.GetState() != Mage.MageState.IDLE)
        {
            buildButton.gameObject.SetActive(false);
        } else if(mage != null && mage.GetState() == Mage.MageState.IDLE && !buildButton.gameObject.activeSelf)
        {
            GameManager_OnSelected(null, new GameManager.SelectedArgs { mage = mage });
        } 

        if (tower != null && tower.GetState() != Tower.State.STANDING)
        {
            buildButton.gameObject.SetActive(false);
        } else if(tower != null && tower.GetState() == Tower.State.STANDING && !buildButton.gameObject.activeSelf)
        {
            GameManager_OnSelected(null, new GameManager.SelectedArgs { tower = tower });
        }
    }

    private void GameManager_OnSelected(object sender, GameManager.SelectedArgs e)
    {
        mage = null;
        tower = null;

        if (e.tower != null && e.tower.GetState()==Tower.State.STANDING)
        {
            if (e.tower.GetLevel() * 7 <= energy)
            {
                buildButton.gameObject.SetActive(true);
                text.text = "Upgrade Tower to Level " + (e.tower.GetLevel() + 1);
                tower = e.tower;
            }
            else
            {
                time = 2f;
                warning.text = "Not enough Energy to upgrade Tower!";
                warning.gameObject.SetActive(true);
                buildButton.gameObject.SetActive(false);
            }
        }
        else if (e.mage != null && e.mage.GetState()==Mage.MageState.IDLE)
        {
            if (7 <= energy)
            {
                buildButton.gameObject.SetActive(true);
                text.text = "Build Tower";
                mage = e.mage;
            }
            else
            {
                time = 2f;
                warning.text = "Not enough Energy to build Tower!";
                warning.gameObject.SetActive(true);
                buildButton.gameObject.SetActive(false);

            }
        }
        else
        {
            buildButton.gameObject.SetActive(false);
            GameManager.SetBuildMode(false);
        }
    }

    private void OnDestroy()
    {
        GameManager.OnSelected -= GameManager_OnSelected;
        GameManager.OnEnergy -= GameManager_OnEnergy;
    }


}

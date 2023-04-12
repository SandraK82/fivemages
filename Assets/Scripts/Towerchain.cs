using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Towerchain 
{
    public List<Tower> linked = new List<Tower>();
    private GameObject gameObject;
    private LineRenderer lr;
    private Material inter;

    public Towerchain(Material m) => inter = m;

    public void Update()
    {
        if(gameObject==null)
        {
            gameObject = new GameObject("beam");
            lr = gameObject.AddComponent<LineRenderer>();
            lr.startWidth = 2f;
            lr.endWidth = 2f;
            lr.material = inter;
            lr.material.SetFloat("_dissolved", 0.3f);
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

        }

        linked.Sort();

        lr.positionCount = linked.Count;
        for(int i = 0; i < linked.Count;i++)
        {
            lr.SetPosition(i, linked[i].transform.position);
        }
    }

    public GameObject Remove()
    {
        GameObject go = gameObject;
        gameObject = null;
        return go;
    }

    public void ChangeMaterial()
    {
        if(gameObject!=null)
            lr.material.SetFloat("_dissolved", UnityEngine.Random.Range(0.25f,0.65f));
    }
}
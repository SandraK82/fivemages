using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : MonoBehaviour
{
    public static event EventHandler OnGoblinEscaped;

    [SerializeField] private Material dissolver;

    [SerializeField] private Material redWings;
    [SerializeField] private Material blueWings;
    [SerializeField] private Material whiteWings;
    [SerializeField] private Material yellowWings;
    [SerializeField] private Material greenWings;
    [ColorUsageAttribute(true, true)][SerializeField] private Color red;
    [ColorUsageAttribute(true, true)][SerializeField] private Color blue;
    [ColorUsageAttribute(true, true)][SerializeField] private Color white;
    [ColorUsageAttribute(true, true)][SerializeField] private Color yellow;
    [ColorUsageAttribute(true, true)][SerializeField] private Color green;

    [SerializeField] private List<Transform> wingObjects;
    [SerializeField] private List<Transform> eyeObjects;

    [SerializeField] private float escapeRadius = 60f;

    private int magic = 0;

    public int level = 1;

    private Vector3 target;
    private Vector3 origin;

    private float speed = .0f;

    private Dictionary<MeshRenderer, Material> originalMaterials;

    private enum GoblinState
    {
        BEAMING,
        ROTATING,
        MOVING,
    }

    private GoblinState state;
    private Vector3 dir;
    private float beamTime;
    private float beamTimeOriginal;
    private Material chosen;

    private void Start()
    {
        origin = transform.position;

        // the goblin awakes and wants to have a color/magic
        magic = UnityEngine.Random.Range(0, 5);
        SetWingColor();
        SetTarget();

        beamTimeOriginal = beamTime = 7f / level;
        state = GoblinState.BEAMING;
        originalMaterials = new Dictionary<MeshRenderer, Material>();
        PrepareBeam(this.transform);
        Debug.Log("found " + originalMaterials.Count + " renderer");
        
    }

    private void PrepareBeam(Transform t)
    {
        if(t.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
        {
            originalMaterials.Add(mr, mr.material);
            mr.material = dissolver;
        }
        for(int i = 0; i < t.childCount; i++)
        {
            PrepareBeam(t.GetChild(i));
        }
    }

    private void SetTarget()
    {
        float x = escapeRadius * Mathf.Cos(Mathf.Deg2Rad * 360f / 5f * ((float)magic));
        float y = escapeRadius * Mathf.Sin(Mathf.Deg2Rad * 360f / 5f * ((float)magic));

        target = new Vector3(x, 4, y);

        
    }

    private void SetWingColor()
    {
        chosen = redWings;
        Color c = red;
        switch (magic)
        {
            case 0:
                chosen = redWings;
                c = red;
                break;
            case 1:
                chosen = yellowWings;
                c = yellow;
                break;
            case 2:
                chosen = blueWings;
                c = blue;
                break;
            case 3:
                chosen = whiteWings;
                c = white;
                break;
            case 4:
                chosen = greenWings;
                c = green;
                break;
        }

        foreach (Transform t in wingObjects)
        {
            if (t.TryGetComponent<MeshRenderer>(out MeshRenderer r))
            {
                r.material = chosen;
            }
        }

        
        foreach (Transform t in eyeObjects)
        {
            if (t.TryGetComponent<MeshRenderer>(out MeshRenderer r))
            {
                r.material.SetColor("_Color", c);
            }
        }
    }

    private void Update()
    {
        switch (state)
        {
            case GoblinState.BEAMING:
                beamTime -= Time.deltaTime;
                if(beamTime<=0f)
                {
                    state = GoblinState.ROTATING;
                    foreach(MeshRenderer mr in originalMaterials.Keys)
                    {
                        mr.material = originalMaterials[mr];
                    }
                } else
                {
                    foreach (MeshRenderer mr in originalMaterials.Keys)
                    {
                        mr.material.SetFloat("_dissolved", beamTime / beamTimeOriginal);
                    }
                }
                

                break;
            case GoblinState.ROTATING:
                RotateGoblin();
                break;
            case GoblinState.MOVING:
                MoveGoblin();
                break;


        }
    }

    private void MoveGoblin()
    {
        speed = Mathf.Lerp(speed, 7f * level, Time.deltaTime);
        transform.position += dir * speed * Time.deltaTime;

        if (transform.position.magnitude >= escapeRadius) // the pentagram is on 0,0,0 therefore we just need our distance from zero
        {
            Debug.Log("dead!");
            Destroy(this.gameObject);
            OnGoblinEscaped?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RotateGoblin()
    {
        dir = (target - transform.position).normalized;
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        if (Mathf.Abs(angle) > 1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime);
        }
        else
        {
            state = GoblinState.MOVING;
        }
    }
}

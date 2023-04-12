using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : MonoBehaviour
{
    public static event EventHandler OnGoblinEscaped;
    public static event EventHandler OnGoblinDestroyed;

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

    private float escapeRadius = 60f;

    public int magic = 0;

    public int level = 1;

    private Vector3 target;
    private Vector3 origin;

    private float speed = .0f;

    private Dictionary<MeshRenderer, Material> originalMaterials;

    private float strengthFactor = 0.8f;
    private float attackTime = 0.5f;

    public enum GoblinState
    {
        BEAMING,
        ROTATING,
        MOVING,
        DESTROYED
    }

    private GoblinState state;
    private Vector3 dir;
    private float beamTime;
    private float beamTimeOriginal;
    private Material chosen;
    [SerializeField] private float maxDamage = 0;
    [SerializeField] private float damage = 0;

    private List<Beam> beams;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.zero, escapeRadius);

    }
    private void Start()
    {
        beams = new List<Beam>();
        origin = transform.position;

        // the goblin awakes and wants to have a color/magic
        // magic = UnityEngine.Random.Range(0, 5);
        beamTimeOriginal = beamTime = 7f / level;
        state = GoblinState.BEAMING;
        originalMaterials = new Dictionary<MeshRenderer, Material>();
        PrepareBeam(this.transform);

        maxDamage =  7f * (level * level) / 2;
        if(level>4)
        {
            maxDamage = 7f * (level * level) / (level - 3f);
        }
        damage = 0f;
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
                    SetWingColor();
                    SetTarget();
                }
                else
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
                Damage();
                break;


        }
    }

    private void Damage()
    {
        float oldDmage = damage;

        foreach(Beam b in beams)
        {
            b.damageTime -= Time.deltaTime;
            if(b.damageTime <= 0f) {
                float level = b.tower.GetLevel();
                b.damageTime = attackTime;// (7f*7f) - (level * 7f);
                int towerMagic = b.tower.GetMagic();
                
                float multiplier = b.tower.GetConnectedMultiplier();
                switch(magic)
                {
                    case 0:
                        switch(towerMagic)
                        {
                            case 2: case 3:
                                damage += level * 7f * strengthFactor * multiplier;
                                break;
                            case 1: case 4:
                                damage += 0.5f * level * 7f * strengthFactor * multiplier;
                                break;
                            case 0:
                                damage -= level * 7f * strengthFactor * multiplier;
                                break;
                        }
                        break;
                    case 1:
                        switch (towerMagic)
                        {
                            case 3:
                            case 4:
                                damage += level * 7f * strengthFactor * multiplier;
                                break;
                            case 0:
                            case 2:
                                damage += 0.5f * level * 7f * strengthFactor * multiplier;
                                break;
                            case 1:
                                damage -= level * 7f * strengthFactor * multiplier;
                                break;
                        }
                        break;
                    case 2:
                        switch (towerMagic)
                        {
                            case 0:
                            case 4:
                                damage += level * 7f * strengthFactor * multiplier;
                                break;
                            case 1:
                            case 3:
                                damage += 0.5f * level * 7f * strengthFactor * multiplier;
                                break;
                            case 2:
                                damage -= level * 7f * strengthFactor * multiplier;
                                break;
                        }
                        break;
                    case 3:
                        switch (towerMagic)
                        {
                            case 0:
                            case 1:
                                damage += level * 7f * strengthFactor * multiplier;
                                break;
                            case 2:
                            case 4:
                                damage += 0.5f * level * 7f * strengthFactor * multiplier;
                                break;
                            case 3:
                                damage -= level * 7f * strengthFactor * multiplier;
                                break;
                        }
                        break;
                    case 4:
                        switch (towerMagic)
                        {
                            case 1:
                            case 2:
                                damage += level * 7f * strengthFactor * multiplier;
                                break;
                            case 0:
                            case 3:
                                damage += 0.5f * level * 7f * strengthFactor * multiplier;
                                break;
                            case 4:
                                damage -= level * 7f * strengthFactor * multiplier;
                                break;
                        }
                        break;
                }
                /*
            case 0:
                red -> blue, white; green, yellow; red
            case 1:
                yellow -> green, white; red, blue; yellow
            case 2:
                blue -> red, green; yellow, white; blue
            case 3:
                white -> yellow, red; blue, green; white
            case 4:
                green -> blue, yellow; red, white; green
                */
            }
        }
        if (damage != oldDmage)
        {
            Debug.Log("Goblin " + transform.parent.name + " new dmg: " + damage + " old: " + this.damage+" maxDmg: "+maxDamage);
        }
        this.damage = damage;
        if (damage < 0f) this.damage = 0f;
        if (damage >= maxDamage)
        {
            OnGoblinDestroyed?.Invoke(this, EventArgs.Empty);
            state = GoblinState.DESTROYED;
            Destroy(this.transform.parent.gameObject, 5f);
        }
        if (oldDmage == 0f && damage > 0f)
        {
            originalMaterials.Clear();
            PrepareBeam(transform);
        } else if (oldDmage > 0f && damage == 0f)
        {
            foreach (MeshRenderer mr in originalMaterials.Keys)
            {
                mr.material = originalMaterials[mr];
            }
        } else if(damage > 0)
        {
            foreach (MeshRenderer mr in originalMaterials.Keys)
            {
                mr.material.SetFloat("_dissolved", damage / maxDamage);
            }
        }
    }

    private void MoveGoblin()
    {
        speed = Mathf.Lerp(speed, 7f * level, Time.deltaTime);

        float ospeed = speed;
        float nspeed = speed;

        /*foreach (Beam b in beams)
        {
            nspeed = nspeed * 2 / (b.tower.GetLevel() + 1);
        }*/
        if(beams.Count > 0)
            nspeed = nspeed / beams.Count;
        
        transform.position += dir * nspeed * Time.deltaTime;

        foreach(Beam b in beams)
        {
            b.beamObject.GetComponent<LineRenderer>().SetPosition(1, new Vector3(b.goblin.transform.position.x, 1f, b.goblin.transform.position.z));
        }
        if (transform.position.magnitude >= escapeRadius) // the pentagram is on 0,0,0 therefore we just need our distance from zero
        {
            Debug.Log("dead!");
            state = GoblinState.DESTROYED;
            Destroy(this.transform.parent.gameObject, 5f);
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

    public void AddBeam(Beam b)
    {
        beams.Add(b);
        b.beamObject = new GameObject("beam");
        LineRenderer lr = b.beamObject.AddComponent<LineRenderer>();
        lr.startWidth = 2f;
        lr.endWidth = 2f;
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(b.tower.transform.position.x, 1f, b.tower.transform.position.z));
        lr.SetPosition(1, new Vector3(b.goblin.transform.position.x, 1f, b.goblin.transform.position.z));
        lr.material = dissolver;
        switch(b.tower.GetMagic())
        {
            case 0:
                lr.material.SetColor("_Color", Color.red);
                break;
            case 1:
                lr.material.SetColor("_Color", Color.yellow);
                break;
            case 2:
                lr.material.SetColor("_Color", Color.blue);
                break;
            case 3:
                lr.material.SetColor("_Color", Color.white);
                break;
            case 4:
                lr.material.SetColor("_Color", Color.green);
                break;
        }
        switch (magic)
        {
            case 0:
                lr.material.SetColor("_EdgeColor", Color.red);
                break;
            case 1:
                lr.material.SetColor("_EdgeColor", Color.yellow);
                break;
            case 2:
                lr.material.SetColor("_EdgeColor", Color.blue);
                break;
            case 3:
                lr.material.SetColor("_EdgeColor", Color.white);
                break;
            case 4:
                lr.material.SetColor("_EdgeColor", Color.green);
                break;
        }
        lr.material.SetFloat("_dissolved", 0.4f);
        lr.receiveShadows = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void RemoveBeam(Beam b)
    {
        beams.Remove(b);
        Destroy(b.beamObject);
    }

    public GoblinState GetState() => state;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour, IComparable
{
    public static event EventHandler OnTowerOverlap;
    public static event EventHandler OnDoomsDay;

    [SerializeField] private int magic = 0;

    [SerializeField] private Material materialRed;
    [SerializeField] private Material materialGreen;
    [SerializeField] private Material materialBlue;
    [SerializeField] private Material materialWhite;
    [SerializeField] private Material materialYellow;
    [SerializeField] private MeshRenderer crystal;
    [SerializeField] private Material dissolver;

    [SerializeField] private int level = 1;

    private Dictionary<MeshRenderer, Material> originalMaterials;
    [SerializeField] private Transform rangePlane;

    public enum State
    {
        BUILDING,
        STANDING,
        UPGRADING
    };

    public State state;
    private float beamTime;
    private float beamTimeOriginal;

    private float radiusFactor = 1.75f;
    private Towerchain towerchain = null;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), 7f * level * 0.5f * radiusFactor);
    }

    private void Awake()
    {
        state = State.BUILDING;
        originalMaterials = new Dictionary<MeshRenderer, Material>();
        beamTimeOriginal = beamTime = 7f;
        beams = new Dictionary<Goblin, Beam>();

        switch (magic)
        {
            case 0:
                crystal.material = materialRed;
                rangePlane.GetComponent<MeshRenderer>().material.color = Color.red;
                break;
            case 1:
                crystal.material = materialYellow;
                rangePlane.GetComponent<MeshRenderer>().material.color = Color.yellow;
                break;
            case 2:
                crystal.material = materialBlue;
                rangePlane.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
            case 3:
                crystal.material = materialWhite;
                rangePlane.GetComponent<MeshRenderer>().material.color = Color.white;
                break;
            case 4:
                crystal.material = materialGreen;
                rangePlane.GetComponent<MeshRenderer>().material.color = Color.green;
                break;
        }

        PrepareBeam(transform);

        connectedGoblins = new List<Goblin>();
    }

    private void Update()
    {
        rangePlane.localScale = new Vector3(7f * level * 0.1f * radiusFactor, 1, 7f * level * 0.1f * radiusFactor);

        List<Goblin> removes = new List<Goblin>();
        foreach(Goblin goblin in beams.Keys)
        {
            if(!connectedGoblins.Contains(goblin))
            {
                removes.Add(goblin);
            }
        }

        foreach(Goblin goblin in removes)
        {
            Beam b = beams[goblin];
            beams.Remove(goblin);
            goblin.RemoveBeam(b);
        }

        switch(state)
        {
            case State.BUILDING:
                beamTime -= Time.deltaTime;
                if (beamTime <= 0f)
                {
                    foreach (MeshRenderer mr in originalMaterials.Keys)
                    {
                        mr.material = originalMaterials[mr];
                    }
                    state = State.STANDING;
                }
                else
                {
                    foreach (MeshRenderer mr in originalMaterials.Keys)
                    {
                        mr.material.SetFloat("_dissolved", beamTime / beamTimeOriginal);
                    }
                }
                break;
            case State.STANDING:

                break;
        }

        if (towerchain != null && towerchain.linked[0] == this) towerchain.ChangeMaterial();
    }

    private void PrepareBeam(Transform t)
    {
        if (t == rangePlane) return;    
        if (t.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
        {
            originalMaterials.Add(mr, mr.material);
            mr.material = dissolver;
        }
        for (int i = 0; i < t.childCount; i++)
        {
            PrepareBeam(t.GetChild(i));
        }
    }

    private List<Goblin> connectedGoblins;
    private Dictionary<Goblin, Beam> beams;

    public void checkOnGoblins(List<Goblin> goblins)
    {
        Vector3 pos = transform.position;

        connectedGoblins.Clear();
        foreach(Goblin goblin in goblins) {
            if (goblin.GetState() != Goblin.GoblinState.MOVING) continue;
            if((goblin.gameObject.transform.position - pos).magnitude < 7f * level * 0.5f * radiusFactor)
            {
                connectedGoblins.Add(goblin);
                if (!beams.ContainsKey(goblin))
                {
                    Beam b = new Beam();
                    b.goblin = goblin;
                    b.tower = this;
                    beams[goblin] = b;
                    goblin.AddBeam(b);
                }
            }
        }
    }

    public int GetLevel() => level;

    public int GetMagic() => magic;

    public void SetLevel(int level)
    {
        this.level = level;
        Vector3 p = transform.position;
        float r = 7f * level * radiusFactor * 0.5f;//FIXME: refakror to helper func
        foreach (Tower tower in GameManager.GetTowers())
        {
            if (tower == this) continue;

            Vector3 tp = tower.transform.position;
            float tr = 7f * tower.level * radiusFactor * 0.5f;
            if ((tp-p).magnitude < r+tr)
            {
                OnTowerOverlap?.Invoke(this, EventArgs.Empty);
                if (this.towerchain != null)
                {
                    if (tower.towerchain != null)
                    {
                        Towerchain oldchain = tower.towerchain;
                        foreach (Tower chained in tower.towerchain.linked)
                        {
                            if (!this.towerchain.linked.Contains(chained))
                            {
                                this.towerchain.linked.Add(chained);
                                chained.towerchain = towerchain;
                            }
                        }
                        Destroy(oldchain.Remove());

                    }
                    if (!this.towerchain.linked.Contains(tower))
                    {
                        this.towerchain.linked.Add(tower);
                        tower.towerchain = towerchain;
                    }
                }
                else
                {
                    if (tower.towerchain != null)
                    {
                        tower.towerchain.linked.Add(this);
                        towerchain = tower.towerchain;
                    } else
                    {
                        tower.towerchain = towerchain = new Towerchain(dissolver);
                        towerchain.linked.Add(this);
                        towerchain.linked.Add(tower);
                    }
                }
            }
        }
        if(towerchain!=null)
        {
            towerchain.Update();

            bool[] magic = new bool[5] { false, false, false, false, false };
            foreach(Tower t in towerchain.linked)
            {
                magic[t.magic] = true;
            }
            if (magic[0] && magic[1] && magic[2] && magic[3] && magic[4])
            {
                OnDoomsDay?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public int CompareTo(object obj)
    {
        Tower t = obj as Tower;

        float ta = Vector3.SignedAngle(Vector3.right, t.transform.position, Vector3.up);
        if (ta < 0) ta = 360f + ta;
        float a = Vector3.SignedAngle(Vector3.right, transform.position, Vector3.up);
        if (a < 0) a = 360f + a;
        return ta.CompareTo(a);
    }

    public float GetConnectedMultiplier()
    {
        if(towerchain!=null)
        {
            return towerchain.linked.Count;
        }
        return 1f;
    }
}

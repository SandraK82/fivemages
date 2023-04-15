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
    private Mage myMage;
    private float beamTime;
    private float beamTimeOriginal;

    public const float radiusFactor = 1.75f;
    private Towerchain towerchain = null;
    private LineRenderer builder;
    [SerializeField] private Material builderMaterial;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip upgrade;
    [SerializeField] private AudioClip beam;
    [SerializeField] private AudioClip connect;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), 7f * level * 0.5f * radiusFactor);
    }

    private void Awake()
    {
        state = State.BUILDING;
        beams = new Dictionary<Goblin, Beam>();

        builder = gameObject.AddComponent<LineRenderer>();
        builder.material = builderMaterial;
        builder.receiveShadows = false;
        builder.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        builder.enabled = false;

        connectedGoblins = new List<Goblin>();

        Deselect();

        source = GetComponent<AudioSource>();
    }

    private void Setup()
    {
        originalMaterials = new Dictionary<MeshRenderer, Material>();
        beamTimeOriginal = beamTime = 7f;
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
        crystal.material.SetFloat("_alpha", 0.4f);

        SetLevel(0);

        PrepareBeam(transform);
    }

    private void Update()
    {
        rangePlane.localScale = new Vector3(7f * level * 0.1f * radiusFactor, 1, 7f * level * 0.1f * radiusFactor);

        if(!rangePlane.gameObject.activeSelf && ( selected || GameManager.IsBuildMode() ))
        {
            rangePlane.gameObject.SetActive(true);
        } else if(rangePlane.gameObject.activeSelf && !selected && !GameManager.IsBuildMode())
        {
            rangePlane.gameObject.SetActive(false);
        }
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
                if(!builder.enabled && myMage != null)
                {
                    builder.positionCount = 2;
                    builder.SetPosition(0, transform.position);
                    builder.SetPosition(1, myMage.transform.position);
                    builder.enabled = true;
                    builder.material.SetFloat("_dissolved", 0.4f);
                    builder.startWidth = 2f;
                    builder.endWidth = 2f;
                }
                builder.material.SetFloat("_dissolved", UnityEngine.Random.Range(0.25f, 0.65f));
                if (myMage != null)
                {
                    myMage.SetBuilding(true);
                    builder.SetPosition(1, myMage.transform.position);
                }
                if (myMage != null && myMage.GetState() == Mage.MageState.BUILDING)
                {

                    beamTime -= Time.deltaTime;
                    if (beamTime <= 0f)
                    {
                        foreach (MeshRenderer mr in originalMaterials.Keys)
                        {
                            mr.material = originalMaterials[mr];
                        }
                        state = State.STANDING;
                        myMage.SetBuilding(false);
                        SetLevel(1);
                        source.Stop();
                        source.loop = false;
                        source.clip = null;
                        source.PlayOneShot(upgrade);
                    }
                    else
                    {
                        foreach (MeshRenderer mr in originalMaterials.Keys)
                        {
                            mr.material.SetFloat("_dissolved", beamTime / beamTimeOriginal);
                        }
                    }
                } else if(myMage==null)
                {
                    myMage = GameManager.GetMage(magic);
                    if(myMage && myMage.GetState() == Mage.MageState.IDLE)
                    {
                        float angle = Vector3.SignedAngle(Vector3.right, transform.position, Vector3.up);
                        angle += 360f;
                        angle %= 360;
                        myMage.Move(angle);
                        Setup();
                    } else
                    {
                        myMage = null;
                    }
                }
                break;
            case State.UPGRADING:
                if(myMage != null)
                {
                    myMage.SetBuilding(true);
                    if (!builder.enabled)
                    {
                        builder.positionCount = 2;
                        builder.SetPosition(0, transform.position);
                        builder.enabled = true;
                        builder.material.SetFloat("_dissolved", 0.4f);
                        builder.startWidth = 2f;
                        builder.endWidth = 2f;
                    }
                    builder.material.SetFloat("_dissolved", UnityEngine.Random.Range(0.25f, 0.65f));
                    builder.SetPosition(1, myMage.transform.position);

                    if (myMage.GetState()==Mage.MageState.BUILDING)
                    {
                        beamTime -= Time.deltaTime;
                        if (beamTime <= 0f)
                        {
                            foreach (MeshRenderer mr in originalMaterials.Keys)
                            {
                                mr.material = originalMaterials[mr];
                            }
                            state = State.STANDING;
                            myMage.SetBuilding(false);
                            SetLevel(level + 1);
                            source.Stop();
                            source.loop = false;
                            source.clip = null;

                            source.PlayOneShot(upgrade);
                        }
                        else
                        {
                            foreach (MeshRenderer mr in originalMaterials.Keys)
                            {
                                mr.material.SetFloat("_dissolved", ((beamTime / beamTimeOriginal)/2f));
                            }
                        }
                    }
                } else
                {
                    state = State.STANDING;
                }
                break;
            case State.STANDING:
                if (builder.enabled) builder.enabled = false;
                break;

        }

        if (towerchain != null && towerchain.linked[0] == this) towerchain.ChangeMaterial();
    }

    private void PrepareBeam(Transform t)
    {
        if (t == transform) originalMaterials.Clear();

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
        List<Goblin> removes = new List<Goblin>();
        foreach (Goblin goblin in beams.Keys)
        {
            if(!connectedGoblins.Contains(goblin))
            {
                Beam b = beams[goblin];
                goblin.RemoveBeam(b);
                removes.Add(goblin);
            }
        }
        foreach (Goblin goblin in removes)
            beams.Remove(goblin);
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

    public State GetState()
    {
        return state;
    }

    private bool selected;
    public void Select()
    {
        selected = true;
    }

    public void Deselect()
    {
        selected = false;
    }

    public void Upgrade()
    {
        state = State.UPGRADING;
        myMage = GameManager.GetMage(magic);
        float angle = Vector3.SignedAngle(Vector3.right, transform.position, Vector3.up);
        angle += 360f;
        angle %= 360;
        myMage.Move(angle);

        beamTimeOriginal = beamTime = 7f;
        PrepareBeam(transform);
        foreach (MeshRenderer mr in originalMaterials.Keys)
        {
            mr.material.SetFloat("_dissolved", 0.5f);
        }
        GameManager.RemoveEnergy(level * 7);

        source.clip = beam;
        source.loop = true;
        source.Play();
    }

    public void StartBuilding(Mage mage)
    {
        this.magic = mage.magic;
        myMage = mage;
        float angle = Vector3.SignedAngle(Vector3.right, transform.position, Vector3.up);
        angle += 360f;
        angle %= 360;
        myMage.Move(angle);

        Setup();

        source.clip = beam;
        source.loop = true;
        source.Play();
    }
}

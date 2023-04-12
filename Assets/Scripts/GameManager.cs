using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public class ScoreEventArgs : EventArgs
    {
        public int score;
    }
    public class EnergyEventArgs : EventArgs
    {
        public float energy;
        public float maxEnergy;
    }
    private static GameManager Instance;
    public static event EventHandler<ScoreEventArgs> OnScore;
    public static event EventHandler<EnergyEventArgs> OnEnergy;
    private int score = 0;

    [SerializeField] private Transform goblinPrefab;
    [SerializeField] private int level = 1;

    [SerializeField] private Transform pentagram;
    [SerializeField] private List<Transform> portals;
    [SerializeField] private Transform lightBlue;
    [SerializeField] private Transform mageBlue;
    [SerializeField] private Transform lightGreen;
    [SerializeField] private Transform mageGreen;
    [SerializeField] private Transform lightWhite;
    [SerializeField] private Transform mageWhite;
    [SerializeField] private Transform lightRed;
    [SerializeField] private Transform mageRed;
    [SerializeField] private Transform lightYellow;
    [SerializeField] private Transform mageYellow;

    [SerializeField] private float mageRadius = 58f;

    public float maxEnergy;
    public float energy;

    public enum GameState
    {
        AWAKE,
        AWAKE_MAGE_1,
        AWAKE_MAGE_2,
        AWAKE_MAGE_3,
        AWAKE_MAGE_4,
        AWAKE_MAGE_5,
        AWAKE_PORTAL,
        PLAY,
        GAME_OVER,
        DOOMSDAY
    };

    public GameState state { private set; get; }

    private float stateTime = 0;

    [SerializeField] private float minRadius = 5f;
    [SerializeField] private float maxRadius = 11f;

    private bool done = false;
    private int spawned = 0;
    private int killed = 0;
    private Dictionary<Transform, Vector3> portalPositions;

    private PlayerInput input;

    private void Awake()
    {
        Instance = this;
        Goblin.OnGoblinEscaped += OnGoblinEscaped;
        Goblin.OnGoblinDestroyed += OnGoblinDestroyed;

        input = GetComponent<PlayerInput>();
        goblins = new List<Goblin>();
        towers = new List<Tower>();

        foreach (Transform portal in portals)
        {
            portal.gameObject.SetActive(false);
        }
        Tower.OnDoomsDay += OnDoomsday;
        Tower.OnTowerOverlap += OnOverlap;
    }

    private void OnOverlap(object sender, EventArgs e)
    {
        Debug.LogError("How Dare you!");
    }

    private void OnDoomsday(object sender, EventArgs e)
    {
        Debug.LogError("doomsday!");
    }

    private void OnGoblinDestroyed(object sender, EventArgs e)
    {
        RemoveGoblin((Goblin)sender);
    }

    private void RemoveGoblin(Goblin goblin)
    {
        score += goblin.level * 7;
        OnScore?.Invoke(this, new ScoreEventArgs { score = score });
        goblins.Remove(goblin);
        killed++;
        energy = Mathf.Min(energy + goblin.level, maxEnergy);
        
        if (killed == spawned && killed > 0)
        {
            level++;
            maxEnergy += level * 7;
            spawned = killed = 0;
            Debug.Log("new game level: " + level);

            //For testing:
            foreach(Tower tower in towers)
            {
                tower.SetLevel(level);
            }
        }
        OnEnergy?.Invoke(this, new EnergyEventArgs { energy = energy,maxEnergy = maxEnergy });
    }

    private void OnGoblinEscaped(object sender, EventArgs e)
    {
        RemoveGoblin((Goblin)sender);
        Debug.LogError("dead by "+((Goblin)sender).transform.parent.name);
    }

    private void OnEnable()
    {
        input.enabled = true;
    }

    private void OnDisable()
    {
        input.enabled = false;
    }

    private List<Goblin> goblins;
    private List<Tower> towers;

    private void Start()
    {
        clickAction = input.currentActionMap.FindAction("Position");
        lightBlue.gameObject.SetActive(false);
        lightRed.gameObject.SetActive(false);
        lightYellow.gameObject.SetActive(false);
        lightWhite.gameObject.SetActive(false);
        lightGreen.gameObject.SetActive(false);

        mageBlue.gameObject.SetActive(false);
        mageRed.gameObject.SetActive(false);
        mageYellow.gameObject.SetActive(false);
        mageWhite.gameObject.SetActive(false);
        mageGreen.gameObject.SetActive(false);

        Vector3 dir = (mageBlue.transform.position - Vector3.zero);
        mageBlue.transform.position = dir * (mageRadius / dir.magnitude);

        dir = (mageRed.transform.position - Vector3.zero);
        mageRed.transform.position = dir * (mageRadius / dir.magnitude);

        dir = (mageYellow.transform.position - Vector3.zero);
        mageYellow.transform.position = dir * (mageRadius / dir.magnitude);

        dir = (mageWhite.transform.position - Vector3.zero);
        mageWhite.transform.position = dir * (mageRadius / dir.magnitude);

        dir = (mageGreen.transform.position - Vector3.zero);
        mageGreen.transform.position = dir * (mageRadius / dir.magnitude);

        state = GameState.AWAKE;
        stateTime = 1f;

        selector = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        selector.GetComponent<Collider>().enabled = false;
        selector.transform.localScale = new Vector3(4f, 4f, 4f);

        OnScore?.Invoke(this, new ScoreEventArgs { score = 0 });
        maxEnergy = 7 * 7;
        energy = maxEnergy;
        OnEnergy?.Invoke(this, new EnergyEventArgs { energy = energy, maxEnergy = maxEnergy });

        //FIXME: debug only
        foreach (Tower tower in UnityEngine.Object.FindObjectsOfType<Tower>())
        {
            towers.Add(tower);
            //For testing:
            tower.SetLevel(level);
            
        }
    }

    private void Update()
    {
        if (state < GameState.PLAY)
        {
            stateTime -= Time.deltaTime;
            if (stateTime < 0 && done)
            {
                state++;
                stateTime = 1f;
                done = false;
            }
            if (state == GameState.PLAY)
            {
                foreach (Transform portal in portals)
                {
                    portal.position = portalPositions[portal];
                }
            }
        }

        switch (state)
        {
            case GameState.AWAKE:
                if (!done)
                {
                    //Instantiate(pentagram, new Vector3(0f,-0.8f,0f), Quaternion.Euler(0,0,-90));
                    done = true;
                }
                break;

            case GameState.AWAKE_MAGE_1:
                if (!mageBlue.gameObject.activeSelf)
                {
                    mageBlue.gameObject.SetActive(true);
                }
                else if (mageBlue.GetComponent<Mage>().GetState() > Mage.MageState.AWAKE)
                {
                    lightBlue.gameObject.SetActive(true);
                    done = true;
                }
                break;
            case GameState.AWAKE_MAGE_2:
                if (!mageRed.gameObject.activeSelf)
                {
                    mageRed.gameObject.SetActive(true);
                }
                else if (mageRed.GetComponent<Mage>().GetState() > Mage.MageState.AWAKE)
                {
                    lightRed.gameObject.SetActive(true);
                    done = true;
                }
                break;
            case GameState.AWAKE_MAGE_3:
                if (!mageWhite.gameObject.activeSelf)
                {
                    mageWhite.gameObject.SetActive(true);
                }
                else if (mageWhite.GetComponent<Mage>().GetState() > Mage.MageState.AWAKE)
                {
                    lightWhite.gameObject.SetActive(true);
                    done = true;
                }
                break;
            case GameState.AWAKE_MAGE_4:
                if (!mageYellow.gameObject.activeSelf)
                {
                    mageYellow.gameObject.SetActive(true);
                }
                else if (mageYellow.GetComponent<Mage>().GetState() > Mage.MageState.AWAKE)
                {
                    lightYellow.gameObject.SetActive(true);
                    done = true;
                }
                break;
            case GameState.AWAKE_MAGE_5:
                if (!mageGreen.gameObject.activeSelf)
                {
                    mageGreen.gameObject.SetActive(true);
                }
                else if (mageGreen.GetComponent<Mage>().GetState() > Mage.MageState.AWAKE)
                {
                    lightGreen.gameObject.SetActive(true);
                    done = true;
                }
                break;
            case GameState.AWAKE_PORTAL:
                if (!done)
                {
                    portalPositions = new Dictionary<Transform, Vector3>();
                    foreach (Transform portal in portals)
                    {
                        portal.gameObject.SetActive(true);
                        portalPositions.Add(portal, portal.position);
                        portal.position = new Vector3(0, -8f, 0);
                    }
                    done = true;
                }
                else
                {
                    foreach (Transform portal in portals)
                    {
                        Vector3 dir = (portalPositions[portal] - new Vector3(0, -8f, 0)) * (1f - stateTime);
                        portal.position = new Vector3(0, -8f, 0) + dir;
                    }

                }
                break;

            case GameState.PLAY:
                //FIXME: make me better
                
                if (UnityEngine.Random.Range(0, 100) < 2 && spawned < level * 7)
                {
                    float grad = UnityEngine.Random.Range(0, 360f);
                    float rad = grad * Mathf.Deg2Rad;
                    float radius = UnityEngine.Random.Range(minRadius, maxRadius);
                    float x = radius * Mathf.Cos(rad);
                    float y = radius * Mathf.Sin(rad);

                    Vector3 pos = new Vector3(x, 4f, y);
                    Quaternion rot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);

                    Transform g = Instantiate(goblinPrefab);
                    g.transform.position = pos;
                    g.transform.rotation = rot;

                    Goblin goblin = g.GetComponentInChildren<Goblin>();
                    goblin.level = level;
                    goblin.magic = (Mathf.FloorToInt(grad / 72f) + 3) % 5;
                    goblins.Add(goblin);
                    spawned++;
                    g.name = "Goblin lvl: " + level + " #" + spawned;
                }

                foreach(Tower tower in towers)
                {
                    tower.checkOnGoblins(goblins);
                }
                break;
        }

        if (selectedMage)
        {
            selector.SetActive(true);
            Vector2 clickPoint = clickAction.ReadValue<Vector2>();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(clickPoint), out RaycastHit hit))
            {
                selector.transform.position = hit.point;

                if (hit.transform.gameObject.TryGetComponent<Mage>(out Mage mage))
                {
                    selector.GetComponent<MeshRenderer>().material.color = Color.green;
                }
                else if (hit.transform.gameObject.name == "Plane")
                {
                    if (selectedMage.GetState() == Mage.MageState.IDLE)
                    {
                        if (hit.point.magnitude <= buildRadius)
                            selector.GetComponent<MeshRenderer>().material.color = Color.green;
                        else
                            selector.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                    else
                    {
                        selector.SetActive(false);
                    }
                }
                else
                {
                    if (selectedMage.GetState() == Mage.MageState.IDLE)
                    {
                        selector.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                    else
                    {
                        selector.SetActive(false);
                    }
                }
            }
        }
        else
        {
            selector.SetActive(false);
            Vector2 clickPoint = clickAction.ReadValue<Vector2>();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(clickPoint), out RaycastHit hit))
            {
                selector.transform.position = hit.point;

                if (hit.transform.gameObject.TryGetComponent<Mage>(out Mage mage))
                {
                    if (mage != selectedMage)
                    {
                        selector.SetActive(true);

                        selector.GetComponent<MeshRenderer>().material.color = Color.green;
                    }
                }
            }
        }
    }

    private GameObject selector;

    private Mage selectedMage = null;
    private InputAction clickAction;

    [SerializeField] private float buildRadius = 0.9f * 58f;//mageRadius

    private void OnClick(InputValue value)
    {
        Vector2 clickPoint = clickAction.ReadValue<Vector2>();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(clickPoint), out RaycastHit hit))
        {
            if (hit.transform.gameObject.TryGetComponent<Mage>(out Mage mage))
            {
                if (mage != selectedMage)
                {
                    if (selectedMage)
                    {
                        selectedMage.Deselect();
                    }
                    selectedMage = mage;
                    mage.Select();
                }
            }
            else //FIXME: need to test towers
            {
                if (hit.transform.gameObject.name == "Plane")
                {
                    if (hit.point.magnitude < buildRadius && selectedMage != null && selectedMage.GetState() == Mage.MageState.IDLE)
                    {
                        /*float angle = Vector3.SignedAngle(Vector3.right, hit.point, Vector3.up);
                        angle += 360f;
                        angle %= 360;
                        
                        int sector = Mathf.FloorToInt(angle / 72f) % 5;
                        
                        float nangle = angle - (sector * 72f);
                        float shortDistance = 20.5f;//TODO: Fix this formula or forget it buildRadius * 0.525731112119134f;//sin beta / sin alpha

                        float rel = 53f - shortDistance;//TODO: soemthign fishy here

                        float dist = shortDistance;

                        if (nangle <= 36f)
                        {
                            float p = nangle / 36f; //FIXME: <-- wrong
                            dist += (1f - p) * rel;
                        }
                        else
                        {
                            float p = (nangle - 36f) / 36f;
                            dist += p * rel;
                        }

                        if (hit.point.magnitude >= dist)
                        {
                            //we are within the build radius, mark this spot for now
                            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            marker.transform.position = hit.point + (Vector3.up * 2f);
                            marker.transform.localScale = new Vector3(3f, 3f, 3f);

                            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            marker.transform.position = hit.point.normalized*dist;
                            Debug.Log("minimal: " + marker.transform.position);
                            marker.transform.localScale = new Vector3(0.1f, 6f, 0.1f);
                        }
                        */ //FIXME nice, not working, dump it

                        float angle = Vector3.SignedAngle(Vector3.right, hit.point, Vector3.up);
                        angle += 360f;
                        angle %= 360;

                        selectedMage.Move(angle);
                    }
                }
            }
        }
    }

    public static IEnumerable<Tower> GetTowers()
    {
        return Instance.towers;
    }
}

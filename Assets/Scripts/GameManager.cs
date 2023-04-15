using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public class GameStateArgs : EventArgs
    {
        public GameState state;
        public int level;
    }
    public class SelectedArgs : EventArgs
    {
        public Mage mage;
        public Tower tower;
    }
    private static GameManager Instance;
    public static event EventHandler<ScoreEventArgs> OnScore;
    public static event EventHandler<EnergyEventArgs> OnEnergy;
    public static event EventHandler<GameStateArgs> OnGameState;
    public static event EventHandler<SelectedArgs> OnSelected;
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
    [SerializeField] private Material ballMaterial;

    [SerializeField] private float mageRadius = 58f;


    [SerializeField] private TextMeshProUGUI error;
    private float errorTime = 0f;

    public float maxEnergy;
    public float currentEnergy;

    [SerializeField] private GameObject doomsday;
    [SerializeField] private GameObject gameover;
    [SerializeField] private TextMeshProUGUI scoreUI;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip click;

    [SerializeField] private GameObject quitScreen;

    public enum GameState
    {
        AWAKE,
        AWAKE_MAGE_1,
        AWAKE_MAGE_2,
        AWAKE_MAGE_3,
        AWAKE_MAGE_4,
        AWAKE_MAGE_5,
        AWAKE_PORTAL,
        PREPARE,
        PLAY,
        GAME_OVER,
        DOOMSDAY
    };

    private GameState gameState;

    private float stateTime = 0;

    [SerializeField] private float minRadius = 5f;
    [SerializeField] private float maxRadius = 11f;

    private bool done = false;
    private int spawned = 0;
    private int killed = 0;
    private Dictionary<Transform, Vector3> portalPositions;

    private PlayerInput input;

    private GameObject selector;

    private GameObject selection = null;
    private InputAction clickAction;

    [SerializeField] private float buildRadius = 0.9f * 58f;//mageRadius

    [SerializeField] private Transform towerPrefab;
    [SerializeField] private Transform hollowTower;

    private List<Mage> mages;
    private List<Goblin> goblins;
    private List<Tower> towers;
    private bool buildMode = false;

    private void Awake()
    {
        Instance = this;

        input = GetComponent<PlayerInput>();
        goblins = new List<Goblin>();
        towers = new List<Tower>();

        foreach (Transform portal in portals)
        {
            portal.gameObject.SetActive(false);
        }

        foreach(Tower t in FindObjectsOfType<Tower>())
        {
            towers.Add(t);
        }
        mages = new List<Mage>();

        selector = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        selector.GetComponent<Collider>().enabled = false;
        selector.GetComponent<MeshRenderer>().material = ballMaterial;
        selector.transform.localScale = new Vector3(4f, 4f, 4f);
    }

    private static string[] quotes = new string[] {
        "How dare you",
        "That’s no good",
        "You are all Goblins",
        "Can’t trust these Goblins"
        };

    private void OnOverlap(object sender, EventArgs e)
    {
        error.SetText("Overlapping magic!\n"+quotes[UnityEngine.Random.Range(0, quotes.Length)]);
        error.gameObject.SetActive(true);
        errorTime = 5f;
    }

    private void OnDoomsday(object sender, EventArgs e)
    {
        gameState = GameState.DOOMSDAY;
        doomsday.SetActive(true);
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
        SetEnergy(Mathf.Min(currentEnergy + (goblin.level*7), maxEnergy));

        if (killed == spawned && killed > 0)
        {
            level++;
            maxEnergy += level * 7 * 7;
            spawned = killed = 0;
            SetState(GameState.PREPARE);
        }
    }

    public void OnBackToTitle()
    {
        source.PlayOneShot(click);
        SceneManager.LoadScene(0);
    }

    public void OnQuit()
    {
        quitScreen.SetActive(true);
        source.PlayOneShot(click);
        Application.Quit();
    }

    private void SetEnergy(float value)
    {
        float old = currentEnergy;

        currentEnergy = value;

        OnEnergy?.Invoke(this, new EnergyEventArgs { energy = currentEnergy, maxEnergy = maxEnergy });
    }

    private void OnGoblinEscaped(object sender, EventArgs e)
    {
        if(gameState == GameState.PLAY)
        {
            gameState = GameState.GAME_OVER;
            gameover.SetActive(true);
            scoreUI.text = "You Scored " + (score / 7) + " x 7 Points!";
        }
    }

    private void OnEnable()
    {
        input.enabled = true;
        Goblin.OnGoblinEscaped += OnGoblinEscaped;
        Goblin.OnGoblinDestroyed += OnGoblinDestroyed;
        Tower.OnDoomsDay += OnDoomsday;
        Tower.OnTowerOverlap += OnOverlap;
    }

    private void OnDisable()
    {
        input.enabled = false;
        Goblin.OnGoblinEscaped -= OnGoblinEscaped;
        Goblin.OnGoblinDestroyed -= OnGoblinDestroyed;
        Tower.OnDoomsDay -= OnDoomsday;
        Tower.OnTowerOverlap -= OnOverlap;
    }
        
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
        mages.Add(mageBlue.GetComponent<Mage>());

        dir = (mageRed.transform.position - Vector3.zero);
        mageRed.transform.position = dir * (mageRadius / dir.magnitude);
        mages.Add(mageRed.GetComponent<Mage>());

        dir = (mageYellow.transform.position - Vector3.zero);
        mageYellow.transform.position = dir * (mageRadius / dir.magnitude);
        mages.Add(mageYellow.GetComponent<Mage>());

        dir = (mageWhite.transform.position - Vector3.zero);
        mageWhite.transform.position = dir * (mageRadius / dir.magnitude);
        mages.Add(mageWhite.GetComponent<Mage>());

        dir = (mageGreen.transform.position - Vector3.zero);
        mageGreen.transform.position = dir * (mageRadius / dir.magnitude);
        mages.Add(mageGreen.GetComponent<Mage>());

        SetState(GameState.AWAKE);
        stateTime = 1f;

        OnScore?.Invoke(this, new ScoreEventArgs { score = 0 });
        maxEnergy = 7 * 7;
        SetEnergy(maxEnergy);

    }

    private void Update()
    {
        if(errorTime >= 0f)
        {
            errorTime -= Time.deltaTime;
            if(errorTime<=0f)
            {
                error.gameObject.SetActive(false);
            }
        }
        if (gameState < GameState.PREPARE)
        {
            stateTime -= Time.deltaTime;
            if (stateTime < 0 && done)
            {
                SetState(gameState + 1);
                stateTime = 1f;
                done = false;
            }
            if (gameState == GameState.PREPARE)
            {
                foreach (Transform portal in portals)
                {
                    portal.position = portalPositions[portal];
                }
            }
        }

        switch (gameState)
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

                foreach (Tower tower in towers)
                {
                    tower.checkOnGoblins(goblins);
                }
                break;
        }

        Vector2 clickPoint = clickAction.ReadValue<Vector2>();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(clickPoint), out RaycastHit hit))
        {
            if (buildMode)
            {
                if (hit.transform.gameObject.name == "Plane")
                {
                    selector.SetActive(false);
                    hollowTower.position = hit.point;
                    hollowTower.forward = new Vector3(-hollowTower.position.x, hollowTower.position.y, -hollowTower.position.z);
                    hollowTower.gameObject.SetActive(true);
                }
                else
                {
                    hollowTower.gameObject.SetActive(false);
                    selector.SetActive(true);
                    selector.transform.position = hit.point;
                    selector.GetComponent<MeshRenderer>().material.color = Color.red;
                }

            }
            else
            {
                hollowTower.gameObject.SetActive(false);
                selector.SetActive(true);
                selector.transform.position = hit.point;

                selector.GetComponent<MeshRenderer>().material.color = Color.red;

                if (hit.transform.gameObject.TryGetComponent<Mage>(out Mage selectedMage) && selectedMage.GetState() == Mage.MageState.IDLE)
                {
                    selector.GetComponent<MeshRenderer>().material.color = Color.green;
                }
                else if (hit.transform.gameObject.TryGetComponent<Tower>(out Tower selectedTower))
                {
                    selector.GetComponent<MeshRenderer>().material.color = Color.green;
                }
                else if (selection != null && hit.transform.gameObject.name == "Plane" && selection.TryGetComponent<Mage>(out Mage mage1) && mage1.GetState() == Mage.MageState.IDLE && hit.point.magnitude <= buildRadius)
                {
                    selector.GetComponent<MeshRenderer>().material.color = Color.green;
                }
            }
        }
        else
        {
            selector.SetActive(false);
        }
    }

    public static void SetBuildMode(bool build)
    {
        Instance.buildMode = build;
    }

    private void OnClick(InputValue value)
    {
        source.PlayOneShot(click);
        Vector2 clickPoint = clickAction.ReadValue<Vector2>();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(clickPoint), out RaycastHit hit))
        {
            if (hit.transform.gameObject.TryGetComponent<Mage>(out Mage mage))
            {
                if (mage.gameObject != selection)
                {
                    if (selection != null && selection.TryGetComponent<Mage>(out Mage selectedMage))
                    {
                        selectedMage.Deselect();
                    }
                    else if (selection != null && selection.TryGetComponent<Tower>(out Tower selectedTower))
                    {
                        selectedTower.Deselect();
                    }
                    selection = mage.gameObject;
                    mage.Select();
                    OnSelected?.Invoke(this, new SelectedArgs { mage = mage });
                }
            }
            else if (hit.transform.gameObject.TryGetComponent<Tower>(out Tower tower))
            {
                if (tower.gameObject != selection)
                {
                    if (selection != null && selection.TryGetComponent<Mage>(out Mage selectedMage))
                    {
                        selectedMage.Deselect();
                    }
                    else if (selection != null && selection.TryGetComponent<Tower>(out Tower selectedTower))
                    {
                        selectedTower.Deselect();
                    }
                    selection = tower.gameObject;
                    tower.Select();
                    OnSelected?.Invoke(this, new SelectedArgs { tower = tower });
                }
            }
            else if (hit.transform.gameObject.name == "Plane")
            {
                if (hit.point.magnitude < buildRadius && selection != null && selection.TryGetComponent<Mage>(out Mage selectedMage) && selectedMage.GetState() == Mage.MageState.IDLE)
                {
                    if (buildMode)
                    {
                        float r = 7f * Tower.radiusFactor * 0.5f;
                        foreach (Tower tow in towers)
                        {
                            Vector3 tp = tow.transform.position;
                            float tr = 7f * tow.GetLevel() * Tower.radiusFactor * 0.5f;
                            if ((tp - hit.point).magnitude < r + tr)
                            {
                                WillOverlap();
                                return;
                            }
                        }
                        Transform inst = Instantiate<Transform>(towerPrefab);
                        inst.position = hit.point;
                        inst.forward = new Vector3(-inst.position.x, 0, -inst.position.z);
                        Tower t = inst.gameObject.GetComponent<Tower>();
                        towers.Add(t);
                        t.StartBuilding(selectedMage);
                        SetBuildMode(false);
                        RemoveEnergy(7);
                    }
                    else
                    {
                        float angle = Vector3.SignedAngle(Vector3.right, hit.point, Vector3.up);
                        angle += 360f;
                        angle %= 360;

                        selectedMage.Move(angle);
                    }
                }
            } else if(selection.TryGetComponent<Tower>(out Tower t))
            {
                t.Deselect();
                OnSelected?.Invoke(this, new SelectedArgs { });
                selection = null;
            }
        }
    }

    public static IEnumerable<Tower> GetTowers()
    {
        return Instance.towers;
    }

    public static void GameStateContinue()
    {
        Instance.SetState(GameState.PLAY);
    }

    private void SetState(GameState newState)
    {
        gameState = newState;
        OnGameState?.Invoke(this, new GameStateArgs { state = gameState, level = level });
    }

    public static Mage GetMage(int magic)
    {
        foreach (Mage mage in Instance.mages)
        {
            if (mage.magic == magic)
            {
                return mage;
            }
        }
        return null;
    }

    public static void RemoveEnergy(int energy)
    {
        Instance.SetEnergy(Instance.currentEnergy - energy);
    }

    public static bool IsBuildMode()
    {
        return Instance.buildMode;
    }

    public static void WillOverlap()
    {
        Instance.error.SetText("No Overlaps allowed!\n" + quotes[UnityEngine.Random.Range(0, quotes.Length)]);
        Instance.error.gameObject.SetActive(true);
        Instance.errorTime = 4f;
    }
}

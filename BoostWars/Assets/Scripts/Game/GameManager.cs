using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isDigged = false;
    public ParticleSystem hitPoint;
    public Camera mainCamera;

    public LayerMask ly;
    public LayerMask ground;
    public int pow;
    [Header("Game Objects")]
    public GameObject ExplosionParticle;
    public GameObject camButton;
    public GameObject modeButton;

    [Header("Game Info")]
    public GameObject player;
    Vector3 prevPosition;
    Vector3 initialPosition;
    Vector3 Finger;
    float V0, V0x, V0y;
    internal ArrayList PathPoints;
    internal System.Collections.Generic.LinkedList<GameObject> PointsColliders;
    public GameObject cpu;
    public int turn = 1;
    public int mode = 0;

    private Vector3 Origin;
    private Vector3 Difference;
    private Vector3 Reset;
    private bool drag = false;

    public float zoomOutMin = 1;
    public float zoomOutMax = 8;

    public Vector2[] dimCamera = new Vector2[4];

    internal const int LinePointAmount = 50;
    const float TimeAdd = 0.055f;
    bool allowMove = true;
    bool allowShot = true;
    bool firstShot = true;
    public GameObject[] BulletTypes;
    public Sprite[] playerSprites;
    public Sprite[] cpuSprites;
    bool moved = false;
    bool firstMove = true;
    bool CPUshot = false;
    public bool nukeLock = false;

    internal float parabolTime, parabolVx, parabolVy, parabolXStep, parabolX, parabolH;
    private float timeLessColides, vXLessColides, vYLessColides, xStepLessColides, xLessColides, hLessColides;
    internal bool CanErase = false;
    internal Vector3 parabolP1;
    private int noCollides = 0;
    internal bool canStart = false;
    private int MaxShownLinePoint = -1;
    
    public bool cpuHasMove = false;
    public bool cpuIsMoving = false;

    public int actualWeaponPlayer = -1;
    public int actualWeaponCpu = -1;

    public List<WeaponInfo> weaponList;

    public GameObject spawnParent;

    public bool deadEvent = false;

    private GameObject EndgameCanvas;
    public GameObject TutorialCanvas;

    private bool moveError = true;

    public bool isGamePaused = false;

    public Sprite[] imageModes;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        weaponList = new List<WeaponInfo>();
        PathPoints = new ArrayList();
        PointsColliders = new System.Collections.Generic.LinkedList<GameObject>();
        EndgameCanvas = GameObject.Find("EndgameCanvas");
        for (int i = 0; i < LinePointAmount; i++)
        {
            GameObject go = new GameObject();
            go.name = i + "";
            go.AddComponent<BoxCollider>();
            go.AddComponent<Rigidbody>();
            go.AddComponent<LineManager>();
            go.layer = 6;
            PointsColliders.AddLast(go);
        }


        StartCoroutine(camLimit());
        StartCoroutine(LocatePlayers());
    }

    private IEnumerator LocatePlayers()
    {
        yield return new WaitForEndOfFrame();
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < spawnParent.transform.childCount; i++)
        {
            if (spawnParent.transform.GetChild(i).gameObject.GetComponent<WeaponManager>().canPlayerSpawn)
            {
                positions.Add(spawnParent.transform.GetChild(i).position);
            }
        }
        int position = Random.Range(0, positions.Count);
        player.transform.position = new Vector3(positions[position].x, positions[position].y, player.transform.position.z);
        positions.RemoveAt(position);
        position = Random.Range(0, positions.Count);
        cpu.transform.position = new Vector3(positions[position].x, positions[position].y, player.transform.position.z);
        yield break;
    }

    private IEnumerator camLimit()
    {
        while (Data.ter == null)
        {
            yield return new WaitForFixedUpdate();
        }

        int xlimit = (int)Data.ter.terrainParts.x;
        int ylimit = (int)Data.ter.terrainParts.y;

        var limit0 = Data.ter.terrains[0,0].mc.transform.position;
        var limit1 = Data.ter.terrains[xlimit - 1, 0].mc.transform.position;
        var limit2 = Data.ter.terrains[0, ylimit - 1].mc.transform.position;
        var limit3 = Data.ter.terrains[xlimit - 1, ylimit - 1].mc.transform.position;
        dimCamera[0] = limit0;
        dimCamera[1] = limit1;
        dimCamera[2] = limit2;
        dimCamera[3] = limit3;
    }

    private void waitifMoveError()
    {
        if (moveError)
        {
            if(cpu.GetComponent<Rigidbody>().velocity.magnitude < 0.01f && cpu.GetComponent<Rigidbody>().velocity.magnitude > -0.01f)
            {
                cpuHasMove = true;
            }
        }
    }

    void Update()
    {
        //Camera
        if (mode == 0 && turn < 6 && !isGamePaused)
        {
            firstMove = true;
            if(turn == 1)
            {
                cpu.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = cpuSprites[0];
                cpu.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                modeButton.SetActive(true);
            }
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                zoom(difference * 0.01f);
            } else if (Input.GetMouseButton(0))
            {
                Difference = (mainCamera.ScreenToWorldPoint(Input.mousePosition)) - mainCamera.transform.position;
                if(drag == false)
                {
                    drag = true;
                    Origin = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                }
            }
            else
            {
                drag = false;
            }

            if (drag)
            {
                Vector2 resul = Origin - Difference;
                if((resul.x > dimCamera[0].x && resul.y > dimCamera[0].y) && (resul.x < dimCamera[1].x && resul.y > dimCamera[1].y) && (resul.x > dimCamera[2].x && resul.y < dimCamera[2].y + 6) && (resul.x < dimCamera[3].x && resul.y < dimCamera[3].y + 6))
                {
                    mainCamera.transform.position = Origin - Difference;
                    mainCamera.GetComponent<CameraManager>().ModifyCoords(Difference);
                }
            }
        }

        if (!nukeLock)
        {
            //Movement
            if (mode == 1 && turn == 1)
            {
                firstShot = true;

                if (Input.touchCount > 1 && !firstMove)
                {
                    allowMove = false;
                }

                if (Input.GetMouseButtonUp(0) && allowMove && !firstMove)
                {
                    player.GetComponent<Player>().getLineRenderer().enabled = false;
                    PathPoints.Clear();
                    player.GetComponent<Rigidbody>().velocity = new Vector3(V0x, V0y, 0f);
                    moved = true;
                    changeMode();

                }

                if (Input.GetMouseButtonDown(0))
                {
                    firstMove = false;
                    allowMove = true;
                    Finger = Input.mousePosition;
                    //StopMouseInput = false;
                    GetComponent<LineRenderer>().enabled = true;
                }

                if (Input.GetMouseButton(0) && allowMove)
                {
                    player.GetComponent<Player>().getLineRenderer().enabled = true;
                    prevPosition = player.transform.position;
                    initialPosition = player.transform.position;
                    Vector2 V0 = Finger - Input.mousePosition;
                    V0x = V0.x / 50;
                    V0y = V0.y / 50;
                    PathPoints.Clear();

                    float t = 0;
                    for (int i = 0; i < LinePointAmount; i++)
                    {
                        float xPos = V0x * t + initialPosition.x;
                        float yPos = 0.5f * (-9.8f) * math.pow(t, 2) + V0y * t + initialPosition.y;

                        float xPosLate = V0x * (t + TimeAdd) + initialPosition.x;
                        float yPosLate = 0.5f * (-9.8f) * math.pow((t + TimeAdd), 2) + V0y * (t + TimeAdd) + initialPosition.y;

                        float size = math.sqrt(math.pow(xPosLate - xPos, 2) + math.pow(yPosLate - yPos, 2));

                        if (i != 0)
                        {
                            PointsColliders.ElementAt(i).GetComponent<BoxCollider>().size = new Vector2(0.28f, size);
                        }
                        else
                        {
                            PointsColliders.ElementAt(i).GetComponent<BoxCollider>().size = new Vector2(0.28f, size / 10);
                        }
                        float angle = Mathf.Rad2Deg * Mathf.Atan((xPosLate - xPos) / (yPosLate - yPos));
                        PointsColliders.ElementAt(i).transform.eulerAngles = new Vector3(0, 0, -angle);
                        PointsColliders.ElementAt(i).transform.position = new Vector3(xPos, yPos, 0);

                        if (PlaceFree(i))
                        {
                            PathPoints.Add(new Vector3(xPos, yPos, 0));
                            t += TimeAdd;
                        }
                        else
                        {
                            break;
                        }

                        prevPosition = new Vector3(xPos, yPos, 0);

                    }
                    player.GetComponent<Player>().getLineRenderer().positionCount = PathPoints.Count;
                    player.GetComponent<Player>().getLineRenderer().SetPositions(PathPoints.ConvertTo<Vector3[]>());
                }
            }

            //Shooting
            if (mode == 2 && turn == 1)
            {
                if (Input.touchCount > 1 && !firstShot)
                {
                    allowShot = false;
                }

                if (Input.GetMouseButtonUp(0) && allowShot && !firstShot)
                {
                    mode = 0;
                    modeButton.SetActive(false);
                    player.GetComponent<Player>().getLineRenderer().enabled = false;
                    PathPoints.Clear();
                    player.GetComponentInChildren<ParticleSystem>().Play();
                    GameObject go = Instantiate(BulletTypes[actualWeaponPlayer + 1]);
                    go.transform.position = player.transform.position;
                    go.GetComponent<Rigidbody>().velocity = new Vector3(V0x, V0y, 0f);
                    go.GetComponent<Projectile>().ShooterPosition = player.transform.position;
                    go.GetComponent<Projectile>().creatorTag = player.gameObject.tag;
                    DontGoThroughThings dGo = go.AddComponent<DontGoThroughThings>();
                    dGo.layerMask = ground;
                    mainCamera.GetComponent<CameraManager>().ChangeFollowObject(go, cpu);
                    cpuHasMove = false;
                    CPUshot = false;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    firstShot = false;
                    PointerEventData pointer = new PointerEventData(EventSystem.current);
                    pointer.position = Input.mousePosition;

                    List<RaycastResult> raycastResults = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, raycastResults);

                    bool cancel = false;
                    if (raycastResults.Count > 0)
                    {
                        foreach (var go in raycastResults)
                        {
                            if (go.gameObject.Equals(camButton) || go.gameObject.Equals(modeButton))
                            {
                                cancel = true;
                            }
                        }
                    }
                    if (!cancel)
                    {
                        allowShot = true;
                        Finger = Input.mousePosition;
                        //StopMouseInput = false;
                        GetComponent<LineRenderer>().enabled = true;
                    }
                    else
                    {
                        allowShot = false;
                    }
                }

                else if (Input.GetMouseButton(0) && allowShot)
                {
                    player.GetComponent<Player>().getLineRenderer().enabled = true;
                    prevPosition = player.transform.position;
                    initialPosition = player.transform.position;
                    Vector2 V0 = Finger - Input.mousePosition;
                    V0x = V0.x / 50;
                    V0y = V0.y / 50;
                    PathPoints.Clear();

                    float t = 0;
                    float g = -9.8f;
                    if (actualWeaponPlayer + 3 == 7)
                    {
                        g = 0f;
                    }
                    for (int i = 0; i < LinePointAmount; i++)
                    {
                        float xPos = V0x * t + initialPosition.x;
                        float yPos = 0.5f * (g) * math.pow(t, 2) + V0y * t + initialPosition.y;

                        float xPosLate = V0x * (t + TimeAdd) + initialPosition.x;
                        float yPosLate = 0.5f * (g) * math.pow((t + TimeAdd), 2) + V0y * (t + TimeAdd) + initialPosition.y;

                        float size = math.sqrt(math.pow(xPosLate - xPos, 2) + math.pow(yPosLate - yPos, 2));

                        if (i != 0)
                        {
                            PointsColliders.ElementAt(i).GetComponent<BoxCollider>().size = new Vector2(0.28f, size);
                        }
                        else
                        {
                            PointsColliders.ElementAt(i).GetComponent<BoxCollider>().size = new Vector2(0.28f, size / 10);
                        }
                        float angle = Mathf.Rad2Deg * Mathf.Atan((xPosLate - xPos) / (yPosLate - yPos));
                        print(-angle);
                        PointsColliders.ElementAt(i).transform.eulerAngles = new Vector3(0, 0, -angle);
                        PointsColliders.ElementAt(i).transform.position = new Vector3(xPos, yPos, 0);

                        if (i == 2)
                        {
                            moveSprites(i);
                        }

                        if (PlaceFree(i))
                        {
                            PathPoints.Add(new Vector3(xPos, yPos, 0));
                            t += TimeAdd;
                        }
                        else
                        {
                            break;
                        }

                        prevPosition = new Vector3(xPos, yPos, 0);

                    }
                    player.GetComponent<Player>().getLineRenderer().positionCount = PathPoints.Count;
                    player.GetComponent<Player>().getLineRenderer().SetPositions(PathPoints.ConvertTo<Vector3[]>());
                }
            }

            //cpu turn: AI
            if (turn == 3)
            {
                modeButton.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = imageModes[1];
                if (!CPUshot)
                {
                    if (!cpuHasMove && !cpuIsMoving)
                    {
                        player.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = playerSprites[0];
                        player.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                        cpuIsMoving = true;
                        StartCoroutine(cpuMoveAI());
                        moveError = true;
                        Invoke("waitifMoveError", 10);
                    }
                    else if (cpuHasMove)
                    {
                        moveError = false;
                        cpuIsMoving = false;
                        player.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = playerSprites[0];
                        player.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                        cpu.GetComponent<Player>().getLineRenderer().SetPositions(new Vector3[] { new Vector3(0f, 0f, 0f) });
                        CPUshot = true;
                        cpu.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = cpuSprites[1];
                        cpu.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                        cpu.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = cpuSprites[actualWeaponCpu + 3];
                        StartCoroutine(cpuShotAI());
                    }
                }
                moved = false;
            }

            if (turn == 7 && !deadEvent)
            {
                deadEvent = true;
                StartCoroutine(manageEnd());
            }
        }
    }

    private IEnumerator checkBoost()
    {
        weaponList.Sort((p1, p2) => Vector2.Distance(p1.position, cpu.transform.position).CompareTo(Vector2.Distance(p2.position, cpu.transform.position)));
        if(weaponList[0].tier <= cpu.GetComponent<CpuManager>().weaponTier)
        {
            yield break;
        }
        //get the best point -> with less collisions between them
        Vector2 point = weaponList[0].position;
        float V0a, V0b, parabolVa, parabolVb, parabolTime;
        float a = -(cpu.transform.position.x - point.x);
        float b = -(cpu.transform.position.y - point.y);
        float AStep = a / PointsColliders.Count;

        List<float> timeList = new List<float>();
        List<Vector2> pointList = new List<Vector2>();

        for (float t = 0.2f; t < ((((Mathf.Sqrt(Mathf.Pow(b, 2) + Mathf.Pow(a, 2)))) / 4) + Math.Abs(b / 50)); t += 0.05f)
        {
            b = -(cpu.transform.position.y - point.y);
            b = b / t;
            V0a = a / t;
            V0b = (b + (0.5f * 9.8f * Mathf.Pow(t, 2)) / t);
            parabolVa = a / t;
            parabolVb = (b + (0.5f * 9.8f * Mathf.Pow(t, 2)) / t);
            float time = (AStep) / V0a;
            parabolTime = time;

            //int collisionsCount = 0; 

            for (int i = 0; i < PointsColliders.Count; i++)
            {
                float xPos = AStep * i + cpu.transform.position.x;
                float yPos = 0.5f * (-9.8f) * math.pow(parabolTime * i, 2) + parabolVb * (parabolTime * i) + cpu.transform.position.y;

                float xPosNext = AStep * (i + 1) + cpu.transform.position.x;
                float yPosNext = 0.5f * (-9.8f) * math.pow(parabolTime * (i + 1), 2) + parabolVb * (parabolTime * (i + 1)) + cpu.transform.position.y;

                float size = math.sqrt(math.pow(xPosNext - xPos, 2) + math.pow(yPosNext - yPos, 2));
                float angle = Mathf.Rad2Deg * Mathf.Atan((xPosNext - xPos) / (yPosNext - yPos));

                List<RaycastHit> collisionList = Raycast(new Vector2(xPos, yPos), new Vector2(xPosNext - xPos, yPosNext - yPos), size, 10f);
                if (collisionList.Count() > 0 && PointsColliders.Count() - 1 > i)
                {
                    break;
                }
                else if (i == PointsColliders.Count() - 1)
                {
                    timeList.Add(t);
                    pointList.Add(point);
                }

            }
        }
        int larger = 0;
        int cont = 0;
        int timeLarger = 0;
        float t2 = 0f;
        try
        {
            for (int j = 0; j < timeList.Count() - 1; j++)
            {
                float value1 = ((float)((int)((timeList[j] + 0.05f) * 100000))) / 100000;
                float value2 = ((float)((int)((timeList[j + 1]) * 100000))) / 100000;
                if (Math.Round(value1,3) == Math.Round(value2,3))
                {
                    cont++;
                }
                else
                {
                    if (cont > larger)
                    {
                        larger = cont;
                        timeLarger = j;
                    }
                    cont = 0;
                }
                if (j == timeList.Count() - 2)
                {
                    if (timeLarger == 0)
                    {
                        larger = cont;
                        timeLarger = j;
                    }
                }
            }
            t2 = timeList[timeLarger - (int)(larger / 2)];
            V0a = a / t2;
            V0b = (b + (0.5f * 9.8f * Mathf.Pow(t2, 2)) / t2);
            Time.timeScale = 1f;
            if (timeLarger != 0)
            {
                agroWeapon = true;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
        }
        if (agroWeapon)
        {
            yield return StartCoroutine(ShowMoveLine(t2, point, a, AStep));
        }
        yield break;
    }

    private bool agroWeapon = false;

    private IEnumerator cpuMoveAI()
    {
        if (weaponList.Count >= 1)
        {
            yield return StartCoroutine(checkBoost());
            if (agroWeapon)
            {
                agroWeapon = false;
                yield break;
            }
        }
        bool moving = false;
        bool isInMyX = false;
        //in case cpu is near to the player -> dont move
        float xDistance = Mathf.Abs(cpu.transform.position.x - player.transform.position.x);
        switch (xDistance)
        {
            case > 9.8f:
                break;
            case < 2.5f:
                isInMyX = true;
                break;
            default:
                cpuHasMove = true;
                yield break;
        }
        //if (cpu.transform.position.x < player.transform.position.x + 9.8f && cpu.transform.position.x > player.transform.position.x - 9.8f)
        //{
        //    cpuHasMove = true;
        //    yield break;
        //}

        //Delimeter move created:
        //Possible point on the player's right side
        Vector3 posFinale = new Vector3(player.transform.position.x + 9.5f, 100000, player.transform.position.z);
        List<RaycastHit> rightColls = Raycast(posFinale, Vector3.down, Mathf.Infinity, 30f);
        posFinale = new Vector3(player.transform.position.x + 9.5f, -100000, player.transform.position.z);
        List<RaycastHit> rightCollsNegative = Raycast(posFinale, -Vector3.down, Mathf.Infinity, 30f);

        //Possible points on the player's left side
        Vector3 posInitiale = new Vector3(player.transform.position.x - 9.5f, 100000, player.transform.position.z);
        List<RaycastHit> leftColls = Raycast(posInitiale, Vector3.down, Mathf.Infinity, 30f);
        posInitiale = new Vector3(player.transform.position.x - 9.5f, -100000, player.transform.position.z);
        List<RaycastHit> leftCollsNegative = Raycast(posInitiale, -Vector3.down, Mathf.Infinity, 30f);

        //Sorting collider points
        rightColls.Sort((p1, p2) => p1.point.y.CompareTo(p2.point.y));
        rightCollsNegative.Sort((p1, p2) => p1.point.y.CompareTo(p2.point.y));

        leftColls.Sort((p1, p2) => p1.point.y.CompareTo(p2.point.y));
        leftCollsNegative.Sort((p1, p2) => p1.point.y.CompareTo(p2.point.y));

        List<Vector2> validPosRight = new List<Vector2>();
        List<Vector2> validPosLeft = new List<Vector2>();


        //Check if theyre not the same --> save the point (right)
        try
        {
            Debug.DrawRay(rightColls[rightColls.Count() - 1].point, -Vector3.right, Color.magenta, 30f);
            for (int i = 0; i < rightColls.Count() - 1; i++)
            {
                if (Math.Round(rightColls[i].point.y, 2) != Math.Round(rightCollsNegative[i + 1].point.y, 2))
                {
                    Debug.DrawRay(rightColls[i].point, -Vector3.right, Color.magenta, 30f);
                    Debug.DrawRay(rightCollsNegative[i + 1].point, -Vector3.right, Color.magenta, 30f);
                    if (rightCollsNegative[i + 1].point.y - rightColls[i].point.y > 1.3f)
                    {
                        validPosRight.Add(rightColls[i].point);
                    }
                }
            }
            //Saving the top
            validPosRight.Add(rightColls[rightColls.Count() - 1].point);
        }
        catch (ArgumentOutOfRangeException)
        {

        }

        //Check if theyre not the same --> save the point (left)
        try
        {
            Debug.DrawRay(leftColls[leftColls.Count() - 1].point, -Vector3.right, Color.magenta, 30f);
            for (int i = 0; i < leftColls.Count() - 1; i++)
            {
                if (Math.Round(leftColls[i].point.y, 2) != Math.Round(leftCollsNegative[i + 1].point.y, 2))
                {
                    Debug.DrawRay(leftColls[i].point, -Vector3.right, Color.magenta, 30f);
                    Debug.DrawRay(leftCollsNegative[i + 1].point, -Vector3.right, Color.magenta, 30f);
                    if (leftCollsNegative[i + 1].point.y - leftColls[i].point.y > 1.3f)
                    {
                        validPosLeft.Add(leftColls[i].point);
                    }
                }
            }
            //Saving the top
            validPosLeft.Add(leftColls[leftColls.Count() - 1].point);
        }
        catch (ArgumentOutOfRangeException)
        {

        }
        if (validPosLeft.Count() == 0 && validPosRight.Count() == 0)
        {
            cpuHasMove = true;
            yield break;
        }

        List<Vector2> validPos = new List<Vector2>();

        if (!isInMyX)
        {
            if (player.transform.position.x < cpu.transform.position.x)
            {
                validPos = validPosRight;
            }
            else
            {
                validPos = validPosLeft;
            }
        }
        else
        {
            if (player.transform.position.x < cpu.transform.position.x)
            {
                validPos.AddRange(validPosRight);
                validPos.AddRange(validPosLeft);
            }
            else
            {
                validPos.AddRange(validPosLeft);
                validPos.AddRange(validPosRight);
            }
        }

        //get the best point -> with less collisions between them
        foreach(Vector2 point in validPos)
        {
            float V0a, V0b, parabolVa, parabolVb, parabolTime;
            float a = -(cpu.transform.position.x - point.x);
            float b = -(cpu.transform.position.y - point.y);
            float AStep = a / PointsColliders.Count;

            List<float> timeList = new List<float>();
            List<Vector2> pointList = new List<Vector2>();

            for (float t = 0.2f; t < ((((Mathf.Sqrt(Mathf.Pow(b, 2) + Mathf.Pow(a, 2)))) / 4) + Math.Abs(b / 50)); t += 0.05f)
            {
                b = -(cpu.transform.position.y - point.y);
                b = b / t;
                V0a = a / t;
                V0b = (b + (0.5f * 9.8f * Mathf.Pow(t, 2)) / t);
                parabolVa = a / t;
                parabolVb = (b + (0.5f * 9.8f * Mathf.Pow(t, 2)) / t);
                float time = (AStep) / V0a;
                parabolTime = time;

                //int collisionsCount = 0; 

                for (int i = 0; i < PointsColliders.Count; i++)
                {
                    float xPos = AStep * i + cpu.transform.position.x;
                    float yPos = 0.5f * (-9.8f) * math.pow(parabolTime * i, 2) + parabolVb * (parabolTime * i) + cpu.transform.position.y;

                    float xPosNext = AStep * (i + 1) + cpu.transform.position.x;
                    float yPosNext = 0.5f * (-9.8f) * math.pow(parabolTime * (i + 1), 2) + parabolVb * (parabolTime * (i + 1)) + cpu.transform.position.y;

                    float size = math.sqrt(math.pow(xPosNext - xPos, 2) + math.pow(yPosNext - yPos, 2));
                    float angle = Mathf.Rad2Deg * Mathf.Atan((xPosNext - xPos) / (yPosNext - yPos));

                    List<RaycastHit> collisionList = Raycast(new Vector2(xPos, yPos), new Vector2(xPosNext - xPos, yPosNext - yPos), size, 10f);
                    if (collisionList.Count() > 0 && PointsColliders.Count() - 1 > i)
                    {
                        break;
                    }
                    else if(i == PointsColliders.Count() - 1)
                    {
                        timeList.Add(t);
                        pointList.Add(point);
                    }

                }
            }
            int larger = 0;
            int cont = 0;
            int timeLarger = 0;
            try
            {
                for (int j = 0; j < timeList.Count() - 1; j++)
                {
                    float value1 = ((float)((int)((timeList[j] + 0.05f) * 100000))) / 100000;
                    float value2 = ((float)((int)((timeList[j + 1]) * 100000))) / 100000;
                    if (Math.Round(value1,3) == Math.Round(value2,3))
                    {
                        cont++;
                    }
                    else
                    {
                        if (cont > larger)
                        {
                            larger = cont;
                            timeLarger = j;
                        }
                        cont = 0;
                    }
                    if(j == timeList.Count() - 2)
                    {
                        if (timeLarger == 0)
                        {
                            larger = cont;
                            timeLarger = j;
                        }
                    }
                }
                float t2 = timeList[timeLarger - (int)(larger / 2)];
                V0a = a / t2;
                V0b = (b + (0.5f * 9.8f * Mathf.Pow(t2, 2)) / t2); 
                Time.timeScale = 1f;
                if (timeLarger != 0)
                {
                    moving = true;
                    StartCoroutine(ShowMoveLine(t2, point, a, AStep));

                    break;
                }
            }
            catch(ArgumentOutOfRangeException)
            {
            }   
        }

        if (!moving)
        {
            yield return new WaitForSeconds(6f);
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 1f;
            yield return new WaitForSeconds(4f);
            while (cpu.GetComponent<Rigidbody>().velocity.magnitude >= 0.01f)
            {
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(1.5f);
        }
        yield return new WaitForSeconds(0.5f);
        cpuHasMove = true;
        yield break;
    }

    private IEnumerator ShowMoveLine(float t2, Vector2 point, float a, float AStep)
    {
        yield return new WaitForSeconds(0.75f);
        float b = -(cpu.transform.position.y - point.y);
        b = b / t2;
        float V0a = a / t2;
        float V0b = (b + (0.5f * 9.8f * Mathf.Pow(t2, 2)) / t2);
        float parabolVa = a / t2;
        float parabolVb = (b + (0.5f * 9.8f * Mathf.Pow(t2, 2)) / t2);
        float time = (AStep) / V0a;
        float parabolTime = time;

        //int collisionsCount = 0; 
        List<Vector3> safePoints = new List<Vector3>();
        for (int i = 0; i < PointsColliders.Count; i++)
        {
            float xPos = AStep * i + cpu.transform.position.x;
            float yPos = 0.5f * (-9.8f) * math.pow(parabolTime * i, 2) + parabolVb * (parabolTime * i) + cpu.transform.position.y;

            float xPosNext = AStep * (i + 1) + cpu.transform.position.x;
            float yPosNext = 0.5f * (-9.8f) * math.pow(parabolTime * (i + 1), 2) + parabolVb * (parabolTime * (i + 1)) + cpu.transform.position.y;

            float size = math.sqrt(math.pow(xPosNext - xPos, 2) + math.pow(yPosNext - yPos, 2));
            float angle = Mathf.Rad2Deg * Mathf.Atan((xPosNext - xPos) / (yPosNext - yPos));

            safePoints.Add(new Vector3(xPos, yPos, 0f));
        }

        cpu.GetComponent<Player>().getLineRenderer().enabled = true;
        cpu.GetComponent<Player>().getLineRenderer().positionCount = safePoints.Count;
        Vector3[] positions = safePoints.GetRange(0, safePoints.Count).ConvertTo<Vector3[]>();
        cpu.GetComponent<Player>().getLineRenderer().SetPositions(positions);
        yield return new WaitForSeconds(2.5f);
        cpu.GetComponent<Player>().getLineRenderer().enabled = false;
        cpu.GetComponent<Rigidbody>().velocity = (new Vector3(V0a, V0b, 0f));
        StartCoroutine(cpu.GetComponent<CpuManager>().stopCPU());
        yield break;
    }

    //Lines which calculates collision with terrain. z 0.888 works like a layer, where le lines are created. player and cpu are in 0.4, then dont collide with the lines
    public List<RaycastHit> Raycast(Vector3 pos, Vector3 direction, float length, float time)
    {
        RaycastHit[] hitAll;

        //pos.z = 0.888f;

        hitAll = Physics.RaycastAll(pos, direction, length, ground);

        //if (DrawDebugRaycasts)
        //{
        Color c = Color.red;

        List<RaycastHit> hit = new List<RaycastHit>();
        foreach (RaycastHit item in hitAll)
        {
            if(item.collider.gameObject.name.Equals("Quad"))
            {
                hit.Add(item);
                c = Color.green;
            }
        }
        //Color color = (hit != null) ? Color.red : Color.green;
        Debug.DrawRay(pos, direction, c, time);
        //}
        return hit;
    }


    private IEnumerator cpuShotAI()
    {
        Vector2 p1 = cpu.transform.position;
        Vector2 p2 = player.transform.position;
        float g = 9.8f;
        

        //CPU Miss shot probability, bigger number = bigger missing chance - Class GameSettings
        p2 = new Vector2(p2.x + Random.Range(-GameSettings.getDifficultyMod(), GameSettings.getDifficultyMod()), p2.y + Random.Range(-GameSettings.getDifficultyMod(), GameSettings.getDifficultyMod()));

        parabolP1 = p1;
        cpu.GetComponent<Player>().getLineRenderer().SetPositions(new Vector3[] { new Vector3(-100, -100, -100) });
        //cpu.GetComponent<Player>().getLineRenderer().enabled = true;
        float x = p2.x - p1.x;
        float h = p2.y - p1.y;

        if (actualWeaponCpu + 3 == 7)
        {
            g = 0.0f;
            float t = 0f;
            Vector2 V0 = p2 - p1;

            float distanceX = V0.x / LinePointAmount;
            float distanceY = V0.y / LinePointAmount;

            PathPoints.Clear();

            for (int i = 0; i < LinePointAmount; i++)
            {
                float xPos = p1.x + (distanceX * i);
                float yPos = p1.y + (distanceY * i);

                float xPosLate = p1.x + (distanceX * (i + 1));
                float yPosLate = p1.y + (distanceY * (i + 1));

                float size = math.sqrt(math.pow(xPosLate - xPos, 2) + math.pow(yPosLate - yPos, 2));
                float angle = Mathf.Rad2Deg * Mathf.Atan((xPosLate - xPos) / (yPosLate - yPos));

                if (i != 0)
                {
                    PointsColliders.ElementAt(i).GetComponent<BoxCollider>().size = new Vector2(0.28f, size);
                }
                else
                {
                    PointsColliders.ElementAt(i).GetComponent<BoxCollider>().size = new Vector2(0.28f, size / 10);
                }

                PointsColliders.ElementAt(i).transform.eulerAngles = new Vector3(0, 0, -angle);
                PointsColliders.ElementAt(i).transform.position = new Vector3(xPos, yPos, 0);

                if (i == 2)
                {
                    moveSprites(i);
                }

                if (PlaceFree(i))
                {
                    PathPoints.Add(new Vector3(xPos, yPos, 0));
                    t += TimeAdd;
                }
                else
                {
                    break;
                }

                prevPosition = new Vector3(xPos, yPos, 0);
            }

            cpu.GetComponent<Player>().getLineRenderer().positionCount = PathPoints.Count;
            Vector3[] positions = PathPoints.GetRange(0, PathPoints.Count).ConvertTo<Vector3[]>();
            cpu.GetComponent<Player>().getLineRenderer().SetPositions(positions);
            cpu.GetComponent<Player>().getLineRenderer().enabled = true;
            canStart = true;
            //1 is the time in parabol eq
            float xPosTemp = p1.x + (distanceX);
            float yPosTemp = p1.y + (distanceY);
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(1f);
            canStart = false;
            yield return new WaitForSeconds(4.5f);
            CPUshot = false;
            cpu.GetComponent<Player>().getLineRenderer().enabled = false;

            GameObject go = Instantiate(BulletTypes[actualWeaponCpu + 1]);
            go.transform.position = cpu.transform.position;
            go.GetComponent<Rigidbody>().velocity = new Vector3(V0.x, V0.y, 0f);
            go.GetComponent<Projectile>().ShooterPosition = cpu.transform.position;
            go.GetComponent<Projectile>().creatorTag = cpu.gameObject.tag;
            DontGoThroughThings dGo = go.AddComponent<DontGoThroughThings>();
            dGo.layerMask = ground;
            mainCamera.GetComponent<CameraManager>().ChangeFollowObject(go, player);
        }
        else
        {

            float XStep = x / PointsColliders.Count;
            parabolXStep = XStep;
            noCollides = 0;

            //https://youtu.be/hwVpNDBEf40?t=254

            for (float t = 0.2f; t < ((((Mathf.Sqrt(Mathf.Pow(h, 2) + Mathf.Pow(x, 2)))) / 4) + Math.Abs(h / 50)); t += 0.01f)
            {
                x = p2.x - p1.x;
                h = p2.y - p1.y;
                h = h / t;

                V0x = x / t;
                V0y = (h + (0.5f * g * Mathf.Pow(t, 2)) / t);

                parabolVx = x / t;
                parabolVy = (h + (0.5f * g * Mathf.Pow(t, 2)) / t);

                PathPoints.Clear();
                float time = (XStep) / V0x;

                parabolTime = time;
                canStart = true;
                //1 is the time in parabol eq
                float xPosTemp = parabolXStep * 1 + parabolP1.x;
                float yPosTemp = 0.5f * (-g) * math.pow(parabolTime * 1, 2) + parabolVy * (parabolTime * 1) + parabolP1.y;

                parabolVx = x / t;
                parabolVy = (h + (0.5f * g * Mathf.Pow(t, 2)) / t);

                while (PointsColliders.ElementAt(1).transform.position != new Vector3(xPosTemp, yPosTemp, 0))
                {
                    yield return null;
                }
                canStart = false;

                int noCollidesTemp = 0;
                MaxShownLinePoint = -1;

                //collisions till the objective
                foreach (GameObject item in PointsColliders)
                {
                    if (!item.GetComponent<LineManager>().IsTriggered())
                    {
                        noCollidesTemp++;
                    }
                    else if (MaxShownLinePoint == -1)
                        MaxShownLinePoint = Int32.Parse(PointsColliders.Find(item).Value.name) - 1;
                }
                if (noCollidesTemp > noCollides)
                {
                    noCollides = noCollidesTemp;
                    timeLessColides = t;
                    vXLessColides = V0x;
                    vYLessColides = V0y;
                    xStepLessColides = XStep;
                    xLessColides = x;
                    hLessColides = h;
                }
                CanErase = true;
                yield return new WaitForSeconds(0.01f);
                //show tray
                if (noCollidesTemp == PointsColliders.Count)
                {
                    cpu.GetComponent<Player>().getLineRenderer().positionCount = PathPoints.Count;
                    Vector3[] positions = PathPoints.GetRange(0, PathPoints.Count).ConvertTo<Vector3[]>();
                    cpu.GetComponent<Player>().getLineRenderer().SetPositions(positions);
                    cpu.GetComponent<Player>().getLineRenderer().enabled = true;
                    yield return new WaitForSeconds(2);
                    break;
                }
                else
                {
                    try
                    {
                        cpu.GetComponent<Player>().getLineRenderer().positionCount = MaxShownLinePoint;
                        Vector3[] positions = PathPoints.GetRange(0, MaxShownLinePoint).ConvertTo<Vector3[]>();
                        cpu.GetComponent<Player>().getLineRenderer().SetPositions(positions);
                        cpu.GetComponent<Player>().getLineRenderer().enabled = true;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        t -= 0.01f;
                        x = p2.x - p1.x;
                        h = p2.y - p1.y;
                        h = h / t;

                        V0x = x / t;
                        V0y = (h + (0.5f * g * Mathf.Pow(t, 2)) / t);

                        parabolVx = x / t;
                        parabolVy = (h + (0.5f * g * Mathf.Pow(t, 2)) / t);

                        PathPoints.Clear();
                        time = (XStep) / V0x;

                        parabolTime = time;
                        canStart = true;
                        //1 is the time in parabol eq
                        xPosTemp = parabolXStep * 1 + parabolP1.x;
                        yPosTemp = 0.5f * (-g) * math.pow(parabolTime * 1, 2) + parabolVy * (parabolTime * 1) + parabolP1.y;

                        parabolVx = x / t;
                        parabolVy = (h + (0.5f * g * Mathf.Pow(t, 2)) / t);
                        canStart = false;
                        break;
                    }
                }
            }
            //check if can shoot
            if (noCollides == PointsColliders.Count)
            {
                cpu.transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>().Play();
                GameObject go = Instantiate(BulletTypes[actualWeaponCpu + 1]);
                go.transform.position = cpu.transform.position;
                go.GetComponent<Rigidbody>().velocity = new Vector3(V0x, V0y + 0.1f, 0f);
                go.GetComponent<Projectile>().ShooterPosition = cpu.transform.position;
                go.GetComponent<Projectile>().creatorTag = cpu.gameObject.tag;
                DontGoThroughThings dGo = go.AddComponent<DontGoThroughThings>();
                dGo.layerMask = ground;
                mainCamera.GetComponent<CameraManager>().ChangeFollowObject(go, player);
            }
            else
            {
                //if cant shoot, go to the better tray (inverse)
                for (float t = ((((Mathf.Sqrt(Mathf.Pow(h, 2) + Mathf.Pow(x, 2)))) / 4) + Math.Abs(h / 50)); t > timeLessColides; t -= 0.05f)
                {
                    if (t < timeLessColides + 0.049f)
                    {
                        t = timeLessColides;
                    }

                    x = p2.x - p1.x;
                    h = p2.y - p1.y;

                    h = h / t;

                    parabolX = x;
                    parabolH = h;

                    V0x = x / t;
                    V0y = (h + (0.5f * 9.8f * Mathf.Pow(t, 2)) / t);

                    parabolVx = x / t;
                    parabolVy = (h + (0.5f * 9.8f * Mathf.Pow(t, 2)) / t);

                    PathPoints.Clear();

                    float time = (XStep) / V0x;

                    parabolTime = time;
                    canStart = true;
                    //1 is the time in parabol eq
                    float xPosTemp = parabolXStep * 1 + parabolP1.x;
                    float yPosTemp = 0.5f * (-9.8f) * math.pow(parabolTime * 1, 2) + parabolVy * (parabolTime * 1) + parabolP1.y;
                    while (PointsColliders.ElementAt(1).transform.position != new Vector3(xPosTemp, yPosTemp, 0))
                    {
                        yield return null;
                    }

                    canStart = false;

                    int noCollidesTemp = 0;

                    foreach (GameObject item in PointsColliders)
                    {
                        if (!item.GetComponent<LineManager>().IsTriggered())
                        {
                            noCollidesTemp++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    cpu.GetComponent<Player>().getLineRenderer().positionCount = noCollidesTemp;
                    Vector3[] positions = PathPoints.GetRange(0, noCollidesTemp).ConvertTo<Vector3[]>();
                    cpu.GetComponent<Player>().getLineRenderer().SetPositions(positions);
                    cpu.GetComponent<Player>().getLineRenderer().enabled = true;
                }
                cpu.GetComponentInChildren<ParticleSystem>().Play();

                GameObject go = Instantiate(BulletTypes[actualWeaponCpu + 1]);
                go.transform.position = cpu.transform.position;
                go.GetComponent<Rigidbody>().velocity = new Vector3(vXLessColides, vYLessColides + 0.1f, 0f);
                go.GetComponent<Projectile>().ShooterPosition = cpu.transform.position;
                go.GetComponent<Projectile>().creatorTag = cpu.gameObject.tag;
                DontGoThroughThings dGo = go.AddComponent<DontGoThroughThings>();
                dGo.layerMask = ground;
                mainCamera.GetComponent<CameraManager>().ChangeFollowObject(go, player);
            }
            foreach (GameObject item in PointsColliders)
            {
                item.GetComponent<LineManager>().ForceCollision = true;
            }
            CPUshot = false;
            cpu.GetComponent<Player>().getLineRenderer().enabled = false;
        }
        yield break;
    }

    private bool PlaceFree(int ColliderID)
    {
        return !PointsColliders.ElementAt(ColliderID).GetComponent<LineManager>().triggered;
    }


    private void zoom(float increment)
    {
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }

    private void moveSprites(int i)
    {
        //Para saber si mira a izquierda o derecha
        Vector3 point1 = player.transform.GetChild(3).position;
        Vector3 point2 = player.transform.GetChild(4).position;

        //Para saber si mira arriba o abajo
        Vector3 point3 = player.transform.GetChild(5).position;
        Vector3 point4 = player.transform.GetChild(6).position;

        //Para darle la vuelta al sprite
        Vector2 V1 = new Vector2(point2.x - point1.x, point2.y - point1.y);
        Vector2 Point1 = point2;
        float m1 = V1.y / V1.x;
        float b1 = Point1.y - m1 * Point1.x;

        Vector2 VPerpendicular1 = new Vector2(-1 * V1.y, V1.x);
        Vector2 Point2 = new Vector2(PointsColliders.ElementAt(i).transform.position.x, PointsColliders.ElementAt(i).transform.position.y);
        float mPerpendicular1 = VPerpendicular1.y / VPerpendicular1.x;
        float bPerpendicular1 = Point2.y - mPerpendicular1 * Point2.x;

        float x1 = (b1 - bPerpendicular1) / (mPerpendicular1 - m1);

        float y1 = m1 * x1 + b1;

        Vector3 CatetoContiguo1 = PointsColliders.ElementAt(i).transform.position - new Vector3(x1, y1, PointsColliders.ElementAt(i).transform.position.z);

        Vector2 V2 = new Vector2(point4.x - point3.x, point4.y - point3.y);
        Vector2 Point3 = point4;
        float m2 = V2.y / V2.x;
        float b2 = Point3.y - m2 * Point3.x;

        Vector2 VPerpendicular2 = new Vector2(-1 * V2.y, V2.x);
        float mPerpendicular2 = VPerpendicular2.y / VPerpendicular2.x;
        float bPerpendicular2 = Point2.y - mPerpendicular2 * Point2.x;

        float x2 = (b2 - bPerpendicular2) / (mPerpendicular2 - m2);

        float y2 = m2 * x2 + b2;


        Vector3 CatetoContiguo2 = PointsColliders.ElementAt(i).transform.position - new Vector3(x2, y2, PointsColliders.ElementAt(i).transform.position.z);

        Vector3 CentralPoint = new Vector3(PointsColliders.ElementAt(0).transform.position.x, PointsColliders.ElementAt(0).transform.position.y, PointsColliders.ElementAt(0).transform.position.z);


        Vector3 CatetoOpuesto = CentralPoint - new Vector3(x1, y1, CentralPoint.z);

        float num1 = Mathf.Sqrt(Mathf.Pow(CatetoOpuesto.x, 2) + Mathf.Pow(CatetoOpuesto.y, 2));
        float num2 = Mathf.Sqrt(Mathf.Pow(CatetoContiguo1.x, 2) + Mathf.Pow(CatetoContiguo1.y, 2));

        float InclinationAngle = Mathf.Atan(num1 / num2);

        if (Input.GetKeyDown("k"))
        {
            print("a");
        }

        float VaritationAngle = player.transform.rotation.eulerAngles.z * Mathf.Deg2Rad < 0.001f ? 0f : player.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        switch ((CatetoContiguo2.y, CatetoContiguo1.x))
        {
            case ( < 0f, < 0f):
                InclinationAngle += VaritationAngle;
                player.transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
                player.transform.GetChild(1).transform.eulerAngles = new Vector3(0, 0, InclinationAngle * Mathf.Rad2Deg);
                break;
            case ( < 0f, >= 0f):
                InclinationAngle -= VaritationAngle;
                player.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                player.transform.GetChild(1).transform.eulerAngles = new Vector3(0, 0, 90 - ((InclinationAngle * Mathf.Rad2Deg) - 270));
                break;
            case ( >= 0f, < 0f):
                InclinationAngle -= VaritationAngle;
                player.transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
                player.transform.GetChild(1).transform.eulerAngles = new Vector3(0, 0, 90 - ((InclinationAngle * Mathf.Rad2Deg) - 270));
                break;
            case ( >= 0f, >= 0f):
                InclinationAngle += VaritationAngle;
                player.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                player.transform.GetChild(1).transform.eulerAngles = new Vector3(0, 0, InclinationAngle * Mathf.Rad2Deg);
                break;
        }

    }

    //mini explosion, less than 10

    public IEnumerator explosionCalculator(int pow, Vector3 position)
    {
        //Audio and animation particle for explosion
        AudioManager.instance.playSound("explode");
        ExplosionParticle.transform.position = new Vector3(position.x, position.y, 0f);
        var particleSize = ExplosionParticle.GetComponent<ParticleSystem>().main;
        particleSize.startSize = pow / 4;
        if(pow <= 3)
        {
            particleSize.startSize = 1;
        }
        ExplosionParticle.GetComponent<ParticleSystem>().Play();

        RaycastHit rh;
        Physics.Raycast(transform.position, Vector3.zero, out rh, 0.085f, ly);
        hitPoint.transform.position = position;
        hitPoint.Emit(1);

        if (pow < 10)
        {
            //Delete central pixel
            Data.ter.Dig(position, 1, false);
            for (int i = 1; i <= pow; i++)
            {
                for(int j = 1; j < 91; j++)
                {
                    for(float k = 0; k < 8; k += 0.5f)
                    {
                        var x1 = i * 0.085f * math.cos(j + (k * 45) * Mathf.Deg2Rad);
                        var y1 = i * 0.085f * math.sin(j + (k * 45) * Mathf.Deg2Rad);
                        Vector3 direction1 = new Vector3(x1, y1);
                        Vector3 newPosition1 = position;
                        newPosition1.x += x1;
                        newPosition1.y += y1;

                        RaycastHit rh1;
                        Physics.Raycast(position, direction1, out rh1, i, ly);
                        Vector3 pos = rh1.point;
                        pos.x += Random.Range(-2, 3) / 40f;
                        pos.y += Random.Range(-2, 3) / 40f;
                        hitPoint.transform.position = newPosition1;
                        hitPoint.Emit(1);
                        Data.ter.Dig(newPosition1, 1, false);

                        Data.ter.Dig(newPosition1, 1, false);
                        if (pow % 2 == 1)
                        {
                            pow++;
                        }
                        if (i == pow && j >= 90 && k == 3)
                        {
                            Data.ter.Dig(newPosition1, 0, true);
                        }
                        else
                        {
                            Data.ter.Dig(newPosition1, 0, false);
                        }
                    }
                }

                yield return new WaitForSeconds(0.01f);
            }
        }
        else
        {
            StartCoroutine(explosionBigCalculator(pow, position));
        }

        yield break;
    }


    //big explosion >10
    public IEnumerator explosionBigCalculator(int pow, Vector3 position)
    {
        if (pow % 2 == 1)
        {
            pow++;
        }

        int tempPower = (int)(pow * 1.35f);
        if (tempPower % 2 == 1)
        {
            tempPower++;
        }

        Data.ter.Dig(position, tempPower, false);

        int powerDivided = pow;

        switch (powerDivided % 4)
        {
            case 0:
                powerDivided = powerDivided / 2;
                break;
            case 1:
                powerDivided = (powerDivided - 1) / 2;
                break;
            case 2:
                powerDivided = (powerDivided + 2) / 2;
                break;
            case 3:
                powerDivided = (powerDivided + 1) / 2;
                break;
        }

        powerDivided = powerDivided / 2;
        if (powerDivided % 2 == 1)
        {
            powerDivided++;
        }

        Vector3 pos;
        float step = pow / 10;
        for (int i = 10; i > 0; i--)
        {
            for (int j = 0; j < 360; j += 10)
            {

                var x1 = (pow - (step * i)) / 1.25f * 0.085f * math.cos(j * Mathf.Deg2Rad);
                var y1 = (pow - (step * i)) / 1.25f * 0.085f * math.sin(j * Mathf.Deg2Rad);
                Vector3 direction1 = new Vector3(x1, y1);
                Vector3 newPosition1 = position;
                newPosition1.x += x1;
                newPosition1.y += y1;


                RaycastHit rh1;
                Physics.Raycast(position, direction1, out rh1, pow, ly);
                Debug.DrawRay(position, direction1, Color.red, 3f);

                pos = rh1.point;
                pos.x += Random.Range(-2, 3) / 40f;
                pos.y += Random.Range(-2, 3) / 40f;

                hitPoint.transform.position = newPosition1;
                hitPoint.Emit(1);
                Data.ter.Dig(newPosition1, powerDivided, true);

            }
            yield return new WaitForSeconds(0.01f);
        }

        for (float k = 0; k < 360; k += 10)
        {
            var x1 = pow / 1.25f * 0.085f * math.cos(k * Mathf.Deg2Rad);
            var y1 = pow / 1.25f * 0.085f * math.sin(k * Mathf.Deg2Rad);
            Vector3 direction1 = new Vector3(x1, y1);
            Vector3 newPosition1 = position;
            newPosition1.x += x1;
            newPosition1.y += y1;


            RaycastHit rh1;
            Physics.Raycast(position, direction1, out rh1, pow, ly);
            Debug.DrawRay(position, direction1, Color.red, 5f);

            pos = rh1.point;

            pos.x += Random.Range(-2, 3) / 40f;
            pos.y += Random.Range(-2, 3) / 40f;

            hitPoint.transform.position = newPosition1;
            hitPoint.Emit(1);
            Data.ter.Dig(newPosition1, powerDivided, true);
        }

        yield return new WaitForSeconds(0.1f);
        for (float k = 0; k < 360; k += 10)
        {
            var x1 = (pow + 2.5f) / 1.25f * 0.085f * math.cos(k * Mathf.Deg2Rad);
            var y1 = (pow + 2.5f) / 1.25f * 0.085f * math.sin(k * Mathf.Deg2Rad);
            Vector3 direction1 = new Vector3(x1, y1);
            Vector3 newPosition1 = position;
            newPosition1.x += x1;
            newPosition1.y += y1;


            RaycastHit rh1;
            Physics.Raycast(position, direction1, out rh1, pow, ly);
            Debug.DrawRay(position, direction1, Color.red, 5f);

            pos = rh1.point;

            pos.x += Random.Range(-2, 3) / 40f;
            pos.y += Random.Range(-2, 3) / 40f;

            hitPoint.transform.position = newPosition1;
            hitPoint.Emit(1);
            Data.ter.Dig(newPosition1, 4, true);
        }
        //more precision
        for (float k = 0; k < 360; k += 1.5f)
        {
            var x1 = (pow + 5) / 1.25f * 0.085f * math.cos(k * Mathf.Deg2Rad);
            var y1 = (pow + 5) / 1.25f * 0.085f * math.sin(k * Mathf.Deg2Rad);
            Vector3 direction1 = new Vector3(x1, y1);
            Vector3 newPosition1 = position;
            newPosition1.x += x1;
            newPosition1.y += y1;


            RaycastHit rh1;
            Physics.Raycast(position, direction1, out rh1, pow, ly);
            Debug.DrawRay(position, direction1, Color.red, 5f);

            pos = rh1.point;

            pos.x += Random.Range(-2, 3) / 40f;
            pos.y += Random.Range(-2, 3) / 40f;

            hitPoint.transform.position = newPosition1;
            hitPoint.Emit(1);
            Data.ter.Dig(newPosition1, 4, true);
        }

        //CANMOVE
        yield break;
    }

    public void changeMode()
    {
        //Text t = new Text("sd");
        //print(modeButton.transform.GetChild(0).gameObject);
        switch (mode)
        {
            case 0:
                //Camera to movement (0-1)
                player.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = playerSprites[0];
                player.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                V0x = 0f;
                V0y = 0f;
                mode++;  
                if (moved)
                {
                    changeMode();
                }
                else
                {
                    modeButton.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = imageModes[2];
                    mainCamera.GetComponent<CameraManager>().Center();
                    camButton.SetActive(false);
                }
                break;

            case 1:
                //Movement to Shoot (1-2)
                player.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = playerSprites[1];
                player.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                player.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = playerSprites[actualWeaponPlayer + 3];
                V0x = 0f;
                V0y = 0f;
                mode++;
                modeButton.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = imageModes[0];
                break;

            case 2:
                //Shoot to Camera (2-0)
                player.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = playerSprites[0];
                player.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                mode = 0;
                if (moved)
                {
                    modeButton.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = imageModes[2];
                }
                else
                {
                    modeButton.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = imageModes[1];
                }
                camButton.SetActive(true);
                break;

        }
    }

    public IEnumerator manageEnd()
    {
        yield return new WaitForSeconds(2.0f);
        if (player.GetComponent<Player>().dead)
        {
            StartCoroutine(AudioManager.instance.pauseMusic("gameOverTheme", 0.5f));
            TutorialCanvas.transform.GetChild(1).gameObject.SetActive(false);
            TutorialCanvas.transform.GetChild(2).gameObject.SetActive(false);
            TutorialCanvas.transform.GetChild(3).gameObject.SetActive(false);
            EndgameCanvas.transform.GetChild(0).gameObject.SetActive(true);

        }
        else
        {
            StartCoroutine(AudioManager.instance.pauseMusic("VictoryFF", 0.5f));
            TutorialCanvas.transform.GetChild(1).gameObject.SetActive(false);
            TutorialCanvas.transform.GetChild(2).gameObject.SetActive(false);
            TutorialCanvas.transform.GetChild(3).gameObject.SetActive(false); 
            EndgameCanvas.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void playAgain()
    {
        //Recargar escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void goToMainManu()
    {
        SceneManager.LoadScene("MainTitle");
    }

    public void updatePlayerArm()
    {
        player.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = playerSprites[actualWeaponPlayer + 3];
    }

    public void updateCpuArm()
    {
        cpu.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = cpuSprites[actualWeaponCpu + 3];
    }

}

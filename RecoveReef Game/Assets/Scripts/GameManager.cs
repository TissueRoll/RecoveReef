using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    #pragma warning disable 0649
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Camera nurseryCamera;
    [SerializeField] private Tilemap coralTileMap;
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap substrataTileMap;
    [SerializeField] private Tilemap substrataOverlayTileMap;
    [SerializeField] private Tilemap algaeTileMap;
    [SerializeField] private TileBase[] coralTileBases;
    [SerializeField] private TileBase[] groundTileBases;
    [SerializeField] private TileBase[] algaeTileBases;
    [SerializeField] private TileBase[] substrataTileBases;
    [SerializeField] private Text fishDisplay;
    [SerializeField] private Text testTimerText;
    #pragma warning restore 0649
    private float zoom = 20f;
    private float updateDelay = 0.5f;
    private int survivabilityFrameCounter = 0;
    private Vector3 cameraFollowPosition;
    private Vector3 savedCameraPosition;
    private bool edgeScrollingEnabled = false;
    private bool showNursery = false;
    private Dictionary<Vector3Int,CoralCellData> coralCells;
    private Dictionary<Vector3Int,float> substrataCells;
    private Dictionary<Vector3Int,AlgaeCellData> algaeCells; 
    private Dictionary<TileBase, float> probCoralSurvivabilityMax;
    private Dictionary<TileBase, float> probAlgaeSurvivabilityMax;
    private int testnum = 0;
    private int fishOutput = 0;
    private int fishIncome = 0;
    private float carnivorousFishTotalInterest = 0;
    private int carnivorousFishTotal = 0;
    private float herbivorousFishTotalInterest = 0;
    private int herbivorousFishTotal = 0;
    private Vector3Int[,] hexNeighbors = new Vector3Int[,] {
        {new Vector3Int(1,0,0), new Vector3Int(0,-1,0), new Vector3Int(-1,-1,0), new Vector3Int(-1,0,0), new Vector3Int(-1,1,0), new Vector3Int(0,1,0)}, 
        {new Vector3Int(1,0,0), new Vector3Int(1,-1,0), new Vector3Int(0,-1,0), new Vector3Int(-1,0,0), new Vector3Int(0,1,0), new Vector3Int(1,1,0)} 
    };
    private string[] coralNames = new string[6] {"columnar", "branching", "encrusting", "foliaceous", "laminar", "massive"};
    private int[] coralGrowTimes = new int[6] {45, 30, 95, 60, 120, 135};
    private CountdownTimer tempTimer;
    private int maxSpaceInNursery = 10;
    private List<NursingCoral>[] growingCorals;
    private Queue<string>[] queuedCorals;

    void Awake()
    {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        print("initializing tiles...");
        initializeTiles();
        print("initialization done");
        tempTimer = new CountdownTimer(1800f);
        initializeGame();
    }

    private TileBase findInTileBaseArray (string code, string type) {
        TileBase tileBase = null;
        if (type == "coral") {
            for (int i = 0; i < coralTileBases.Length; i++) {
                if (Regex.IsMatch(coralTileBases[i].name, ".*"+type+"_"+code+".*")) {
                    tileBase = coralTileBases[i];
                    break;
                }
            }
        } else if (type == "algae") {
            for (int i = 0; i < algaeTileBases.Length; i++) {
                if (Regex.IsMatch(algaeTileBases[i].name, ".*"+type+"_"+code+".*")) {
                    tileBase = algaeTileBases[i];
                    break;
                }
            }
        } else if (type == "substrata") {
            for (int i = 0; i < substrataTileBases.Length; i++) {
                if (Regex.IsMatch(substrataTileBases[i].name, ".*"+type+"_"+code+".*")) {
                    tileBase = substrataTileBases[i];
                    break;
                }
            }
        }
        if (tileBase == null)
            print("ERROR: tileBase not found");
        return tileBase;
    }

    private void initializeTiles() {
        // instantiation
        coralCells = new Dictionary<Vector3Int,CoralCellData>();
        substrataCells = new Dictionary<Vector3Int, float>();
        algaeCells = new Dictionary<Vector3Int, AlgaeCellData>();
        growingCorals = new List<NursingCoral>[6];
        queuedCorals = new Queue<string>[6];

        // setting values
        probCoralSurvivabilityMax = new Dictionary<TileBase, float>();
        probCoralSurvivabilityMax.Add(findInTileBaseArray("big", "substrata"), 100);
        probCoralSurvivabilityMax.Add(findInTileBaseArray("med", "substrata"), 97);
        probCoralSurvivabilityMax.Add(findInTileBaseArray("small", "substrata"), 90);

        probAlgaeSurvivabilityMax = new Dictionary<TileBase, float>();
        probAlgaeSurvivabilityMax.Add(findInTileBaseArray("brown", "algae"), 95);
        probAlgaeSurvivabilityMax.Add(findInTileBaseArray("green", "algae"), 85);

        for (int i = 0; i < 6; i++) {
            growingCorals[i] = new List<NursingCoral>();
            queuedCorals[i] = new Queue<string>();
        }

        // initialization

        // Setting the tiles in the tilemap to the coralCells dictionary
        foreach(Vector3Int pos in coralTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!coralTileMap.HasTile(localPlace)) continue;
            CoralCellData cell = new CoralCellData(localPlace, coralTileMap, coralTileMap.GetTile(localPlace), 101.0f, UnityEngine.Random.Range(0.0007f,0.0035f), UnityEngine.Random.Range(0.001f,0.005f));
            carnivorousFishTotalInterest += cell.carnivorousFishInterest;
            herbivorousFishTotalInterest += cell.herbivorousFishInterest;
            coralCells.Add(cell.LocalPlace, cell);
            substrataOverlayTileMap.SetTile(localPlace, groundTileMap.GetTile(localPlace));
        }

        
        foreach (Vector3Int pos in substrataTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!substrataTileMap.HasTile(localPlace)) continue;
            substrataCells.Add(localPlace, probCoralSurvivabilityMax[substrataTileMap.GetTile(localPlace)]);
        }

        
        foreach (Vector3Int pos in algaeTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!algaeTileMap.HasTile(localPlace)) continue;
            AlgaeCellData cell = new AlgaeCellData(localPlace, algaeTileMap, algaeTileMap.GetTile(localPlace), 101.0f, UnityEngine.Random.Range(10,20));
            herbivorousFishTotal += cell.herbivorousFishProduction;
            algaeCells.Add(cell.LocalPlace,cell);
            substrataOverlayTileMap.SetTile(localPlace, groundTileMap.GetTile(localPlace));
        }
    }

    private void initializeGame() {
        fishDisplay = GameObject.Find("FishDisplay").GetComponent<Text>();
        fishDisplay.text = "Fish Output: 0\nCarnivorous Fish: 0\nHerbivorous Fish: 0\nFish Income: 0";
        testTimerText.text = convertTimetoMS(tempTimer.currentTime);
    }

    private void Start() {
        // sets the cameraFollowPosition to the default 
        cameraFollowPosition = cameraFollow.transform.position;
        cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
        cameraFollow.enabled = true;
        // nurseryCamera.Setup(() => new Vector3(500,500,-10), () => zoom);
        nurseryCamera.enabled = false;
        InvokeRepeating("doStuff", 1.0f, updateDelay);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            if (!growCoral(0))
                print("Cannot grow coral");
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            if (!growCoral(1))
                print("Cannot grow coral");
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            if (!growCoral(2))
                print("Cannot grow coral");
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            if (!growCoral(3))
                print("Cannot grow coral");
        } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            if (!growCoral(4))
                print("Cannot grow coral");
        } else if (Input.GetKeyDown(KeyCode.Alpha6)) {
            if (!growCoral(5))
                print("Cannot grow coral");
        } 

        if (Input.GetKeyDown(KeyCode.Z)) {
            testnum = 0;
        } else if (Input.GetKeyDown(KeyCode.X)) {
            testnum = 1;
        } else if (Input.GetKeyDown(KeyCode.C)) {
            testnum = 2;
        } else if (Input.GetKeyDown(KeyCode.V)) {
            testnum = 3;
        } else if (Input.GetKeyDown(KeyCode.B)) {
            testnum = 4;
        } else if (Input.GetKeyDown(KeyCode.N)) {
            testnum = 5;
        }

        // testing for hex tile coords
        bool lb = Input.GetMouseButtonDown(0);
        if (lb) {
            print("left mouse button has been pressed");
            Vector3Int position = getMouseGridPosition();
            print(":: " + position);
            print(":: " + substrataOverlayTileMap.GetTile(position).name); 
        }

        bool rb = Input.GetMouseButtonDown(1);
        if (rb) {
            if(!plantCoral(testnum)) {
                print("Cannot plant coral onto the reef.");
            }
        }

        // movement of screen
        if (Input.GetKeyDown(KeyCode.Space)) {
            edgeScrollingEnabled = !edgeScrollingEnabled;
            print("edgeScrolling = " + edgeScrollingEnabled);
        }
        
        // transporting to some far off place for the nursery
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            showNursery = !showNursery;
            if (showNursery) {
                savedCameraPosition = cameraFollow.transform.position;
                cameraFollow.enabled = false;
                nurseryCamera.enabled = true;
            } else {
                cameraFollow.enabled = true;
                nurseryCamera.enabled = false;
                cameraFollowPosition = savedCameraPosition;
                cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
            }
            
        }

        if (!showNursery) {
            moveCameraWASD(20f);
            if (edgeScrollingEnabled) moveCameraMouseEdge(20f,10f);
            zoomKeys(1f);
        }
        
        for (int i = 0; i < 6; i++) {
            foreach (NursingCoral x in growingCorals[i]) {
                x.timer.updateTime();
                if (x.timer.isDone())
                    queuedCorals[i].Enqueue(x.coral);
            }
            growingCorals[i].RemoveAll(coral => coral.timer.isDone() == true); // yay for internship
        }

        tempTimer.updateTime();
        testTimerText.text = convertTimetoMS(tempTimer.currentTime);
    }

    private int getCoralsInNursery() {
        int coralsInNursery = 0;
        for (int i = 0; i < 6; i++) {
            coralsInNursery += growingCorals[i].Count;
            coralsInNursery += queuedCorals[i].Count;
        }
        return coralsInNursery;
    }

    private bool growCoral(int type) {
        bool successful = false;
        if (getCoralsInNursery() <= maxSpaceInNursery) {
            successful = true;
            growingCorals[type].Add(new NursingCoral(coralNames[type], new CountdownTimer(coralGrowTimes[type])));
        }
        return successful;
    }

    private bool plantCoral(int type) {
        bool successful = false;
        // should be unable to replace a tile
        // need to add more useful stuff like notifying why you can place a tile here
        print("right mouse button has been pressed");
        Vector3Int position = getMouseGridPosition();
        print("position: " + position);
        if (coralTileMap.HasTile(position)) {
            print("coral already existing; cannot place tile");
            print(coralCells[position].printData());
        } else if (algaeTileMap.HasTile(position)) {
            print("algae already existing; cannot place tile");
            print(algaeCells[position].printData());
        } else if ((substrataTileMap.HasTile(position) || substrataCells.ContainsKey(position)) && queuedCorals[type].Count > 0) { 
            successful = true;
            queuedCorals[type].Dequeue();
            CoralCellData cell = new CoralCellData(position, coralTileMap, findInTileBaseArray(coralNames[type], "coral"), 0.0f, UnityEngine.Random.Range(0.0007f,0.0035f), UnityEngine.Random.Range(0.001f,0.005f));
            coralCells.Add(position, cell);
            carnivorousFishTotalInterest += coralCells[position].carnivorousFishInterest;
            herbivorousFishTotalInterest += coralCells[position].herbivorousFishInterest;
            coralTileMap.SetTile(position, findInTileBaseArray(coralNames[type], "coral"));
            substrataOverlayTileMap.SetTile(position, groundTileMap.GetTile(position));
        } else if (queuedCorals[type].Count == 0 && growingCorals[type].Count > 0) {
            print("soonest to mature has " + convertTimetoMS(growingCorals[type][0].timer.currentTime) + " time left");
        }
        
        if (coralTileMap.HasTile(position))
            print(coralCells[position].printData());

        return successful;
    }

    private string convertTimetoMS(float rawTime) {
        string minutes = Mathf.Floor(rawTime/60).ToString("00");
        string seconds = Mathf.RoundToInt(rawTime%60).ToString("00");
        return string.Format("{0}:{1}", minutes, seconds);
    }

    private void doStuff() {
        survivabilityFrameCounter = (++survivabilityFrameCounter % 7 == 0 ? 0 : survivabilityFrameCounter);
        if (survivabilityFrameCounter == 0) 
            updateAllCoral();
        else if (survivabilityFrameCounter == 3 || survivabilityFrameCounter == 6)
            updateAllAlgae();
        
        updateFishOutput();
        fishDisplay.text = "Fish Output: " + fishOutput
                        + "\nCarnivorous Fish: " + carnivorousFishTotal
                        + "\nHerbivorous Fish: " + herbivorousFishTotal
                        + "\nFish Income: " + fishIncome;
    }

    private void updateAllAlgae() {
        updateAlgaeSurvivability();
        updateAlgaePropagation();
    }

    private void updateAlgaeSurvivability() {
        List<Vector3Int> keys = new List<Vector3Int>(algaeCells.Keys);
        foreach (Vector3Int key in keys) {
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            if (algaeCells[key].maturity <= 100.0f) {
                algaeCells[key].addMaturity(1);
                if (!algaeCells[key].willSurvive(randNum, substrataCells[key])) { // BUGGED
                    algaeTileMap.SetTile(key, null);
                    substrataOverlayTileMap.SetTile(key, null);
                    algaeCells.Remove(key);
                }
            }
        }
    }

    private void updateAlgaePropagation() {
        // handles propagation
        // basically get list of keys, then propagate
        // https://www.redblobgames.com/grids/hexagons/
        // note: unity is using inverted odd-q; switch x and y then baliktad
        // each algae has a random propagation chance; generate a random num to roll chance

        List<Vector3Int> keys = new List<Vector3Int>(algaeCells.Keys);
        foreach (Vector3Int key in keys) {
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            float basePropagationChance = UnityEngine.Random.Range(50.0f, 60.0f);
            if (algaeCells[key].maturity > 100.0f) { // propagate only if "mature"
                for (int i = 0; i < 6; i++) {
                    if (UnityEngine.Random.Range(0.0f,100.0f) <= basePropagationChance + UnityEngine.Random.Range(0.0f, 5.0f)) {
                        Vector3Int localPlace = key+hexNeighbors[key.y&1,i];
                        if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace)) continue;
                        if (algaeTileMap.HasTile(localPlace) || algaeCells.ContainsKey(localPlace)) continue;
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace)) {
                            randNum -= coralCells[localPlace].maturity*0.15f;
                            CoralCellData temp;
                            float avgMaturity = 0;
                            float numCorals = 1;
                            for (int j = 0; j < 6; j++)
                                if (coralCells.TryGetValue(localPlace+hexNeighbors[localPlace.y&1, j], out temp)) {
                                    randNum -= coralCells[localPlace+hexNeighbors[localPlace.y&1, j]].maturity*0.05f;
                                    avgMaturity += coralCells[localPlace+hexNeighbors[localPlace.y&1, j]].maturity;
                                    numCorals++;
                                }
                            if (randNum < avgMaturity/numCorals) continue;
                        }
                        AlgaeCellData cell = new AlgaeCellData(localPlace, algaeTileMap, algaeCells[key].TileBase, 0.0f, UnityEngine.Random.Range(10,20));
                        herbivorousFishTotal += cell.herbivorousFishProduction;
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace)) {
                            coralTileMap.SetTile(localPlace, null);
                            substrataOverlayTileMap.SetTile(localPlace, null);
                            herbivorousFishTotalInterest -= coralCells[localPlace].herbivorousFishInterest;
                            carnivorousFishTotalInterest -= coralCells[localPlace].carnivorousFishInterest;
                            coralCells.Remove(localPlace);
                        }
                        algaeCells.Add(cell.LocalPlace,cell);
                        algaeTileMap.SetTile(cell.LocalPlace, cell.TileBase);
                        substrataOverlayTileMap.SetTile(cell.LocalPlace, groundTileMap.GetTile(cell.LocalPlace));
                    }
                }
            }
        }

    }

    private void updateFishOutput() {
        int tempHFT = herbivorousFishTotal;
        int tempCFT = carnivorousFishTotal;
        herbivorousFishTotal += (int)Math.Round(tempHFT*herbivorousFishTotalInterest - tempCFT);
        carnivorousFishTotal += (int)Math.Round((tempHFT-tempCFT)*carnivorousFishTotalInterest + (tempHFT-tempCFT)*0.03f);

        // anti civ gandhi
        herbivorousFishTotal = Math.Max(herbivorousFishTotal, 0);
        carnivorousFishTotal = Math.Max(carnivorousFishTotal, 0);

        herbivorousFishTotal = Math.Min(herbivorousFishTotal, 10000);
        carnivorousFishTotal = Math.Min(carnivorousFishTotal, 10000);

        fishIncome = (int)Math.Round(carnivorousFishTotal*0.3f + herbivorousFishTotal*0.2f);
        fishOutput += fishIncome;
        fishOutput = Math.Min(fishOutput, 50000);
    }

    private void updateAllCoral() {
        updateCoralSurvivability();
        updateCoralPropagation();
    }

    private void updateCoralSurvivability() {
        List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
        foreach (Vector3Int key in keys) {
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            if (coralCells[key].maturity <= 100) {
                // check adj corals
                float miscFactors = 0.0f;
                for (int i = 0; i < 6; i++)
                    if (coralCells.ContainsKey(key+hexNeighbors[key.y&1, i]))
                        miscFactors += 0.01f*coralCells[key+hexNeighbors[key.y&1, i]].maturity;
                coralCells[key].addMaturity(1.0f);
                if (!coralCells[key].willSurvive(randNum, substrataCells[key], miscFactors)) {
                    coralTileMap.SetTile(key, null);
                    substrataOverlayTileMap.SetTile(key, null);
                    herbivorousFishTotalInterest -= coralCells[key].herbivorousFishInterest;
                    carnivorousFishTotalInterest -= coralCells[key].carnivorousFishInterest;
                    coralCells.Remove(key);
                    substrataOverlayTileMap.SetTile(key, null);
                }
            }
        }
    }

    private void updateCoralPropagation() {
        List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
        foreach (Vector3Int key in keys) {
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            float basePropagationChance = UnityEngine.Random.Range(50.0f, 60.0f);
            if (coralCells[key].maturity > 100.0f) { // propagate only if "mature"
                for (int i = 0; i < 6; i++) {
                    if (UnityEngine.Random.Range(0.0f,100.0f) <= basePropagationChance + UnityEngine.Random.Range(0.0f, 5.0f)) {
                        Vector3Int localPlace = key+hexNeighbors[key.y&1,i];
                        if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace)) continue;
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || algaeTileMap.HasTile(localPlace)) continue;
                        CoralCellData cell = new CoralCellData(localPlace, coralTileMap, coralCells[key].TileBase, 0.0f, UnityEngine.Random.Range(0.0007f,0.0035f), UnityEngine.Random.Range(0.001f,0.005f));
                        carnivorousFishTotalInterest += cell.carnivorousFishInterest;
                        herbivorousFishTotalInterest += cell.herbivorousFishInterest;
                        coralCells.Add(cell.LocalPlace,cell);
                        coralTileMap.SetTile(cell.LocalPlace, cell.TileBase);
                        substrataOverlayTileMap.SetTile(cell.LocalPlace, groundTileMap.GetTile(cell.LocalPlace));
                    }
                }
            }
        }
    }

    private Vector3Int getMouseGridPosition() {
        Grid grid = GameObject.Find("Grid").GetComponent<Grid>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPoint = ray.GetPoint(-ray.origin.z/ray.direction.z);
        Vector3Int position = grid.WorldToCell(worldPoint);
        return position;
    }

    private void moveCameraWASD(float moveAmount) {
        if (Input.GetKey(KeyCode.W)) {
            cameraFollowPosition.y += moveAmount * Time.deltaTime;
        } 
        if (Input.GetKey(KeyCode.A)) {
            cameraFollowPosition.x -= moveAmount * Time.deltaTime;
        } 
        if (Input.GetKey(KeyCode.S)) {
            cameraFollowPosition.y -= moveAmount * Time.deltaTime;
        } 
        if (Input.GetKey(KeyCode.D)) {
            cameraFollowPosition.x += moveAmount * Time.deltaTime;
        }
    }

    private void moveCameraMouseEdge(float moveAmount, float edgeSize) {
        if (Input.mousePosition.x > Screen.width - edgeSize) {
            cameraFollowPosition.x += moveAmount * Time.deltaTime;
        }
        if (Input.mousePosition.x < edgeSize) {
            cameraFollowPosition.x -= moveAmount * Time.deltaTime;
        }
        if (Input.mousePosition.y > Screen.height - edgeSize) {
            cameraFollowPosition.y += moveAmount * Time.deltaTime;
        }
        if (Input.mousePosition.y < edgeSize) {
            cameraFollowPosition.y -= moveAmount * Time.deltaTime;
        }
    }

    private void zoomKeys(float zoomChangeAmount) {
        if (Input.GetKey(KeyCode.KeypadPlus)) {
            zoom -= zoomChangeAmount * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.KeypadMinus)) {
            zoom += zoomChangeAmount * Time.deltaTime;
        }

        if (Input.mouseScrollDelta.y > 0) {
            zoom -= zoomChangeAmount;
        }
        if (Input.mouseScrollDelta.y < 0) {
            zoom += zoomChangeAmount;
        }

        zoom = Mathf.Clamp(zoom, 5f, 30f);
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Tilemap algaeTileMap;
    [SerializeField] private TileBase[] coralTileBases;
    [SerializeField] private TileBase[] groundTileBases;
    [SerializeField] private TileBase[] algaeTileBases;
    [SerializeField] private Text fishDisplay;
    [SerializeField] private Text testTimerText;
    #pragma warning restore 0649
    private float zoom = 10f;
    private float updateDelay = 0.5f;
    private int survivabilityFrameCounter = 0;
    private Vector3 cameraFollowPosition;
    private Vector3 savedCameraPosition;
    private bool edgeScrollingEnabled = false;
    private bool showNursery = false;
    private Dictionary<Vector3Int,CoralCellData> coralCells;
    private Dictionary<Vector3Int,float> groundCells;
    private Dictionary<Vector3Int,AlgaeCellData> algaeCells; // eventually convert to algae cell data or unified structure
    private Dictionary<TileBase, float> probCoralSurvivabilityMax;
    private Dictionary<TileBase, float> probAlgaeSurvivabilityMax;
    private Dictionary<TileBase,int> coralFishValue;
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
    private Dictionary<int,Dictionary<Vector3Int,CoralCellData>> coralGroups;

    private CountdownTimer tempTimer;

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

    private void initializeTiles() {
        // instantiation
        coralFishValue = new Dictionary<TileBase, int>();
        coralGroups = new Dictionary<int, Dictionary<Vector3Int, CoralCellData>>();
        coralCells = new Dictionary<Vector3Int,CoralCellData>();
        groundCells = new Dictionary<Vector3Int, float>();
        algaeCells = new Dictionary<Vector3Int, AlgaeCellData>();

        // setting values
        for (int i = 0; i < coralTileBases.Length; i++) {
            coralFishValue.Add(coralTileBases[i], UnityEngine.Random.Range(5,15));
        }
        probCoralSurvivabilityMax = new Dictionary<TileBase, float>();
        probCoralSurvivabilityMax.Add(groundTileBases[0], 100);
        probCoralSurvivabilityMax.Add(groundTileBases[1], 97);
        probCoralSurvivabilityMax.Add(groundTileBases[2], 90);

        probAlgaeSurvivabilityMax = new Dictionary<TileBase, float>();
        probAlgaeSurvivabilityMax.Add(algaeTileBases[0], 90);
        probAlgaeSurvivabilityMax.Add(algaeTileBases[1], 90);

        // initialization
        coralGroups.Add(0,new Dictionary<Vector3Int, CoralCellData>());

        // Setting the tiles in the tilemap to the coralCells dictionary
        foreach(Vector3Int pos in coralTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!coralTileMap.HasTile(localPlace)) continue;
            CoralCellData cell = new CoralCellData {
                LocalPlace = localPlace,
                WorldLocation = coralTileMap.CellToWorld(localPlace),
                TileBase = coralTileMap.GetTile(localPlace),
                TilemapMember = coralTileMap,
                name = localPlace.x + "," + localPlace.y,
                maturity = 101.0f,
                fishProduction = coralFishValue[coralTileMap.GetTile(localPlace)],
                carnivorousFishInterest = UnityEngine.Random.Range(0.0007f,0.0035f),
                herbivorousFishInterest = UnityEngine.Random.Range(0.001f,0.005f)
            };
            carnivorousFishTotalInterest += cell.carnivorousFishInterest;
            herbivorousFishTotalInterest += cell.herbivorousFishInterest;
            coralCells.Add(cell.LocalPlace, cell);
            coralGroups[0].Add(cell.LocalPlace, cell);
        }

        
        foreach (Vector3Int pos in groundTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!groundTileMap.HasTile(localPlace)) continue;
            groundCells.Add(localPlace, probCoralSurvivabilityMax[groundTileMap.GetTile(localPlace)]);
        }

        
        foreach (Vector3Int pos in algaeTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!algaeTileMap.HasTile(localPlace)) continue;
            AlgaeCellData cell = new AlgaeCellData {
                LocalPlace = localPlace,
                WorldLocation = algaeTileMap.CellToWorld(localPlace),
                TileBase = algaeTileMap.GetTile(localPlace),
                TilemapMember = algaeTileMap,
                name = localPlace.x + "," + localPlace.y,
                maturity = 101.0f,
                herbivorousFishProduction = UnityEngine.Random.Range(10,20)
            };
            herbivorousFishTotal += cell.herbivorousFishProduction;
            algaeCells.Add(cell.LocalPlace,cell);
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
        // testing for hex tile coords
        bool lb = Input.GetMouseButtonDown(0);
        if (lb) {
            print("left mouse button has been pressed");
            Vector3Int position = getMouseGridPosition();
            print(":: " + position);
            // if (coralCells.ContainsKey(position)) {
            //     herbivorousFishTotal -= coralCells[position].herbivorousFishProduction;
            //     coralTileMap.SetTile(position, null);
            //     coralCells.Remove(position);
            // }    
        }

        bool rb = Input.GetMouseButtonDown(1);
        if (rb) {
            // should be unable to replace a tile
            print("right mouse button has been pressed");
            Vector3Int position = getMouseGridPosition();
            print("position: " + position);
            if (coralTileMap.HasTile(position)) {
                print("coral already existing; cannot place tile");
                print(coralCells[position].printData());
            } else if (algaeTileMap.HasTile(position)) {
                print("algae already existing; cannot place tile");
                print(algaeCells[position].printData());
            } else {
                CoralCellData cell = new CoralCellData {
                    LocalPlace = position,
                    WorldLocation = coralTileMap.CellToWorld(position),
                    TileBase = coralTileBases[testnum],
                    TilemapMember = coralTileMap,
                    name = position.x + "," + position.y,
                    maturity = 0,
                    fishProduction = coralFishValue[coralTileBases[testnum]],
                    carnivorousFishInterest = UnityEngine.Random.Range(0.0007f,0.0035f),
                    herbivorousFishInterest = UnityEngine.Random.Range(0.001f,0.005f)
                };
                coralCells.Add(position, cell);
                carnivorousFishTotalInterest += coralCells[position].carnivorousFishInterest;
                herbivorousFishTotalInterest += coralCells[position].herbivorousFishInterest;
                coralTileMap.SetTile(position, coralTileBases[testnum]);
            }
            
            if (coralTileMap.HasTile(position))
                print(coralCells[position].printData());
            testnum = (testnum+1) % coralTileBases.Length;
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
        
        tempTimer.updateTime();
        testTimerText.text = convertTimetoMS(tempTimer.currentTime);
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
                if (!algaeCells[key].willSurvive(randNum, groundCells[key])) {
                    algaeTileMap.SetTile(key, null);
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
                        
                        AlgaeCellData cell = new AlgaeCellData {
                            LocalPlace = localPlace,
                            WorldLocation = algaeTileMap.CellToWorld(localPlace),
                            TileBase = algaeCells[key].TileBase,
                            TilemapMember = algaeTileMap,
                            name = localPlace.x + "," + localPlace.y,
                            maturity = 0.0f,
                            herbivorousFishProduction = UnityEngine.Random.Range(10,20)
                        };
                        herbivorousFishTotal += cell.herbivorousFishProduction;
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace)) {
                            coralTileMap.SetTile(localPlace, null);
                            herbivorousFishTotalInterest -= coralCells[localPlace].herbivorousFishInterest;
                            carnivorousFishTotalInterest -= coralCells[localPlace].carnivorousFishInterest;
                            coralCells.Remove(localPlace);
                        }
                        algaeCells.Add(cell.LocalPlace,cell);
                        algaeTileMap.SetTile(cell.LocalPlace, cell.TileBase);
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

        fishIncome = (int)Math.Round(carnivorousFishTotal*0.3f + herbivorousFishTotal*0.2f);
        fishOutput += fishIncome;
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
                if (!coralCells[key].willSurvive(randNum, groundCells[key], miscFactors)) {
                    coralTileMap.SetTile(key, null);
                    herbivorousFishTotalInterest -= coralCells[key].herbivorousFishInterest;
                    carnivorousFishTotalInterest -= coralCells[key].carnivorousFishInterest;
                    coralCells.Remove(key);
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
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || algaeTileMap.HasTile(localPlace)) continue;
                        CoralCellData cell = new CoralCellData {
                            LocalPlace = localPlace,
                            WorldLocation = coralTileMap.CellToWorld(localPlace),
                            TileBase = coralCells[key].TileBase,
                            TilemapMember = coralTileMap,
                            name = localPlace.x + "," + localPlace.y,
                            maturity = 0.0f,
                            carnivorousFishInterest = UnityEngine.Random.Range(0.0007f, 0.0035f),
                            herbivorousFishInterest = UnityEngine.Random.Range(0.001f,0.005f)
                        };
                        carnivorousFishTotalInterest += cell.carnivorousFishInterest;
                        herbivorousFishTotalInterest += cell.herbivorousFishInterest;
                        coralCells.Add(cell.LocalPlace,cell);
                        coralTileMap.SetTile(cell.LocalPlace, cell.TileBase);
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

        zoom = Mathf.Clamp(zoom, 5f, 15f);
    }

}

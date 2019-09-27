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
    [SerializeField] private Tilemap coralTileMap;
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap algaeTileMap;
    [SerializeField] private TileBase[] coralTileBases;
    [SerializeField] private TileBase[] groundTileBases;
    [SerializeField] private TileBase[] algaeTileBases;
    [SerializeField] private Text fishDisplay;
    #pragma warning restore 0649
    private float zoom = 10f;
    private float updateDelay = 5f;
    private int survivabilityFrameCounter = 0;
    private Vector3 cameraFollowPosition;
    private bool edgeScrollingEnabled = false;
    private Dictionary<Vector3Int,CoralCellData> coralCells;
    private Dictionary<Vector3Int,float> groundCells;
    private Dictionary<Vector3Int,AlgaeCellData> algaeCells; // eventually convert to algae cell data or unified structure
    private Dictionary<TileBase, float> probCoralSurvivabilityMax;
    private Dictionary<TileBase, float> probAlgaeSurvivabilityMax;
    private Dictionary<TileBase,int> coralFishValue;
    private int testnum = 0;
    private int fishOutput = 0;
    private int fishIncome = 0;
    private int smallFishTotal = 0;
    private int bigFishTotal = 0;
    private int herbivorousFishTotal = 0;
    private Vector3Int[,] hexNeighbors = new Vector3Int[,] {
        {new Vector3Int(1,0,0), new Vector3Int(0,-1,0), new Vector3Int(-1,-1,0), new Vector3Int(-1,0,0), new Vector3Int(-1,1,0), new Vector3Int(0,1,0)}, 
        {new Vector3Int(1,0,0), new Vector3Int(1,-1,0), new Vector3Int(0,-1,0), new Vector3Int(-1,0,0), new Vector3Int(0,1,0), new Vector3Int(1,1,0)} 
    };
    private Dictionary<int,Dictionary<Vector3Int,CoralCellData>> coralGroups;

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
        initializeGame();
    }

    private void initializeTiles() {
        coralFishValue = new Dictionary<TileBase, int>();
        for (int i = 0; i < coralTileBases.Length; i++) {
            coralFishValue.Add(coralTileBases[i], UnityEngine.Random.Range(5,15));
        }
        probCoralSurvivabilityMax = new Dictionary<TileBase, float>();
        probCoralSurvivabilityMax.Add(groundTileBases[0], 100);
        probCoralSurvivabilityMax.Add(groundTileBases[1], 85);
        probCoralSurvivabilityMax.Add(groundTileBases[2], 65);

        probAlgaeSurvivabilityMax = new Dictionary<TileBase, float>();
        probAlgaeSurvivabilityMax.Add(algaeTileBases[0], 90);
        probAlgaeSurvivabilityMax.Add(algaeTileBases[1], 90);

        coralGroups = new Dictionary<int, Dictionary<Vector3Int, CoralCellData>>();
        coralGroups.Add(0,new Dictionary<Vector3Int, CoralCellData>());

        // Setting the tiles in the tilemap to the coralCells dictionary
        coralCells = new Dictionary<Vector3Int,CoralCellData>();
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
                smallFishProduction = UnityEngine.Random.Range(10,50),
                bigFishProduction = UnityEngine.Random.Range(5,15),
                herbivorousFishProduction = UnityEngine.Random.Range(10,20),
                group = 0
            };
            smallFishTotal += cell.smallFishProduction;
            bigFishTotal += cell.bigFishProduction;
            herbivorousFishTotal += cell.herbivorousFishProduction;
            coralCells.Add(cell.LocalPlace, cell);
            coralGroups[0].Add(cell.LocalPlace, cell);
        }

        groundCells = new Dictionary<Vector3Int, float>();
        foreach (Vector3Int pos in groundTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!groundTileMap.HasTile(localPlace)) continue;
            groundCells.Add(localPlace, probCoralSurvivabilityMax[groundTileMap.GetTile(localPlace)]);
        }

        algaeCells = new Dictionary<Vector3Int, AlgaeCellData>();
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
                fishProduction = 0,
                smallFishProduction = UnityEngine.Random.Range(10,50),
                bigFishProduction = UnityEngine.Random.Range(5,15),
                herbivorousFishProduction = UnityEngine.Random.Range(10,20)
            };
            algaeCells.Add(cell.LocalPlace,cell);
        }

        // foreach (KeyValuePair<Vector3Int,AlgaeCellData> entry in algaeCells) {
        //     print("position: " + entry.Key + "\n" + entry.Value.printData());
        // }
    }

    private void initializeGame() {
        fishDisplay = GameObject.Find("FishDisplay").GetComponent<Text>();
        fishDisplay.text = "Fish Output: 0\nSmall Fish: 0\nBig Fish: 0\nHerbivorous Fish: 0\nFish Income: 0";
    }

    private void Start() {
        // sets the cameraFollowPosition to the default 
        cameraFollowPosition = cameraFollow.transform.position;
        cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
        InvokeRepeating("doStuff", 2.0f, 2.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
        // testing for hex tile coords
        bool lb = Input.GetMouseButtonDown(0);
        if (lb) {
            print("left mouse button has been pressed");
            Vector3Int position = getMouseGridPosition();
            print(":: " + position);
            if (coralCells.ContainsKey(position))
                print("::--> group: " + coralCells[position].group);
            // if (coralCells.ContainsKey(position)) {
            //     smallFishTotal -= coralCells[position].smallFishProduction;
            //     bigFishTotal -= coralCells[position].bigFishProduction;
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
                // coralCells[position].TileBase = coralTileBases[testnum];
                // coralCells[position].maturity = 0;
                // coralCells[position].fishProduction = coralFishValue[coralTileBases[testnum]];
                // coralCells[position].smallFishProduction = UnityEngine.Random.Range(10,50);
                // coralCells[position].bigFishProduction = UnityEngine.Random.Range(5,15);
                // coralCells[position].herbivorousFishProduction = UnityEngine.Random.Range(10,20);
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
                    smallFishProduction = UnityEngine.Random.Range(10,50),
                    bigFishProduction = UnityEngine.Random.Range(5,15),
                    herbivorousFishProduction = UnityEngine.Random.Range(10,20),
                    group = 0
                };
                coralCells.Add(position, cell);
                smallFishTotal += coralCells[position].smallFishProduction;
                bigFishTotal += coralCells[position].bigFishProduction;
                herbivorousFishTotal += coralCells[position].herbivorousFishProduction;
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
        
        moveCameraWASD(20f);
        if (edgeScrollingEnabled) moveCameraMouseEdge(20f,10f);
        zoomKeys(1f);
        
    }

    private void doStuff() {
        survivabilityFrameCounter = (++survivabilityFrameCounter % 5 == 0 ? 0 : survivabilityFrameCounter);
        if (survivabilityFrameCounter == 0) {
            updateCoralSurvivability();
            updateAllAlgae();
            updateCoralGrouping();
        }
        updateFishOutput();
        fishDisplay.text = "Fish Output: " + fishOutput
                        + "\nSmall Fish: " + smallFishTotal
                        + "\nBig Fish: " + bigFishTotal
                        + "\nHerbivorous Fish: " + herbivorousFishTotal
                        + "\nFish Income: " + fishIncome;
    }

    private void updateAllAlgae() {
        // handles propagation
        // refer to updateCoralSurvivability for structure
        // basically get list of keys, then propagate
        // https://www.redblobgames.com/grids/hexagons/
        // note: unity is using inverted odd-q; switch x and y then baliktad
        // each algae has a random propagation chance; generate a random num to roll chance

        // survival
        List<Vector3Int> keys = new List<Vector3Int>(algaeCells.Keys);
        foreach (Vector3Int key in keys) {
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            // print(randNum + " " + key);
            if (algaeCells[key].maturity <= 100.0f) {
                algaeCells[key].addMaturity(10);
                if (!algaeCells[key].willSurvive(randNum, groundCells[key])) {
                    // float temp = Mathf.Min((groundCells[key]-50.0f)/100.0f*algaeCells[key].maturity + 50.0f, groundCells[key]);
                    algaeTileMap.SetTile(key, null);
                    algaeCells.Remove(key);
                }
            }
            
        }

        // propagation
        keys = new List<Vector3Int>(algaeCells.Keys);
        foreach (Vector3Int key in keys) {
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            float basePropagationChance = UnityEngine.Random.Range(50.0f, 60.0f);
            if (algaeCells[key].maturity > 100.0f) { // propagate only if "mature"
                for (int i = 0; i < 6; i++) {
                    if (UnityEngine.Random.Range(0.0f,100.0f) <= basePropagationChance + UnityEngine.Random.Range(0.0f, 5.0f)) {
                        Vector3Int localPlace = key+hexNeighbors[key.y&1,i];
                        if (algaeTileMap.HasTile(localPlace) || algaeCells.ContainsKey(localPlace)) continue;
                        AlgaeCellData cell = new AlgaeCellData {
                            LocalPlace = localPlace,
                            WorldLocation = algaeTileMap.CellToWorld(localPlace),
                            TileBase = algaeCells[key].TileBase,
                            TilemapMember = algaeTileMap,
                            name = localPlace.x + "," + localPlace.y,
                            maturity = 0.0f,
                            fishProduction = 0,
                            smallFishProduction = UnityEngine.Random.Range(10,50),
                            bigFishProduction = UnityEngine.Random.Range(5,15),
                            herbivorousFishProduction = UnityEngine.Random.Range(10,20)
                        };
                        algaeCells.Add(cell.LocalPlace,cell);
                        algaeTileMap.SetTile(cell.LocalPlace, cell.TileBase);
                    }
                }
            }
        }
    }

    private void updateFishOutput() {
        int toBeAdded = 0;
        foreach(KeyValuePair<Vector3Int, CoralCellData> entry in coralCells) {
            toBeAdded += entry.Value.fishProduction;
        }
        fishIncome = toBeAdded;
        fishOutput += toBeAdded;
    }

    private void updateCoralSurvivability() {
        List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
        foreach (Vector3Int key in keys) {
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            // print(randNum + " " + key);
            if (coralCells[key].maturity <= 100) {
                coralCells[key].addMaturity(10);
                if (!coralCells[key].willSurvive(randNum, groundCells[key])) {
                    coralTileMap.SetTile(key, null);
                    coralCells.Remove(key);
                }
            }
            
        }
    }

    private void updateCoralGrouping() {
        foreach(KeyValuePair<Vector3Int,CoralCellData> entry in coralCells) {
            Vector3Int pos = entry.Key;
            CoralCellData posData = entry.Value;
            float randNum = UnityEngine.Random.Range(0.0f, 100.0f);
            float baseFusionChance = UnityEngine.Random.Range(25.0f, 50.0f);
            if (posData.maturity > 100.0f) {
                for (int i = 0; i < 6; i++) {
                    if (!coralCells.ContainsKey(pos+hexNeighbors[pos.y&1,i])) continue;
                    CoralCellData other = coralCells[pos+hexNeighbors[pos.y&1,i]];
                    if (other.maturity > 100.0f && randNum <= baseFusionChance) {
                        // add them to a data struct
                        if (posData.group == 0 && other.group == 0) {
                            int availableGroup = checkAvailableGroup();
                            coralGroups.Add(availableGroup, new Dictionary<Vector3Int, CoralCellData>());
                            entry.Value.group = availableGroup;
                            coralCells[pos+hexNeighbors[pos.y&1,i]].group = availableGroup;
                            coralGroups[availableGroup].Add(entry.Value.LocalPlace, entry.Value);
                            coralGroups[availableGroup].Add(other.LocalPlace, coralCells[pos+hexNeighbors[pos.y&1,i]]);
                            coralGroups[0].Remove(entry.Key);
                            coralGroups[0].Remove(other.LocalPlace);
                        } else if (posData.group == 0 && other.group != 0) {
                            entry.Value.group = other.group;
                            coralGroups[other.group].Add(entry.Key, entry.Value);
                            coralGroups[0].Remove(entry.Key);
                        } else if (posData.group != 0 && other.group == 0) {
                            coralCells[pos+hexNeighbors[pos.y&1,i]].group = posData.group;
                            coralGroups[posData.group].Add(other.LocalPlace, coralCells[pos+hexNeighbors[pos.y&1,i]]);
                            coralGroups[0].Remove(other.LocalPlace);
                        } else {
                            // do nothing
                        }
                    }
                }
            }
        }
    }

    private int checkAvailableGroup() {
        int available = 0;
        for (int i = 1; i < 3601; i++) {
            if (!coralGroups.ContainsKey(i)) {
                available = i;
                break;
            }
        }
        return available;
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

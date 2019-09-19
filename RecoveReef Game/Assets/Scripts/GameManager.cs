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
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private TileBase[] tileBases;
    [SerializeField] private Text fishDisplay;
    #pragma warning restore 0649
    private float zoom = 10f;
    private float updateDelay = 5f;
    private Vector3 cameraFollowPosition;
    private bool edgeScrollingEnabled = false;
    private Dictionary<Vector3Int,CellData> cells;
    private Dictionary<TileBase,int> coralFishValue;
    private int testnum = 0;
    private int fishOutput = 0;

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
        for (int i = 0; i < tileBases.Length; i++) {
            coralFishValue.Add(tileBases[i], UnityEngine.Random.Range(20,100));
        }

        // Setting the tiles in the tilemap to the cells dictionary
        cells = new Dictionary<Vector3Int,CellData>();
        foreach(Vector3Int pos in tileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!tileMap.HasTile(localPlace)) continue;
            CellData cell = new CellData {
                LocalPlace = localPlace,
                WorldLocation = tileMap.CellToWorld(localPlace),
                TileBase = tileMap.GetTile(localPlace),
                TilemapMember = tileMap,
                name = localPlace.x + "," + localPlace.y,
                maturity = 0,
                fishProduction = coralFishValue[tileMap.GetTile(localPlace)]
            };
            cells.Add(cell.LocalPlace, cell);
        }

        print("printing contents of coralFishValue");
        foreach(KeyValuePair<TileBase, int> entry in coralFishValue) {
            print(entry.Key + " : " + entry.Value);
        }

        print("printing contents of cells");
        foreach(KeyValuePair<Vector3Int, CellData> entry in cells) {
            print(entry.Key + " :---\n" + entry.Value.printData());
        }
    }

    private void initializeGame() {
        fishDisplay = GameObject.Find("FishDisplay").GetComponent<Text>();
        fishDisplay.text = "Fish Output: 0";
    }

    private void Start() {
        // sets the cameraFollowPosition to the default 
        cameraFollowPosition = cameraFollow.transform.position;
        cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
        InvokeRepeating("doStuff", 2.0f, 2.0f);
        // cameraFollow.Setup(() => new Vector3(0,-100));
    }

    // Update is called once per frame
    void Update()
    {
        // testing for hex tile coords
        bool lb = Input.GetMouseButtonDown(0);
        if (lb) {
            print("left mouse button has been pressed");
            Vector3Int position = getMouseGridPosition();
            print("position: " + position);

            tileMap.SetTile(position, null);
            cells.Remove(position);
        }

        bool rb = Input.GetMouseButtonDown(1);
        if (rb) {
            print("right mouse button has been pressed");
            Vector3Int position = getMouseGridPosition();
            print("position: " + position);
            if (tileMap.HasTile(position)) {
                print(cells[position].printData());
                cells[position].TileBase = tileBases[testnum];
                cells[position].maturity = 0;
                cells[position].fishProduction = coralFishValue[tileBases[testnum]];
            } else {
                CellData cell = new CellData {
                    LocalPlace = position,
                    WorldLocation = tileMap.CellToWorld(position),
                    TileBase = tileBases[testnum],
                    TilemapMember = tileMap,
                    name = position.x + "," + position.y,
                    maturity = 0,
                    fishProduction = coralFishValue[tileBases[testnum]]
                };
                cells.Add(position, cell);
            }
            tileMap.SetTile(position, tileBases[testnum]);
            if (tileMap.HasTile(position))
                print(cells[position].printData());
            testnum = (testnum+1) % tileBases.Length;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            edgeScrollingEnabled = !edgeScrollingEnabled;
            print("edgeScrolling = " + edgeScrollingEnabled);
        }

        // movement of screen
        moveCameraWASD(20f);
        if (edgeScrollingEnabled) moveCameraMouseEdge(20f,10f);
        zoomKeys(1f);
        
    }

    private void doStuff() {
        updateFishOutput();
        fishDisplay.text = "Fish Output: " + fishOutput;
    }

    private void updateFishOutput() {
        int toBeAdded = 0;
        foreach(KeyValuePair<Vector3Int, CellData> entry in cells) {
            toBeAdded += entry.Value.fishProduction;
        }
        print("toBeAdded: " + toBeAdded);
        fishOutput += toBeAdded;
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

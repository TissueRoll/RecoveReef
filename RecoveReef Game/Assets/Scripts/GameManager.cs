using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    #region Things I Plug in Unity
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
    [SerializeField] private TileBase[] toxicTileBases;
    [SerializeField] private Text fishDisplay;
    [SerializeField] private Text testTimerText;
    [SerializeField] private GameObject CNC;
    [SerializeField] private GameObject[] CoralOptions;
    [SerializeField] private Text feedbackText;
    [SerializeField] private TileBase toxicOverlay;
    [SerializeField] private GameObject popupCanvas;
    [SerializeField] private Sprite emptyRack;
    [SerializeField] private Sprite[] smallRack;
    [SerializeField] private Sprite[] bigRack;
    [SerializeField] private GameObject endGameScreen;
    #pragma warning restore 0649
    #endregion
    #region Data Structures for the Game
    private Dictionary<Vector3Int,CoralCellData> coralCells;
    private Dictionary<Vector3Int,float> substrataCells;
    private Dictionary<Vector3Int,AlgaeCellData> algaeCells; 
    #endregion
    #region Global Unchanging Values
    private Vector3Int[,] hexNeighbors = new Vector3Int[,] {
        {new Vector3Int(1,0,0), new Vector3Int(0,-1,0), new Vector3Int(-1,-1,0), new Vector3Int(-1,0,0), new Vector3Int(-1,1,0), new Vector3Int(0,1,0)}, 
        {new Vector3Int(1,0,0), new Vector3Int(1,-1,0), new Vector3Int(0,-1,0), new Vector3Int(-1,0,0), new Vector3Int(0,1,0), new Vector3Int(1,1,0)} 
    };
    GlobalContainer globalVarContainer;
    CoralDataContainer coralBaseData;
    SubstrataDataContainer substrataDataContainer;
    AlgaeDataContainer algaeDataContainer;
    Color progressNotDone = new Color(43f/255f,90f/255f,147f/255f,1f);
    Color progressIsDone = new Color(0f,1f,176f/255f,1f);
    #endregion
    #region Global Changing Values
    private float zoom;
    private Vector3 cameraFollowPosition;
    private Vector3 savedCameraPosition;
    private bool edgeScrollingEnabled = false;
    private bool showNursery = false;
    private int testnum = 0;
    private int fishOutput = 0;
    private int fishIncome = 0;
    private float cfTotalProduction = 0;
    private float hfTotalProduction = 0;
    private CountdownTimer tempTimer;
    private List<NursingCoral>[] growingCorals;
    private CountdownTimer disasterTimer;
    private CountdownTimer climateChangeTimer;
    private bool climateChangeHasWarned;
    private bool climateChangeHasHappened;
    private float coralPropagationDebuff = 0;
    private float coralSurvivabilityDebuff = 0;
    private EconomyMachine economyMachine;
    private CountdownTimer timeUntilEnd;
    #endregion

    #region Generic Helper Functions
    private Vector3Int getMouseGridPosition() {
        Grid grid = GameObject.Find("Grid").GetComponent<Grid>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPoint = ray.GetPoint(-ray.origin.z/ray.direction.z);
        Vector3Int position = grid.WorldToCell(worldPoint);
        return position;
    }
    private string convertTimetoMS(float rawTime) {
        string minutes = Mathf.Floor(rawTime/60).ToString("00");
        string seconds = Mathf.RoundToInt(rawTime%60).ToString("00");
        return string.Format("{0}:{1}", minutes, seconds);
    }
    private HashSet<Vector3Int> spread (Vector3Int position, int level) {
        HashSet<Vector3Int> result = new HashSet<Vector3Int>();
        result.Add(position);
        if (level > 0) {
            for (int i = 0; i < 6; i++) {
                Vector3Int posNeighbor = position+hexNeighbors[position.y&1,i];
                result.UnionWith(spread(posNeighbor,level-1));
            }
        }
        return result;
    }
    #endregion
    #region Game-Specific Helper Functions
    private int findIndexOfEntityFromType (string code, string type) {
        int index = -1;
        if (type == "coral") {
            for (int i = 0; i < coralBaseData.corals.Count; i++) {
                if (Regex.IsMatch(coralBaseData.corals[i].name, code)) {
                    index = i;
                    break;
                }
            }
        } else if (type == "algae") {
            for (int i = 0; i < algaeDataContainer.algae.Count; i++) {
                if (Regex.IsMatch(algaeDataContainer.algae[i].name, code)) {
                    index = i;
                    break;
                }
            }
        } else if (type == "substrata") {
            for (int i = 0; i < substrataDataContainer.substrata.Count; i++) {
                if (Regex.IsMatch(substrataDataContainer.substrata[i].name, code)) {
                    index = i;
                    break;
                }
            }
        }
        if (index == -1)
            print("ERROR: Entity not found");
        return index;
    }
    private int findIndexOfEntityFromName (string nameOfTileBase) {
        int index = -1;
        if (Regex.IsMatch(nameOfTileBase, ".*coral.*")) {
            for (int i = 0; i < coralBaseData.corals.Count; i++) {
                if (Regex.IsMatch(nameOfTileBase, ".*coral_"+coralBaseData.corals[i].name+".*")) {
                    index = i;
                    break;
                }
            }
        } else if (Regex.IsMatch(nameOfTileBase, ".*algae.*")) {
            for (int i = 0; i < algaeDataContainer.algae.Count; i++) {
                if (Regex.IsMatch(nameOfTileBase, ".*algae_"+algaeDataContainer.algae[i].name+".*")) {
                    index = i;
                    break;
                }
            }
        } else if (Regex.IsMatch(nameOfTileBase, ".*substrata.*")) {
            for (int i = 0; i < substrataDataContainer.substrata.Count; i++) {
                if (Regex.IsMatch(nameOfTileBase, ".*substrata_"+substrataDataContainer.substrata[i].name+".*")) {
                    index = i;
                    break;
                }
            }
        }
        if (index == -1)
            print("ERROR: Entity not found");
        return index;
    }
    public void change_coral(int select) {
        testnum = select;
    }
    private int getCoralsPerType(int type) {
        int result = 0;
        for (int i = 0; i < globalVarContainer.globalVariables.maxSpacePerCoral; i++) {
            if (growingCorals[type][i] == null)
                continue;
            result += 1;
        }
        return result;
    }
    private int getCoralsInNursery() {
        int coralsInNursery = 0;
        for (int i = 0; i < 6; i++) {
            coralsInNursery += getCoralsPerType(i);
        }
        return coralsInNursery;
    }
    private int getReadyCoralsPerType(int type) {
        int ready = 0;
        for (int i = 0; i < globalVarContainer.globalVariables.maxSpacePerCoral; i++) {
            if (growingCorals[type][i] == null)
                continue;
            if (growingCorals[type][i].timer.isDone())
                ready += 1;
        }
        return ready;
    }
    private int getIndexOfReadyCoral(int type) {
        int index = -1;
        for (int i = 0; i < globalVarContainer.globalVariables.maxSpacePerCoral; i++) {
            if (growingCorals[type][i] == null)
                continue;
            if (index == -1 && growingCorals[type][i].timer.isDone())
                index = i;
        }
        return index;
    }
    #endregion

    void Awake()
    {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        print("loading XML data...");
        globalVarContainer = GlobalContainer.Load("GlobalsXML");
        substrataDataContainer = SubstrataDataContainer.Load("SubstrataXML");
        coralBaseData = CoralDataContainer.Load("CoralDataXML");
        algaeDataContainer = AlgaeDataContainer.Load("AlgaeDataXML");
        zoom = globalVarContainer.globalVariables.zoom;
        print("XML data loaded");
        print("initializing tiles...");
        initializeTiles();
        print("initialization done");
        tempTimer = new CountdownTimer(globalVarContainer.globalVariables.maxGameTime);
        disasterTimer = new CountdownTimer(1200f); // make into first 5 mins immunity
        climateChangeTimer = new CountdownTimer(globalVarContainer.globalVariables.timeUntilClimateChange);
        climateChangeHasWarned = false;
        climateChangeHasHappened = false;
        economyMachine = new EconomyMachine(10f,0f,5f);
        timeUntilEnd = new CountdownTimer(60f);
        initializeGame();
    }


    private void initializeTiles() {
        // instantiation
        coralCells = new Dictionary<Vector3Int,CoralCellData>();
        substrataCells = new Dictionary<Vector3Int, float>();
        algaeCells = new Dictionary<Vector3Int, AlgaeCellData>();
        growingCorals = new List<NursingCoral>[6];
        for (int i = 0; i < 6; i++) {
            growingCorals[i] = new List<NursingCoral>() {null, null, null, null};
        }

        // initialization
        // Setting the substrata data
        foreach (Vector3Int pos in substrataTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!substrataTileMap.HasTile(localPlace)) continue;
            TileBase currentTB = substrataTileMap.GetTile(localPlace);
            int idx = findIndexOfEntityFromName(currentTB.name);
            if (idx == -1) { // UNKNOWN TILE; FOR NOW TOXIC
                HashSet<Vector3Int> toxicSpread = spread(localPlace, 2);
                foreach (Vector3Int toxicPos in toxicSpread) {
                    substrataOverlayTileMap.SetTile(toxicPos,toxicOverlay);
                }
            } else {
                substrataCells.Add(localPlace, substrataDataContainer.substrata[idx].groundViability);
            }
        }
        
        // Setting the tiles in the tilemap to the coralCells dictionary
        foreach(Vector3Int pos in coralTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!coralTileMap.HasTile(localPlace)) continue;
            if (!substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace)) {
                coralTileMap.SetTile(localPlace, null);
                continue;
            }
            TileBase currentTB = coralTileMap.GetTile(localPlace);
            CoralCellData cell = new CoralCellData(
                localPlace, 
                coralTileMap, 
                currentTB, 
                101.0f, 
                coralBaseData.corals[findIndexOfEntityFromName(currentTB.name)]
            );
            cfTotalProduction += cell.coralData.cfProduction;
            hfTotalProduction += cell.coralData.hfProduction;
            coralCells.Add(cell.LocalPlace, cell);
        }
        
        foreach (Vector3Int pos in algaeTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!algaeTileMap.HasTile(localPlace)) continue;
            if (!substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace)) {
                algaeTileMap.SetTile(localPlace, null);
                continue;
            }
            TileBase currentTB = algaeTileMap.GetTile(localPlace);
            AlgaeCellData cell = new AlgaeCellData(
                localPlace, 
                algaeTileMap, 
                currentTB, 
                101.0f, 
                algaeDataContainer.algae[findIndexOfEntityFromName(currentTB.name)]
            );
            hfTotalProduction += cell.algaeData.hfProduction;
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
        // __FIX__ MAKE INTO GLOBALS?
        // InvokeRepeating("updateFishData", 0f, 1.0f);
        InvokeRepeating("updateAllAlgae", 1.0f, 1.0f); 
        InvokeRepeating("updateAllCoral", 2.0f, 2.0f); 
    }

    void Update()
    {
        if (GameEnd.gameHasEnded) {
            return;
        }

        // if (Input.GetKeyDown(KeyCode.Backspace)) {
        //     endTheGame("force end");
        // }

        updateFishData();

        if (PauseScript.GamePaused) {
            return;
        }

        #region Disaster Happenings
        disasterTimer.updateTime();
        if (disasterTimer.isDone()) {
            disasterTimer = new CountdownTimer(60f);
            randomDisaster();
        }
        
        if (!climateChangeTimer.isDone())
            climateChangeTimer.updateTime();
        if (!climateChangeHasWarned && climateChangeTimer.currentTime <= climateChangeTimer.timeDuration*(2.0/3.0)) {
            climateChangeHasWarned = true;
            makePopup("Scientists have predicted that our carbon emmisions will lead to devastating damages to sea life in a few years! This could slow down the growth of coral reefs soon...");
        } else if (climateChangeHasWarned && !climateChangeHasHappened && climateChangeTimer.isDone()) {
            climateChangeHasHappened = true;
            makePopup("Scientists have determined that the increased temperature and ocean acidity has slowed down coral growth! We have to make a greater effort to coral conservation and rehabilitation!");
            applyClimateChange();
        }

        // test script for popup messages
        if (Input.GetKeyDown(KeyCode.Slash)) {
            
            makePopup("Hello!");
            // randomDisaster(2);
        }
        #endregion

        #region Keyboard Shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            tryGrowCoral(0);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            tryGrowCoral(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            tryGrowCoral(2);
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            tryGrowCoral(3);
        } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            tryGrowCoral(4);
        } else if (Input.GetKeyDown(KeyCode.Alpha6)) {
            tryGrowCoral(5);
        } 

        if (Input.GetKeyDown(KeyCode.Z)) {
            change_coral(0);
        } else if (Input.GetKeyDown(KeyCode.X)) {
            change_coral(1);
        } else if (Input.GetKeyDown(KeyCode.C)) {
            change_coral(2);
        } else if (Input.GetKeyDown(KeyCode.V)) {
            change_coral(3);
        } else if (Input.GetKeyDown(KeyCode.B)) {
            change_coral(4);
        } else if (Input.GetKeyDown(KeyCode.N)) {
            change_coral(5);
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            openThing();
        }
        #endregion

        // testing for hex tile coords
        if (Input.GetMouseButtonDown(0)) {
            Vector3Int position = getMouseGridPosition();
            print("L:: " + position);
            // print(":: " + substrataOverlayTileMap.GetTile(position).name); // temp disabled cuz it can error
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0);
            if (hit) {
                if (hit.collider.CompareTag("plastic")) {
                    Debug.Log("This is plastic");
                }
            }
        }

        bool rb = Input.GetMouseButtonDown(1);
        if (rb) {
            if(!plantCoral(testnum)) {
                // feedbackDialogue("Cannot plant coral onto the reef", 1);
            }
        }

        #region Screen Movement
        // movement of screen
        if (Input.GetKeyDown(KeyCode.Space)) {
            edgeScrollingEnabled = !edgeScrollingEnabled;
            print("edgeScrolling = " + edgeScrollingEnabled);
        }
        
        // transporting to some far off place for the nursery
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            operateNursery();
        }

        if (!showNursery) {
            moveCameraWASD(20f);
            if (edgeScrollingEnabled) moveCameraMouseEdge(20f,10f);
            zoomKeys(1f);
            clampCamera();
        }
        #endregion
        
        for (int i = 0; i < 6; i++) {
            GameObject thing = CoralOptions[i].transform.Find("CoralIndicator").gameObject;
            GameObject rack = nurseryCamera.transform.Find("NurseryCanvas/Racks")
                            .gameObject.transform.GetChild(i/3)
                            .gameObject.transform.GetChild(i%3)
                            .gameObject.transform.Find("Selection/RackPlacements").gameObject;
            for (int j = 0; j < globalVarContainer.globalVariables.maxSpacePerCoral; j++) {
                Sprite currentPositionSprite = rack.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>().sprite;
                if (growingCorals[i][j] == null) {
                    thing.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>().color = progressNotDone;
                    if (currentPositionSprite.name != emptyRack.name)
                        rack.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>().sprite = emptyRack;
                    continue;
                }
                growingCorals[i][j].timer.updateTime();
                bool currentCoralDone = growingCorals[i][j].timer.isDone();
                thing.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(progressNotDone, progressIsDone, growingCorals[i][j].timer.percentComplete);
                if (currentCoralDone && currentPositionSprite.name != bigRack[i].name) 
                    rack.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>().sprite = bigRack[i];
                else if (!currentCoralDone && currentPositionSprite.name != smallRack[i].name)
                    rack.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>().sprite = smallRack[i];
            }
        }

        CNC.GetComponent<TMPro.TextMeshProUGUI>().text = getCoralsInNursery() + "/" + globalVarContainer.globalVariables.maxSpaceInNursery + " SLOTS LEFT";

        tempTimer.updateTime();
        testTimerText.text = convertTimetoMS(tempTimer.currentTime);
        if (tempTimer.isDone()) {
            endTheGame("The reef could not recover...");
        }

        if (fishIncome >= 50000f) {
            timeUntilEnd.updateTime();
        } else {
            timeUntilEnd.reset();
        }

        if (timeUntilEnd.isDone()) {
            endTheGame("You have recovered the reef!");
        }
    }
    private void endTheGame(string s) {
        endGameScreen.GetComponent<GameEnd>().endMessage(s);
        endGameScreen.GetComponent<GameEnd>().gameEndReached();
    }
    public void operateNursery() {
        showNursery = !showNursery;
        if (showNursery) {
            savedCameraPosition = cameraFollow.transform.position;
            cameraFollow.enabled = false;
            nurseryCamera.enabled = true;
            nurseryCamera.transform.Find("NurseryCanvas").gameObject.SetActive(true); // BAND AID
        } else {
            cameraFollow.enabled = true;
            nurseryCamera.transform.Find("NurseryCanvas").gameObject.SetActive(false); // BAND AID
            nurseryCamera.enabled = false;
            cameraFollowPosition = savedCameraPosition;
            cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
        }
    }

    private void makePopup(string s) {
        popupCanvas.GetComponent<PopupScript>().SetPopupMessage(s);
        popupCanvas.GetComponent<PopupScript>().OpenPopup();
    }

    private void feedbackDialogue(string text, float time) {
        StartCoroutine(ShowMessage(text,time));
    }

    IEnumerator ShowMessage(string text, float time) {
        feedbackText.text = text;
        feedbackText.enabled = true;
        yield return new WaitForSeconds(time);
        feedbackText.enabled = false;
    }

    public void openThing() {
        cameraFollow.GetComponent<MenuAnimator>().OpenThing();
    }

    private void updateFishData() {        
        updateFishOutput();
        fishDisplay.text = "Fish Output: " + fishOutput
                        + "\nHerbivorous Fish: " + economyMachine.getActualHF()
                        + "\nCarnivorous Fish: " + economyMachine.getActualCF()
                        + "\nFish Income: " + fishIncome;
    }

    // __ECONOMY__
    #region Algae Updates
    private void updateAllAlgae() {
        updateAlgaeSurvivability();
        updateAlgaePropagation();
    }

    private void updateAlgaeSurvivability() {
        List<Vector3Int> keys = new List<Vector3Int>(algaeCells.Keys);
        foreach (Vector3Int key in keys) {
            if (algaeCells[key].maturity <= 100.0f) {
                algaeCells[key].addMaturity(1);
                if (!economyMachine.algaeWillSurvive(algaeCells[key], substrataCells[key], 0f, 1f)) { // BUGGED
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
            if (algaeCells[key].maturity > 100.0f) { // propagate only if "mature"
                for (int i = 0; i < 6; i++) {
                    if (economyMachine.algaeWillPropagate(algaeCells[key], 0f, 1f)) {
                        Vector3Int localPlace = key+hexNeighbors[key.y&1,i];
                        if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace)) continue;
                        if (substrataOverlayTileMap.HasTile(localPlace)) continue;
                        if (algaeTileMap.HasTile(localPlace) || algaeCells.ContainsKey(localPlace)) continue;
                        // __ECONOMY__ MANUAL OVERRIDE TO CHECK IF ALGAE CAN TAKE OVER
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace)) {
                            randNum -= coralCells[localPlace].maturity*0.15f;
                            CoralCellData temp;
                            float avgMaturity = 0;
                            float numCorals = 1;
                            // __FIX__ can improve this check with spread; but will do for now
                            for (int j = 0; j < 6; j++)
                                if (coralCells.TryGetValue(localPlace+hexNeighbors[localPlace.y&1, j], out temp)) {
                                    randNum -= coralCells[localPlace+hexNeighbors[localPlace.y&1, j]].maturity*0.05f;
                                    avgMaturity += coralCells[localPlace+hexNeighbors[localPlace.y&1, j]].maturity;
                                    numCorals++;
                                }
                            if (randNum < avgMaturity/numCorals) continue;
                        }
                        // adding algae
                        AlgaeCellData cell = new AlgaeCellData(
                            localPlace, 
                            algaeTileMap, 
                            algaeCells[key].TileBase, 
                            0.0f, 
                            algaeDataContainer.algae[findIndexOfEntityFromName(algaeCells[key].TileBase.name)]
                        );
                        hfTotalProduction += cell.algaeData.hfProduction;
                        algaeCells.Add(cell.LocalPlace,cell);
                        algaeTileMap.SetTile(cell.LocalPlace, cell.TileBase);
                        // delete coral under algae
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace)) {
                            coralTileMap.SetTile(localPlace, null);
                            hfTotalProduction -= coralCells[localPlace].coralData.hfProduction;
                            cfTotalProduction -= coralCells[localPlace].coralData.cfProduction;
                            coralCells.Remove(localPlace);
                        }
                    }
                }
            }
        }

    }
    #endregion

    #region Coral Updates
    private void updateAllCoral() {
        updateCoralSurvivability();
        updateCoralPropagation();
    }
    public void tryGrowCoral(int type) {
        growCoral(type);
        // if (!growCoral(type))
        //     feedbackDialogue("Cannot grow coral.", 1);
    }

    private void growCoral(int type) {
        bool spaceInNursery = getCoralsInNursery() < globalVarContainer.globalVariables.maxSpaceInNursery;
        bool underMaxGrow = getCoralsPerType(type) < globalVarContainer.globalVariables.maxSpacePerCoral;
        if (spaceInNursery && underMaxGrow) {
            int nullIdx = -1;
            for (int i = 0; i < globalVarContainer.globalVariables.maxSpacePerCoral; i++) {
                if (growingCorals[type][i] == null)
                    nullIdx = i;
            }
            if (nullIdx == -1) {
                print("ERROR: no null spot found");
            } else {
                growingCorals[type][nullIdx] = new NursingCoral(coralBaseData.corals[type].name, new CountdownTimer(coralBaseData.corals[type].growTime));
            }
        } else if (!underMaxGrow) {
            feedbackDialogue("Can only grow 4 corals per type.", globalVarContainer.globalVariables.feedbackDelayTime);
        } else if (!spaceInNursery) {
            feedbackDialogue("Nursery is at maximum capacity.", globalVarContainer.globalVariables.feedbackDelayTime);
        }
    }

    private bool plantCoral(int type) {
        bool successful = false;
        // should be unable to replace a tile
        // need to add more useful stuff like notifying why you can place a tile here
        print("right mouse button has been pressed");
        Vector3Int position = getMouseGridPosition();
        print("position: " + position);
        int readyNum = getReadyCoralsPerType(type);
        int loadedNum = getCoralsPerType(type);
        if (coralTileMap.HasTile(position)) {
            feedbackDialogue("Can't put corals on top of other corals!.", globalVarContainer.globalVariables.feedbackDelayTime);
        } else if (algaeTileMap.HasTile(position)) {
            feedbackDialogue("Can't put corals on top of algae! The coral will die!.", globalVarContainer.globalVariables.feedbackDelayTime);
        } else if (substrataOverlayTileMap.HasTile(position)) {
            feedbackDialogue("This is a toxic tile! Coral won't survive here.", globalVarContainer.globalVariables.feedbackDelayTime);
        }else if ((substrataTileMap.HasTile(position) || substrataCells.ContainsKey(position)) && readyNum > 0) { 
            successful = true;
            int tempIdx = getIndexOfReadyCoral(type);
            NursingCoral tempCoral = null;
            if (tempIdx != -1) {
                tempCoral = growingCorals[type][tempIdx];
                growingCorals[type][tempIdx] = null;
            }
            CoralCellData cell = new CoralCellData(
                position, 
                coralTileMap, 
                coralTileBases[type], 
                0.0f, 
                coralBaseData.corals[type]
            );
            coralCells.Add(position, cell);
            cfTotalProduction += coralCells[position].coralData.cfProduction;
            hfTotalProduction += coralCells[position].coralData.hfProduction;
            coralTileMap.SetTile(position, coralTileBases[type]);
        } else if (readyNum == 0 && loadedNum-readyNum > 0) {
            float minTime = 3600f;
            for (int i = 0; i < globalVarContainer.globalVariables.maxSpacePerCoral; i++) {
                if (growingCorals[type][i] == null)
                    continue;
                minTime = Math.Min(minTime, growingCorals[type][i].timer.currentTime);
            }
            string t = "Soonest to mature coral of this type has " + convertTimetoMS(minTime) + " time left.";
            feedbackDialogue(t, globalVarContainer.globalVariables.feedbackDelayTime);
        }

        return successful;
    }
    private void updateCoralSurvivability() {
        List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
        foreach (Vector3Int key in keys) {
            if (coralCells[key].maturity <= 100) {
                // check adj corals
                // miscFactors aka the amount of corals around it influences how much more they can add to the survivability of one
                // how much they actually contribue can be varied; change the amount 0.01f to something that makes more sense
                float miscFactors = 0.0f;
                // __FIX__ MAYBE USE SPREAD?
                for (int i = 0; i < 6; i++)
                    if (coralCells.ContainsKey(key+hexNeighbors[key.y&1, i]))
                        miscFactors += 0.01f*coralCells[key+hexNeighbors[key.y&1, i]].maturity; 
                coralCells[key].addMaturity(1.0f);
                if (!economyMachine.coralWillSurvive(coralCells[key], substrataCells[key], miscFactors, coralSurvivabilityDebuff)) {
                    // setting data
                    coralTileMap.SetTile(key, null);
                    hfTotalProduction -= coralCells[key].coralData.hfProduction;
                    cfTotalProduction -= coralCells[key].coralData.cfProduction;
                    coralCells.Remove(key);
                }
            }
        }
    }

    private void updateCoralPropagation() {
        List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
        foreach (Vector3Int key in keys) {
            if (coralCells[key].maturity > 100.0f) { // propagate only if "mature"
                for (int i = 0; i < 6; i++) {
                    if (economyMachine.coralWillPropagate(coralCells[key], 0f, 1f)) { // __FIX__ the additive and multiplicative
                        Vector3Int localPlace = key+hexNeighbors[key.y&1,i];
                        if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace)) continue;
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || algaeTileMap.HasTile(localPlace)) continue;
                        CoralCellData cell = new CoralCellData(
                            localPlace, 
                            coralTileMap, 
                            coralCells[key].TileBase, 
                            0.0f, 
                            coralBaseData.corals[findIndexOfEntityFromName(coralCells[key].TileBase.name)]
                        );
                        cfTotalProduction += cell.coralData.cfProduction;
                        hfTotalProduction += cell.coralData.hfProduction;
                        coralCells.Add(cell.LocalPlace,cell);
                        coralTileMap.SetTile(cell.LocalPlace, cell.TileBase);
                    }
                }
            }
        }
    }
    #endregion
    #region Disasters
    private void randomDisaster(int forceEvent = 0) {
        // chance: 1/100
        // random: random area selection
        // toxic: center must be a coral
        int t = UnityEngine.Random.Range(0,1000);
        if (t == 69 || forceEvent == 1) {
            if (coralCells.Count > 15) {
                Vector3Int pos = coralCells.ElementAt(UnityEngine.Random.Range(0,coralCells.Count)).Key;
                HashSet<Vector3Int> removeSpread = spread(pos, 2);
                foreach (Vector3Int removePos in removeSpread) {
                    if (coralCells.ContainsKey(removePos)) {
                        coralCells.Remove(removePos);
                        coralTileMap.SetTile(removePos, null);
                    }
                }
                makePopup("Oh no! A group of tourists took coral parts as a souvenir! A coral group has died due to this!");
            }
        } else if (t == 420 || forceEvent == 2) {
            Vector3Int pos = new Vector3Int(UnityEngine.Random.Range(-30,31), UnityEngine.Random.Range(-30,31), 0);
            if (substrataCells.ContainsKey(pos))
                substrataCells.Remove(pos);
            substrataTileMap.SetTile(pos, toxicTileBases[UnityEngine.Random.Range(0,toxicTileBases.Length)]);
            HashSet<Vector3Int> toxicSpread = spread(pos, 2);
            foreach (Vector3Int toxicPos in toxicSpread) {
                if (coralCells.ContainsKey(toxicPos)) {
                    coralCells.Remove(toxicPos);
                    coralTileMap.SetTile(toxicPos, null);
                }
                if (algaeCells.ContainsKey(toxicPos)) {
                    algaeCells.Remove(toxicPos);
                    algaeTileMap.SetTile(toxicPos, null);
                }
                substrataOverlayTileMap.SetTile(toxicPos,toxicOverlay);
            }
            makePopup("Oh no! Toxic waste has been dumped in the ocean again! Seaweed and coral alike have died around the fallen toxic waste.");
        }
    }
    // __ECONOMY__
    private void applyClimateChange() {
        // scripted event
        // reduce the growth rates globally
        coralPropagationDebuff = 1f;
        coralSurvivabilityDebuff = 1f;
    }
    #endregion

    // __ECONOMY__
    #region Misc Updates
    private void updateFishOutput() {
        economyMachine.updateHFCF(hfTotalProduction, cfTotalProduction);
        float hf = economyMachine.getActualHF();
        float cf = economyMachine.getActualCF();
        fishIncome = (int)Math.Round(economyMachine.getTotalFish());
        fishOutput += fishIncome;
        fishOutput = Math.Min(fishOutput, 50000);
    }
    #endregion

    #region Camera Movement and Zoom
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

    private void clampCamera() {
        cameraFollowPosition = new Vector3(
            Mathf.Clamp(cameraFollowPosition.x, -75f, 75f),
            Mathf.Clamp(cameraFollowPosition.y, -75f, 75f),
            cameraFollowPosition.z
        );
    }
    #endregion
}

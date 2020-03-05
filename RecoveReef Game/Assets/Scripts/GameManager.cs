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
    [SerializeField] private TileBase[] coralDeadTileBases;
    [SerializeField] private TileBase[] groundTileBases;
    [SerializeField] private TileBase[] algaeTileBases;
    [SerializeField] private TileBase[] substrataTileBases;
    [SerializeField] private TileBase[] toxicTileBases;
    [SerializeField] private GameObject fishDisplay;
    [SerializeField] private GameObject fishImage;
    [SerializeField] private GameObject timeLeft;
    [SerializeField] private GameObject feedbackText;
    [SerializeField] private GameObject CNC;
    [SerializeField] private GameObject[] CoralOptions;
    [SerializeField] private TileBase toxicOverlay;
    [SerializeField] private GameObject popupCanvas;
    [SerializeField] private Sprite emptyRack;
    [SerializeField] private Sprite[] smallRack;
    [SerializeField] private Sprite[] bigRack;
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private GameObject[] plasticBags;
    [SerializeField] private Sprite gameWinWordArt;
    [SerializeField] private Sprite gameLoseWordArt;
    [SerializeField] private GameObject ccTimerImage;
    [SerializeField] private GameObject ccOverlay;
    [SerializeField] private int level;
    #pragma warning restore 0649
    #endregion
    #region Components of GameObjects
    Grid grid;
    TMPro.TextMeshProUGUI fishDisplayText;
    UnityEngine.UI.Image fishImageImage;
    TMPro.TextMeshProUGUI timeLeftText;
    TMPro.TextMeshProUGUI ccTimerText;
    ClimateChangeTimer ccTimer;
    UnityEngine.UI.Image[,] coralIndicators;
    UnityEngine.UI.Image[,] coralRackIndicators;
    TMPro.TextMeshProUGUI cncText;
    GameEnd endGameScript;
    PopupScript popupScript;
    TMPro.TextMeshProUGUI feedbackTextText;
    MenuAnimator hudOpenSidebar;
    GameObject nurseryScreen;
    private void initializeComponents() {
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        fishDisplayText = fishDisplay.GetComponent<TMPro.TextMeshProUGUI>();
        fishImageImage = fishImage.GetComponent<UnityEngine.UI.Image>();
        timeLeftText = timeLeft.GetComponent<TMPro.TextMeshProUGUI>();
        ccTimerText = ccTimerImage.transform.Find("CCTimeLeft").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        ccTimer = ccTimerImage.GetComponent<ClimateChangeTimer>();
        coralIndicators = new UnityEngine.UI.Image[6,4];
        coralRackIndicators = new UnityEngine.UI.Image[6,4];
        for (int i = 0; i < 6; i++) {
            GameObject thing = CoralOptions[i].transform.Find("CoralIndicator").gameObject;
            GameObject rack = nurseryCamera.transform.Find("NurseryCanvas/Racks")
                    .gameObject.transform.GetChild(i/3)
                    .gameObject.transform.GetChild(i%3)
                    .gameObject.transform.Find("Selection/RackPlacements").gameObject;
            for (int j = 0; j < 4; j++) {
                coralIndicators[i,j] = thing.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>();
                coralRackIndicators[i,j] = rack.transform.GetChild(j).gameObject.GetComponent<UnityEngine.UI.Image>();
            }
        }
        cncText = CNC.GetComponent<TMPro.TextMeshProUGUI>();
        endGameScript = endGameScreen.GetComponent<GameEnd>();
        popupScript = popupCanvas.GetComponent<PopupScript>();
        feedbackTextText = feedbackText.GetComponent<TMPro.TextMeshProUGUI>();
        hudOpenSidebar = cameraFollow.GetComponent<MenuAnimator>();
        nurseryScreen = nurseryCamera.transform.Find("NurseryCanvas").gameObject;
    }
    #endregion
    #region Data Structures for the Game
    private Dictionary<Vector3Int,CoralCellData> coralCells;
    private Dictionary<Vector3Int,int> substrataCells;
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
    Color progressNone = new Color(43f/255f,90f/255f,147f/255f,1f);
    Color progressZero = new Color(247f/255f,104f/255f,9f/255f,1f); // new Color(69f/255f,129f/255f,201f/255f,1f);
    Color progressIsDone = new Color(249f/255f, 237f/255f, 6f/255f, 1f); // new Color(0f,1f,17f/255f,1f); // new Color(0f,1f,176f/255f,1f);
    Color progressDefinitelyDone = new Color(0f,1f,17f/255f,1f); // new Color(1f, 200f/255f, 102f/255f, 1f);
    #endregion
    #region Global Changing Values
    private float zoom;
    private Vector3 cameraFollowPosition;
    private Vector3 savedCameraPosition;
    private bool edgeScrollingEnabled = false;
    private bool showNursery = false;
    private int selectedCoral = 0;
    private int fishIncome = 0;
    private float cfTotalProduction = 0;
    private float hfTotalProduction = 0;
    private CountdownTimer tempTimer;
    private List<NursingCoral>[] growingCorals;
    private CountdownTimer disasterTimer;
    private CountdownTimer climateChangeTimer;
    private bool climateChangeHasWarned;
    private bool climateChangeHasHappened;
    private int coralPropagationDebuff = 0;
    private int coralSurvivabilityDebuff = 0;
    private EconomyMachine economyMachine;
    private CountdownTimer timeUntilEnd;
    private CountdownTimer plasticSpawner;
    private int totalPlasticSpawned;
    private bool gameIsWon;
    private List<Vector3Int> markedToDieCoral;
    private bool timeToKillCorals;
    #endregion

    #region Generic Helper Functions
    private Vector3Int getMouseGridPosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPoint = ray.GetPoint(-ray.origin.z/ray.direction.z);
        Vector3Int position = grid.WorldToCell(worldPoint);
        return position;
    }
    private string convertTimetoMS(float rawTime) {
        int minutes = Mathf.FloorToInt(rawTime / 60f);
        int seconds = Mathf.FloorToInt(rawTime - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
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
    private bool withinBoardBounds (Vector3Int position, int bound) {
        bool ok = (Math.Abs(position.x) <= bound && Math.Abs(position.y) <= bound);
        return ok;
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
        selectedCoral = select;
    }
    private int getCoralsPerType(int type) {
        int result = 0;
        for (int i = 0; i < globalVarContainer.globals[level].maxSpacePerCoral; i++) {
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
        for (int i = 0; i < globalVarContainer.globals[level].maxSpacePerCoral; i++) {
            if (growingCorals[type][i] == null)
                continue;
            if (growingCorals[type][i].timer.isDone())
                ready += 1;
        }
        return ready;
    }
    private int getIndexOfReadyCoral(int type) {
        int index = -1;
        for (int i = 0; i < globalVarContainer.globals[level].maxSpacePerCoral; i++) {
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
        initializeComponents();
        print("loading XML data...");
        globalVarContainer = GlobalContainer.Load("GlobalsXML");
        substrataDataContainer = SubstrataDataContainer.Load("SubstrataXML");
        coralBaseData = CoralDataContainer.Load("CoralDataXML");
        algaeDataContainer = AlgaeDataContainer.Load("AlgaeDataXML");
        zoom = globalVarContainer.globals[level].zoom;
        print("XML data loaded");
        print("initializing tiles...");
        initializeTiles();
        print("initialization done");
        tempTimer = new CountdownTimer(globalVarContainer.globals[level].maxGameTime);
        disasterTimer = new CountdownTimer(30f); // make into first 5 mins immunity
        climateChangeTimer = new CountdownTimer(globalVarContainer.globals[level].timeUntilClimateChange);
        climateChangeHasWarned = false;
        climateChangeHasHappened = false;
        economyMachine = new EconomyMachine(10f,0f,5f);
        timeUntilEnd = new CountdownTimer(60f);
        plasticSpawner = new CountdownTimer(15f);
        totalPlasticSpawned = 0;
        gameIsWon = false;
        timeToKillCorals = false;
        ccTimerImage.transform.Find("CCTimeLeft").gameObject.SetActive(false);
        ccOverlay.SetActive(false);
        print("level is " + globalVarContainer.globals[level].level);
        initializeGame();
    }


    private void initializeTiles() {
        // instantiation
        coralCells = new Dictionary<Vector3Int,CoralCellData>();
        substrataCells = new Dictionary<Vector3Int, int>();
        algaeCells = new Dictionary<Vector3Int, AlgaeCellData>();
        growingCorals = new List<NursingCoral>[6];
        for (int i = 0; i < 6; i++) {
            growingCorals[i] = new List<NursingCoral>() {null, null, null, null};
        }
        markedToDieCoral = new List<Vector3Int>();

        // initialization
        // Setting the substrata data
        foreach (Vector3Int pos in substrataTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!substrataTileMap.HasTile(localPlace)) continue;
            if (!withinBoardBounds(localPlace, 30)) {
                substrataTileMap.SetTile(localPlace, null);
                continue;
            }
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
            if (!groundTileMap.HasTile(localPlace)) continue;
            if (!coralTileMap.HasTile(localPlace)) continue;
            if (!substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace) || !withinBoardBounds(localPlace, 30)) {
                coralTileMap.SetTile(localPlace, null);
                continue;
            }
            TileBase currentTB = coralTileMap.GetTile(localPlace);
            CoralCellData cell = new CoralCellData(
                localPlace, 
                coralTileMap, 
                currentTB, 
                26, 
                coralBaseData.corals[findIndexOfEntityFromName(currentTB.name)]
            );
            cfTotalProduction += cell.coralData.cfProduction;
            hfTotalProduction += cell.coralData.hfProduction;
            coralCells.Add(cell.LocalPlace, cell);
        }
        
        foreach (Vector3Int pos in algaeTileMap.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (!groundTileMap.HasTile(localPlace)) continue;
            if (!algaeTileMap.HasTile(localPlace)) continue;
            if (!substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || !withinBoardBounds(localPlace, 30)) {
                algaeTileMap.SetTile(localPlace, null);
                continue;
            }
            TileBase currentTB = algaeTileMap.GetTile(localPlace);
            AlgaeCellData cell = new AlgaeCellData(
                localPlace, 
                algaeTileMap, 
                currentTB, 
                26, 
                algaeDataContainer.algae[findIndexOfEntityFromName(currentTB.name)]
            );
            hfTotalProduction += cell.algaeData.hfProduction;
            algaeCells.Add(cell.LocalPlace,cell);
        }
    }

    private void initializeGame() {
        fishDisplayText.text = "Fish Income: 0";
        if (hfTotalProduction >= cfTotalProduction) {
            fishImageImage.color = new Color(73f/255f,196f/255f,114f/255f,1f);
        } else {
            fishImageImage.color = new Color(255f/255f,69f/255f,69f/255f,1f);
        }
        timeLeftText.text = convertTimetoMS(tempTimer.currentTime);
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
        InvokeRepeating("killCorals", 1.0f, 3.0f);
    }

    void Update()
    {
        if (GameEnd.gameHasEnded) {
            return;
        }

        // __REMOVE__
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            gameIsWon = false;
            endTheGame("force end");
        }

        if (PauseScript.GamePaused) {
            return;
        }

        updateFishData();

        plasticSpawner.updateTime();
        if (plasticSpawner.isDone()) {
            Instantiate(plasticBags[UnityEngine.Random.Range(0,plasticBags.Length)]);
            adjustTotalPlasticBags(1);
            plasticSpawner.currentTime = plasticSpawner.timeDuration;
        }

        #region Disaster Happenings
        disasterTimer.updateTime();
        if (disasterTimer.isDone()) {
            disasterTimer = new CountdownTimer(60f);
            randomDisaster();
        }
        
        if (!climateChangeTimer.isDone()) {
            climateChangeTimer.updateTime();
            ccTimerText.text = convertTimetoMS(climateChangeTimer.currentTime);
        }
        if (!climateChangeHasWarned && climateChangeTimer.currentTime <= climateChangeTimer.timeDuration*(2.0/3.0)) {
            climateChangeHasWarned = true;
            ccTimerImage.transform.Find("CCTimeLeft").gameObject.SetActive(true);
            ccTimer.climateChangeIsHappen();
            // makePopup("Climate Change is coming...", "Scientists have predicted that our carbon emmisions will lead to devastating damages to sea life in a few years! This could slow down the growth of coral reefs soon...");
            popupScript.makeEvent(0, "Climate Change is coming! Scientists have predicted that our carbon emmisions will lead to devastating damages to sea life in a few years! This could slow down the growth of coral reefs soon...");
        } else if (climateChangeHasWarned && !climateChangeHasHappened && climateChangeTimer.isDone()) {
            climateChangeHasHappened = true;
            // ccTimerImage.transform.Find("CCTimeLeft").gameObject.SetActive(false);
            // ccTimer.climateChangeIsHappen();
            ccOverlay.SetActive(true);
            // makePopup("Climate Change has come...", "Scientists have determined that the increased temperature and ocean acidity has slowed down coral growth! We have to make a greater effort to coral conservation and rehabilitation!");
            popupScript.makeEvent(0, "Climate Change has come! Scientists have determined that the increased temperature and ocean acidity has slowed down coral growth! We have to make a greater effort to coral conservation and rehabilitation!");
            applyClimateChange();
        }

        // __REMOVE__
        // test script for popup messages
        if (Input.GetKeyDown(KeyCode.Slash)) {
            // makePopup("Title", "Hello!", false);
            randomDisaster(UnityEngine.Random.Range(1,3));
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Backslash)) {
            hfTotalProduction += 10000;
        }

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

        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Z)) {
            change_coral(0);
        } else if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.X)) {
            change_coral(1);
        } else if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.C)) {
            change_coral(2);
        } else if (Input.GetKeyDown(KeyCode.F4) || Input.GetKeyDown(KeyCode.V)) {
            change_coral(3);
        } else if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.B)) {
            change_coral(4);
        } else if (Input.GetKeyDown(KeyCode.F6) || Input.GetKeyDown(KeyCode.N)) {
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
        }

        bool rb = Input.GetMouseButtonDown(1);
        if (rb) {
            if(!plantCoral(selectedCoral)) {
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
        if (Input.GetKeyDown(KeyCode.K)) {
            operateNursery();
        }

        if (!showNursery) {
            moveCameraWASD(100f);
            if (edgeScrollingEnabled) moveCameraMouseEdge(100f,10f);
            zoomKeys(5f);
            clampCamera();
        }
        #endregion
        
        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < globalVarContainer.globals[level].maxSpacePerCoral; j++) {
                Sprite currentPositionSprite = coralRackIndicators[i,j].sprite;
                if (growingCorals[i][j] == null) {
                    coralIndicators[i,j].color = progressNone;
                    if (currentPositionSprite.name != emptyRack.name)
                        coralRackIndicators[i,j].sprite = emptyRack;
                    continue;
                }
                growingCorals[i][j].timer.updateTime();
                bool currentCoralDone = growingCorals[i][j].timer.isDone();
                if (!currentCoralDone)
                    coralIndicators[i,j].color = Color.Lerp(progressZero, progressIsDone, growingCorals[i][j].timer.percentComplete);
                else if (currentCoralDone && coralIndicators[i,j].color != progressDefinitelyDone)
                    coralIndicators[i,j].color = progressDefinitelyDone;
                if (currentCoralDone && currentPositionSprite.name != bigRack[i].name) 
                    coralRackIndicators[i,j].sprite = bigRack[i];
                else if (!currentCoralDone && currentPositionSprite.name != smallRack[i].name)
                    coralRackIndicators[i,j].sprite = smallRack[i];
            }
        }

        cncText.text = getCoralsInNursery() + "/" + globalVarContainer.globals[level].maxSpaceInNursery + " SLOTS LEFT";

        tempTimer.updateTime();
        timeLeftText.text = convertTimetoMS(tempTimer.currentTime);
        if (tempTimer.isDone()) {
            endTheGame("The reef could not recover...");
        }

        if (fishIncome >= globalVarContainer.globals[level].goal) {
            timeUntilEnd.updateTime();
        } else {
            timeUntilEnd.reset();
        }

        if (timeUntilEnd.isDone()) {
            gameIsWon = true;
            endTheGame("You have recovered the reef!");
        }
    }
    public void adjustTotalPlasticBags(int x) {
        totalPlasticSpawned += x;
    }
    private void endTheGame(string s) {
        endGameScript.finalStatistics(fishIncome, convertTimetoMS(tempTimer.currentTime));
        endGameScript.setCongrats((gameIsWon ? gameWinWordArt : gameLoseWordArt));
        endGameScript.endMessage(s);
        endGameScript.gameEndReached();
    }
    
    public void operateNursery() {
        showNursery = !showNursery;
        if (showNursery) {
            savedCameraPosition = cameraFollow.transform.position;
            cameraFollow.enabled = false;
            nurseryCamera.enabled = true;
            nurseryScreen.SetActive(true);
        } else {
            cameraFollow.enabled = true;
            nurseryScreen.SetActive(false);
            nurseryCamera.enabled = false;
            cameraFollowPosition = savedCameraPosition;
            cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
        }
    }

    private void feedbackDialogue(string text, float time) {
        StartCoroutine(ShowMessage(text,time));
    }

    IEnumerator ShowMessage(string text, float time) {
        feedbackTextText.text = text;
        feedbackTextText.enabled = true;
        yield return new WaitForSeconds(time);
        feedbackTextText.enabled = false;
    }

    public void openThing() {
        hudOpenSidebar.OpenThing();
    }

    private void updateFishData() {        
        updateFishOutput();

        fishDisplayText.text = "Fish Income: " + fishIncome;
        if (hfTotalProduction >= cfTotalProduction) {
            fishImageImage.color = new Color(73f/255f,196f/255f,114f/255f,1f);
        } else {
            fishImageImage.color = new Color(255f/255f,69f/255f,69f/255f,1f);
        }
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
            if (algaeCells[key].maturity <= 25) {
                algaeCells[key].addMaturity(1);
            }
            HashSet<Vector3Int> coralsAround = spread(key, 1);
            int weightedCoralMaturity = 0;
            foreach(Vector3Int pos in coralsAround) {
                if (coralCells.ContainsKey(pos)) {
                    weightedCoralMaturity += coralCells[pos].maturity;
                }
            }
            if (!economyMachine.algaeWillSurvive(algaeCells[key], substrataCells[key], -3*weightedCoralMaturity/2+coralSurvivabilityDebuff)) {
                algaeTileMap.SetTile(key, null);
                algaeCells.Remove(key);
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
            if (algaeCells[key].maturity > 25) { // propagate only if "mature"
                for (int i = 0; i < 6; i++) {
                    if (economyMachine.algaeWillPropagate(algaeCells[key], coralPropagationDebuff, groundTileMap.GetTile(key).name)) {
                        Vector3Int localPlace = key+hexNeighbors[key.y&1,i];
                        if (!groundTileMap.HasTile(localPlace)) continue;
                        if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace)) continue;
                        if (substrataOverlayTileMap.HasTile(localPlace)) continue;
                        if (!withinBoardBounds(localPlace, 30)) continue;
                        if (algaeTileMap.HasTile(localPlace) || algaeCells.ContainsKey(localPlace)) continue;
                        // __ECONOMY__ __FIX__ MANUAL OVERRIDE TO CHECK IF ALGAE CAN TAKE OVER
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace)) {
                            int randNum = UnityEngine.Random.Range(0, 101);
                            HashSet<Vector3Int> surrounding = spread(localPlace, 1);
                            CoralCellData temp;
                            foreach(Vector3Int tempLocation in surrounding) {
                                if (coralCells.TryGetValue(tempLocation, out temp)) {
                                    randNum -= coralCells[tempLocation].maturity/3;
                                }
                            }
                            if (randNum < 60) continue;
                        }
                        // adding algae
                        AlgaeCellData cell = new AlgaeCellData(
                            localPlace, 
                            algaeTileMap, 
                            algaeCells[key].TileBase, 
                            0, 
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
        bool spaceInNursery = getCoralsInNursery() < globalVarContainer.globals[level].maxSpaceInNursery;
        bool underMaxGrow = getCoralsPerType(type) < globalVarContainer.globals[level].maxSpacePerCoral;
        if (spaceInNursery && underMaxGrow) {
            int nullIdx = -1;
            for (int i = 0; i < globalVarContainer.globals[level].maxSpacePerCoral; i++) {
                if (growingCorals[type][i] == null)
                    nullIdx = i;
            }
            if (nullIdx == -1) {
                print("ERROR: no null spot found");
            } else {
                growingCorals[type][nullIdx] = new NursingCoral(coralBaseData.corals[type].name, new CountdownTimer(coralBaseData.corals[type].growTime));
            }
        } else if (!underMaxGrow) {
            feedbackDialogue("Can only grow 4 corals per type.", globalVarContainer.globals[level].feedbackDelayTime);
        } else if (!spaceInNursery) {
            feedbackDialogue("Nursery is at maximum capacity.", globalVarContainer.globals[level].feedbackDelayTime);
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
        if (!withinBoardBounds(position, 30)) {
            feedbackDialogue("Can't put corals out of bounds!", globalVarContainer.globals[level].feedbackDelayTime);
        } else if (coralTileMap.HasTile(position)) {
            feedbackDialogue("Can't put corals on top of other corals!.", globalVarContainer.globals[level].feedbackDelayTime);
        } else if (algaeTileMap.HasTile(position)) {
            feedbackDialogue("Can't put corals on top of algae! The coral will die!", globalVarContainer.globals[level].feedbackDelayTime);
        } else if (substrataOverlayTileMap.HasTile(position)) {
            feedbackDialogue("This is a toxic tile! Corals won't survive here.", globalVarContainer.globals[level].feedbackDelayTime);
        } else if ((substrataTileMap.HasTile(position) || substrataCells.ContainsKey(position)) && readyNum > 0) { 
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
                0, 
                coralBaseData.corals[type]
            );
            coralCells.Add(position, cell);
            cfTotalProduction += coralCells[position].coralData.cfProduction;
            hfTotalProduction += coralCells[position].coralData.hfProduction;
            coralTileMap.SetTile(position, coralTileBases[type]);
        } else if (readyNum == 0 && loadedNum-readyNum > 0) {
            float minTime = 3600f;
            for (int i = 0; i < globalVarContainer.globals[level].maxSpacePerCoral; i++) {
                if (growingCorals[type][i] == null)
                    continue;
                minTime = Math.Min(minTime, growingCorals[type][i].timer.currentTime);
            }
            string t = "Soonest to mature coral of this type has " + convertTimetoMS(minTime) + " time left.";
            feedbackDialogue(t, globalVarContainer.globals[level].feedbackDelayTime);
        }

        return successful;
    }
    private void updateCoralSurvivability() {
        List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
        foreach (Vector3Int key in keys) {
            if (coralCells[key].maturity <= 25) {
                // check adj corals
                // miscFactors aka the amount of corals around it influences how much more they can add to the survivability of one
                // how much they actually contribue can be varied; change the amount 0.01f to something that makes more sense
                int miscFactors = 0;
                // __FIX__ MAYBE USE SPREAD?
                for (int i = 0; i < 6; i++)
                    if (coralCells.ContainsKey(key+hexNeighbors[key.y&1, i]))
                        miscFactors += coralCells[key+hexNeighbors[key.y&1, i]].maturity/5; 
                coralCells[key].addMaturity(1);
                if (!economyMachine.coralWillSurvive(coralCells[key], substrataCells[key], miscFactors-coralSurvivabilityDebuff, groundTileMap.GetTile(key).name)) {
                    // setting data
                    coralTileMap.SetTile(key, coralDeadTileBases[findIndexOfEntityFromName(coralCells[key].TileBase.name)]);
                    markedToDieCoral.Add(key);
                }
            }
        }
    }

    private void killCorals() {
        timeToKillCorals = !timeToKillCorals;
        if (!timeToKillCorals)
            return;
        foreach(Vector3Int key in markedToDieCoral) {
            if (!coralCells.ContainsKey(key))
                continue;
            coralTileMap.SetTile(key, null);
            hfTotalProduction -= coralCells[key].coralData.hfProduction;
            cfTotalProduction -= coralCells[key].coralData.cfProduction;
            coralCells.Remove(key);
        }
        markedToDieCoral.Clear();
    }

    private void updateCoralPropagation() {
        List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
        foreach (Vector3Int key in keys) {
            if (coralCells[key].maturity > 25) { // propagate only if "mature"
                for (int i = 0; i < 6; i++) {
                    if (economyMachine.coralWillPropagate(coralCells[key], -coralPropagationDebuff, groundTileMap.GetTile(key).name)) {
                        Vector3Int localPlace = key+hexNeighbors[key.y&1,i];
                        if (!groundTileMap.HasTile(localPlace)) continue;
                        if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace)) continue;
                        if (!withinBoardBounds(localPlace, 30)) continue;
                        if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || algaeTileMap.HasTile(localPlace)) continue;
                        CoralCellData cell = new CoralCellData(
                            localPlace, 
                            coralTileMap, 
                            coralCells[key].TileBase, 
                            0, 
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
        // chance: 1/50
        // random: random area selection
        // toxic: center must be a coral
        int t = UnityEngine.Random.Range(1,51);
        if (t == 21 || forceEvent == 1) {
            if (coralCells.Count > 15) {
                Vector3Int pos = coralCells.ElementAt(UnityEngine.Random.Range(0,coralCells.Count)).Key;
                HashSet<Vector3Int> removeSpread = spread(pos, 2);
                foreach (Vector3Int removePos in removeSpread) {
                    if (coralCells.ContainsKey(removePos)) {
                        markedToDieCoral.Add(removePos); // __TIMING__
                        coralTileMap.SetTile(removePos, coralDeadTileBases[findIndexOfEntityFromName(coralCells[removePos].TileBase.name)]);
                    }
                }
                popupScript.makeEvent(UnityEngine.Random.Range(2,4));
            }
        } else if (t == 42 || forceEvent == 2) {
            Vector3Int pos = new Vector3Int(UnityEngine.Random.Range(-30,31), UnityEngine.Random.Range(-30,31), 0);
            if (substrataCells.ContainsKey(pos))
                substrataCells.Remove(pos);
            substrataTileMap.SetTile(pos, toxicTileBases[UnityEngine.Random.Range(0,toxicTileBases.Length)]);
            HashSet<Vector3Int> toxicSpread = spread(pos, 2);
            foreach (Vector3Int toxicPos in toxicSpread) {
                if (coralCells.ContainsKey(toxicPos)) {
                    markedToDieCoral.Add(toxicPos); // __TIMING__
                    coralTileMap.SetTile(toxicPos, coralDeadTileBases[findIndexOfEntityFromName(coralCells[toxicPos].TileBase.name)]);
                }
                if (algaeCells.ContainsKey(toxicPos)) {
                    algaeCells.Remove(toxicPos);
                    algaeTileMap.SetTile(toxicPos, null);
                }
                substrataOverlayTileMap.SetTile(toxicPos,toxicOverlay);
            }
            popupScript.makeEvent(1);
        }
    }
    // __ECONOMY__
    private void applyClimateChange() {
        // scripted event
        // reduce the growth rates globally
        coralPropagationDebuff = 5;
        coralSurvivabilityDebuff = 5;
    }
    #endregion

    // __ECONOMY__
    #region Misc Updates
    private void updateFishOutput() {
        economyMachine.updateHFCF(hfTotalProduction, cfTotalProduction);
        float hf = economyMachine.getActualHF();
        float cf = economyMachine.getActualCF();
        fishIncome = (int)Math.Round(economyMachine.getTotalFish())-totalPlasticSpawned/3;
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
        if (Input.GetKey(KeyCode.Q)) {
            zoom -= zoomChangeAmount * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E)) {
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
            Mathf.Clamp(cameraFollowPosition.x, -90.0f, 90.0f),
            Mathf.Clamp(cameraFollowPosition.y, -150.0f, 150.0f),
            cameraFollowPosition.z
        );
    }
    #endregion
}

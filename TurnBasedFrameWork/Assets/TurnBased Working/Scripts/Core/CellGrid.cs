using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// CellGrid class keeps track of the game, stores cells, units and players objects. It starts the game and makes turn transitions. 
/// It reacts to user interacting with units or cells, and raises events related to game progress. 
/// </summary>
public class CellGrid : MonoBehaviour
{
    /// <summary>
    /// LevelLoading event is invoked before Initialize method is run.
    /// </summary>
    public event EventHandler LevelLoading;
    /// <summary>
    /// LevelLoadingDone event is invoked after Initialize method has finished running.
    /// </summary>
    public event EventHandler LevelLoadingDone;
    /// <summary>
    /// GameStarted event is invoked at the beggining of StartGame method.
    /// </summary>
    public event EventHandler GameStarted;
    /// <summary>
    /// GameEnded event is invoked when there is a single player left in the game.
    /// </summary>
    public event EventHandler GameEnded;
    /// <summary>
    /// Turn ended event is invoked at the end of each turn.
    /// </summary>
    public event EventHandler TurnEnded;

    /// <summary>
    /// UnitAdded event is invoked each time AddUnit method is called.
    /// </summary>
    public event EventHandler<UnitCreatedEventArgs> UnitAdded;




    private CellGridState _cellGridState; //The grid delegates some of its behaviours to cellGridState object.
    public CellGridState CellGridState
    {
        private get
        {
            return _cellGridState;
        }
        set
        {
            if(_cellGridState != null)
                _cellGridState.OnStateExit();
            _cellGridState = value;
            _cellGridState.OnStateEnter();
        }
    }

    public ICellGridGenerator _cellGridGenerator;//CJG 11/24/2018


    public int NumberOfPlayers { get; private set; }

    public Player CurrentPlayer
    {
        get { return Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)); }
    }
    public int CurrentPlayerNumber { get; private set; }

    /// <summary>
    /// GameObject that holds player objects.
    /// </summary>
    public Transform PlayersParent;

    public CustomObstacleGenerator obstacleGenerator;// to come from the inspector
    public CustomObjectiveGenerator objectiveGenerator;// to come from the inspector
    public TurnTracker turnTracker;// to come from the inspector
    public RunTimeAIUnitGenerator AIUnitsGenerator;// to come from the inspector

    public List<Player> Players { get; private set; }
    public List<Cell> Cells { get; private set; }
    public List<Unit> Units { get; private set; }

    public static int counter = 120;

    SpawnManager spawnManager;
    bool FriendUnitsSpawnedIn = false;
    private void Awake()
    {
        spawnManager = SpawnManager.instance;

    }
    private void Start()
    {
        spawnManager = SpawnManager.instance;
        spawnManager.SpawnFriendUnits += MoveFriendUnitsAsDeclaredBySpawnManager;
        if (LevelLoading != null)
            LevelLoading.Invoke(this, new EventArgs());

        Initialize();
       // TurnOffFriendUnitsMeshRenderer();
       // StartCoroutine(EnableFriendUnitsMeshRenderers(10f));
    }

    void ClearAndCreateMap()
    {
        //START CJG
        //Clear and Generate Grid at run time, added this in addtion to the generator working in the inspector.
        var _cellsToDelete = new List<GameObject>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            var cell = this.gameObject.transform.GetChild(i);
            if (cell != null)
            {
                // Debug.Log("When Clearing Grid in the Initialize function of CellGrid, the cell in the foreach statement is not null");
                _cellsToDelete.Add(cell.gameObject);
            }
            else
            {
                Debug.Log("Cells where null, no grid detected");
            }
        }
        _cellsToDelete.ForEach(c => DestroyImmediate(c));

        _cellGridGenerator.GenerateGrid();

        Cells = new List<Cell>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var cell = transform.GetChild(i).gameObject.GetComponent<Cell>();
            if (cell != null)
                Cells.Add(cell);
            else
                Debug.LogError("Invalid object in cells paretn game object");
        }

        foreach (var cell in Cells)
        {
            cell.CellClicked += OnCellClicked;
            cell.CellHighlighted += OnCellHighlighted;
            cell.CellDehighlighted += OnCellDehighlighted;
            cell.GetComponent<Cell>().GetNeighbours(Cells);
        }
        //END CJG

    }

    void GenerateObstacles()
    {
        // Spawn Obstacles need to make the locations a function of the Map Data base handler.
        obstacleGenerator.RandomSpawnObstacles(Cells);
    }

    private void Initialize()
    {
        // ExecuteMapDatabase();

        ClearAndCreateMap();

        GenerateObstacles();

        //GenerateObjectives();

        GetPlayersScripts();

        // IUnitGenerator (UnitGeneratorTest.cs) Spawn starting enemies, once finished invoke Starting enemy units spawned event. this will serve to enable the starting location selector script.
        // Enable the starting location selection script
        // Spawns are confirmed disable the starting location script after invoking event handler that IUnitGenerator is subscribed to in oder to spawn in the Friend units (seperate function).

        GenerateUnits();

        if (GameStarted != null)
            GameStarted.Invoke(this, new EventArgs());
        if (LevelLoadingDone != null)
            LevelLoadingDone.Invoke(this, new EventArgs());
        StartMap();
        // Enemy units are spawned in 
        //                            these are done from GenerateUnits function
        // Friend units are spawned in
        // Need to:
        // 1. Disable the Friend units mesh renderer => Done
         TurnOffFriendUnitsMeshRenderer();

        // 2. Highlight selectable cells to spawn in, this should depend upon maps basis
         spawnManager.HighlightSpawnableCells(Cells); //TODO bug with UnMark cell after OnMouseExit Unmark cell.

        // 3.From Spawn Manager ActivateSelectSpawnLocations
        spawnManager.ActivateSelectSpawnLocations(Cells);
        // 4.Fill up the spawn locations list, user input

        // 5.Once the spawn location list is confirmed, DeActivateSelectSpawnLocations

        // 6.Invoke Ready to spawn Event hanlder,

        // 7.Spawn friend units and enable mesh renderers
        // 
    }

    private void GenerateUnits()
    {
        var unitGenerator = GetComponent<UnitGeneratorTest>();
        if (unitGenerator != null)
        {
            Units = unitGenerator.SpawnUnits(Cells);
            foreach (var unit in Units)
            {
                AddUnit(unit.GetComponent<Transform>());
                // unit.GetComponent<MeshRenderer>().enabled = false; 

            }
        }
        else
            Debug.LogError("No IUnitGenerator script attached to cell grid");
    }

    private void TurnOffFriendUnitsMeshRenderer()
    {
        foreach (var unit in Units)
        {
            if(unit.transform.tag == "Friend")
               unit.GetComponent<MeshRenderer>().enabled = false; 

        }
    }

    private void TurnOnFriendUnitsMeshRenderer()
    {
        foreach (var unit in Units)
        {
            if (unit.transform.tag == "Friend")
                unit.GetComponent<MeshRenderer>().enabled = enabled;

        }
    }

    private void GenerateObjectives()
    {
        throw new NotImplementedException();
    }

    private void ExecuteMapDatabase()
    {
        throw new NotImplementedException();
    }

    private void GetPlayersScripts()
    {
        Players = new List<Player>();
        for (int i = 0; i < PlayersParent.childCount; i++)
        {
            var player = PlayersParent.GetChild(i).GetComponent<Player>();
            if (player != null)
                Players.Add(player);
            else
                Debug.LogError("Invalid object in Players Parent game object");
        }
        NumberOfPlayers = Players.Count;
        CurrentPlayerNumber = Players.Min(p => p.PlayerNumber);
    }

    private IEnumerator EnableFriendUnitsMeshRenderers(float v)
    {
        yield return new WaitForSeconds(v);
        foreach (var unit in Units)
        {
            unit.GetComponent<MeshRenderer>().enabled = true;
        }

    }

    private void OnCellDehighlighted(object sender, EventArgs e)
    {
        if (FriendUnitsSpawnedIn)
        {
            CellGridState.OnCellDeselected(sender as Cell);
        }
        else
        {
            bool spawnableCell = spawnManager.spawnableCells.Contains(sender as Cell);// is the dehighlighted cell part of the spawnable cells list?
            if (!spawnableCell)
            {
                CellGridState.OnCellDeselected(sender as Cell);
            }
            else
            {
                var _cell = sender as SampleSquare;
                _cell.MarkAsSpawnableCell();
            }
        }         
    }
    private void OnCellHighlighted(object sender, EventArgs e)
    {
        CellGridState.OnCellSelected(sender as Cell);
    } 
    private void OnCellClicked(object sender, EventArgs e)
    {
        CellGridState.OnCellClicked(sender as Cell);
    }

    private void OnUnitClicked(object sender, EventArgs e)
    {
        CellGridState.OnUnitClicked(sender as Unit);
    }
    private void OnUnitDestroyed(object sender, AttackEventArgs e)
    {
        Units.Remove(sender as Unit);
        var totalPlayersAlive = Units.Select(u => u.PlayerNumber).Distinct().ToList(); //Checking if the game is over
        if (totalPlayersAlive.Count == 1)
        {
            if(GameEnded != null)
                GameEnded.Invoke(this, new EventArgs());
        }
    }

    /// <summary>
    /// Adds unit to the game
    /// </summary>
    /// <param name="unit">Unit to add</param>
    public void AddUnit(Transform unit)
    {
        unit.GetComponent<Unit>().UnitClicked += OnUnitClicked;
        unit.GetComponent<Unit>().UnitDestroyed += OnUnitDestroyed;

        if(UnitAdded != null)
            UnitAdded.Invoke(this, new UnitCreatedEventArgs(unit)); 
    }

    /// <summary>
    /// Method is called once, at the beggining of the game.
    /// </summary>
    public void StartMap()
    {
        Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnStart(); });
        Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);
    }
    /// <summary>
    /// Method makes turn transitions. It is called by player at the end of his turn.
    /// </summary>
    public void EndTurn()
    {
        int TurnBeforeSpawingObjective = 2; // Should be get from a random thing. Remember it starts from 0 in CellGrid
        int TurnToSpawnObjective = 3;// Should be get from a random thing. Remember it starts from 0 in CellGrid
        int TurnToSpawnNewUnit = 6;
		int NumberOfTurnsForThisMap = 10;
        if (turnTracker.CurrentTurn == TurnBeforeSpawingObjective)
        {
            Debug.Log("Objective coming in the next turn!. Be ready");
        }
        if(turnTracker.CurrentTurn == TurnToSpawnObjective)
        {
            Debug.Log("CellGrid:EndTurn Creating an objective on the objectives parent");
            AddObjectiveAtRandomPosition();// Random Seed is inside this function
        }

        if(turnTracker.CurrentTurn == TurnToSpawnNewUnit)
        {
            AddAIUnitsAtRunTime();// default parameter of 2, // Random Seed is inside this function
        }
        if (Units.Select(u => u.PlayerNumber).Distinct().Count() == 1)
        {
            return;
        }
        CellGridState = new CellGridStateTurnChanging(this);

        Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnEnd(); });

        CurrentPlayerNumber = (CurrentPlayerNumber + 1) % NumberOfPlayers;
        while (Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).Count == 0)
        {
            CurrentPlayerNumber = (CurrentPlayerNumber + 1)%NumberOfPlayers;
        }//Skipping players that are defeated.

        if (TurnEnded != null)
            TurnEnded.Invoke(this, new EventArgs());

        Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnStart(); });
        Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);     
    }

    public void AddAIUnitsAtRunTime(int unitsToAdd = 2)//Will add the numbers of units determined by the AIUnitsToGenerate public int variable.
    {

        if (AIUnitsGenerator != null)
        {
            AIUnitsGenerator.AIUnitsToGenerate = unitsToAdd;
            AIUnitsGenerator.seed = new System.Random().Next();
            List<Unit> NewAIUnitsAtRunTime = AIUnitsGenerator.SpawnUnits(Cells);
            foreach (var unit in NewAIUnitsAtRunTime)
            {
                Units.Add(unit as Unit);//part of the CellGrid
                AddUnit(unit.GetComponent<Transform>());// func call
            }
        }
        else
            Debug.LogError("No AIUnitsGenerator script attached to cell grid");
    }

    public void AddObjectiveAtRandomPosition()
    {
        objectiveGenerator.seed = new System.Random().Next();
        objectiveGenerator.RandomSpawnObjective(Cells);
    }

    public int FriendUnitCounter = 0;
    public void MoveFriendUnitsAsDeclaredBySpawnManager(object sender, EventArgs e)
    {
        FriendUnitsSpawnedIn = true;
        var _spawnManager = sender as SpawnManager;
        var spawnInCells = _spawnManager._spawnInCells;
        var spawnableCells = _spawnManager.spawnableCells;
        _spawnManager.DeActivateSelectSpawnLocations(Cells);

        foreach (Cell c in spawnableCells)
        {
            c.UnMark();
        }

        int i = 0;
        Stack<Unit> FriendUnits = new Stack<Unit>(3);
        foreach(Unit _fu in Units)
        {
            if(_fu.transform.tag == "Friend")
            {
                FriendUnits.Push(_fu);
            }
        }
        FriendUnitCounter = FriendUnits.Count;
        for (i = 0; i <= spawnInCells.Count-1; i++)
        {
            Debug.Log(i);
            var _cell = spawnInCells.ElementAt(i);
            var _unit = FriendUnits.Pop();
            Debug.Log("Moving unit: " + _unit.name + " to " + _cell.transform.position);
            _unit.Cell.IsTaken = false;
            _unit.transform.position = _cell.transform.position + new Vector3(0,1,0);
            _unit.Cell = _cell;
            _unit.Cell.IsTaken = true;
        }
        // for loop above spawns in the units

        TurnOnFriendUnitsMeshRenderer();
    }
}

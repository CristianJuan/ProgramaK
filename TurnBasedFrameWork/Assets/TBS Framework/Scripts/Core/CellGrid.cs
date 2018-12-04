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
    private void Start()
    {
        StartCoroutine(FuelNotification());

        if (LevelLoading != null)
            LevelLoading.Invoke(this, new EventArgs());

        Initialize();

        if (LevelLoadingDone != null)
            LevelLoadingDone.Invoke(this, new EventArgs());

        StartGame();
    }

    IEnumerator FuelNotification()
    {
        Debug.Log("Waiting for tank to get empty");
        yield return new WaitUntil(() => counter <= 0);
        Debug.Log("Tank Empty!"); //Notification Would need the rest of the Initialize source code called from a new function. Such as to
        // WaitUntil the user accepts its units on the map.
    }

    void Update()
    {
        if (counter > 0)
        {
            //Debug.Log("Fuel Level: " + counter);
            counter--;
        }
    }

    private void Initialize()
    {
        //START CJG
        //Clear and Generate Grid at run time, added this in addtion to the generator working in the inspector.
        var _cellsToDelete = new List<GameObject>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            var cell = this.gameObject.transform.GetChild(i);
            if(cell != null)
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

        //END CJG

        //START CJG
        // Spawn Obstacles
        obstacleGenerator.RandomSpawnObstacles(Cells);

        //END CJG


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


        foreach (var cell in Cells)
        {
            cell.CellClicked += OnCellClicked;
            cell.CellHighlighted += OnCellHighlighted;
            cell.CellDehighlighted += OnCellDehighlighted;
            cell.GetComponent<Cell>().GetNeighbours(Cells);
        }

        var unitGenerator = GetComponent<IUnitGenerator>();
        if (unitGenerator != null)
        {
            Units = unitGenerator.SpawnUnits(Cells);
            foreach (var unit in Units)
            {
                AddUnit(unit.GetComponent<Transform>());
                unit.GetComponent<MeshRenderer>().enabled = false;

            }
        }
        else
            Debug.LogError("No IUnitGenerator script attached to cell grid");

        StartCoroutine(EnableFriendUnitsMeshRenderers(0.5f));

    }

    private IEnumerator EnableFriendUnitsMeshRenderers(float v)
    {
        yield return new WaitForSeconds(v);
        Vector3 spawnInOffset = new Vector3(1, 0, 0);
        foreach (var unit in Units)
        {
            unit.transform.position = unit.transform.position + spawnInOffset;
        }
        foreach (var unit in Units)
        {
            unit.GetComponent<MeshRenderer>().enabled = true;
        }

    }

    private void OnCellDehighlighted(object sender, EventArgs e)
    {
        CellGridState.OnCellDeselected(sender as Cell);
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
    public void StartGame()
    {
        if(GameStarted != null)
            GameStarted.Invoke(this, new EventArgs());
  
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
}

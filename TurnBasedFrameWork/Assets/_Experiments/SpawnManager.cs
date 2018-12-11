using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
#region Singleton
    public static SpawnManager instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

    }
#endregion

    Stack<Cell> spawnInCellLocations;
    List<Vector3> CellPositionVectors;

    bool FriendlyUnitsSpawnedIn = false;// to be set as true when CellGrid invokes a EventHandler indicating that friend units are spawned in.

    private void Start()
    {
        spawnInCellLocations = new Stack<Cell>(3);
        CellPositionVectors = new List<Vector3>(3);
    }

    public void ActivateSelectSpawnLocations(List<Cell> _CellGridCellsList)
    {
        foreach(Cell cell in _CellGridCellsList)
        {
            cell.CellClicked += OnCellClicked;
        }

    }

    public void DeActivateSelectSpawnLocations(List<Cell> _CellGridCellsList)
    {
        foreach (Cell cell in _CellGridCellsList)
        {
            cell.CellClicked -= OnCellClicked;
        }

    }

    private void OnCellClicked(object sender, EventArgs e)
    {
        if(!FriendlyUnitsSpawnedIn)// add condition necessary to track a GameState from a GameStateManger singleton.
        {
            var cell = sender as Cell;
            bool ClikedCellisOkToSpawnIn = ProcessSelectedCellForSpawnAvailablity(cell);// TODO would a list filled at Awake work better?
            if (ClikedCellisOkToSpawnIn)
                AddSpawnLocationCell(cell);
            else
                Debug.Log("Not a permited cell to spawn in.");
        }

    }

    bool ProcessSelectedCellForSpawnAvailablity(Cell _cell)
    {
        if (_cell.transform.position.z >= 8 && !_cell.HasObjective && !_cell.IsTaken)
        {
            Debug.Log("Saw a available Cell to select for spawn");
            return true;
        }
        else
        {
            Debug.Log("Selected Cell is not avaible for spawining in");
            return false;

        }
    }

    public void HighlightSpawnableCells(List<Cell> _CellGridCellsList)
    {
        foreach(Cell c in _CellGridCellsList)
        {
            if(ProcessSelectedCellForSpawnAvailablity(c))
            {
                c.MarkAsReachable();
            }
        }
    }

    public void DeHighlightSpawnableCells(List<Cell> _CellGridCellsList)
    {
        foreach (Cell c in _CellGridCellsList)
        {
            if (ProcessSelectedCellForSpawnAvailablity(c))
            {
                c.UnMark();
            }
        }
    }


    private void AddSpawnLocationCell(Cell cell)
    {
        if(!spawnInCellLocations.Contains(cell))
        {
            if(spawnInCellLocations.Count < 3)
            {
                spawnInCellLocations.Push(cell);
                CellPositionVectors.Add(cell.transform.position);
            }
                
            else
            {
                Debug.Log("spawnInLocations is full");
            }
        }
        else
        {
            Debug.Log("Position is taken pick another one");
        }

    }

    Cell DeleteSpawnLocation()
    {
        if (spawnInCellLocations.Count > 0)
        {
            var c = spawnInCellLocations.Pop();
            CellPositionVectors.Remove(c.transform.position);
            return c;
        }
        else return null;

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.B))
        {
            Debug.Log("Pressed the B key, want to delete spawn location");
            var CellSpawnLocationDeleted = DeleteSpawnLocation();
            if (CellSpawnLocationDeleted != null)
            {
                //Tell some UI that the last selected spawn in location was deleted
            }
        }
        // Write logic to Invoke Ready to spawn Event hanlder.
        // 
    }

    // 
}

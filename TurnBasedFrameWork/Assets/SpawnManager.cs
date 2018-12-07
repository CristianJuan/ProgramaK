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

    public void InitializeSpawnManager(List<Cell> _CellGridCellsList)
    {
        foreach(Cell cell in _CellGridCellsList)
        {
            cell.CellClicked += OnCellClicked;
        }

    }
  
    private void OnCellClicked(object sender, EventArgs e)
    {
        if(!FriendlyUnitsSpawnedIn)// add condition necessary to track a GameState from a GameStateManger singleton.
        {
            var cell = sender as Cell;
            AddSpawnLocationCell(cell);
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
    }
}

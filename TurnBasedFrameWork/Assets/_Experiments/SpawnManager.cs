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
    public event EventHandler SpawnFriendUnits;// Correct way would be to use this with an UI instead of CellGrid. The UI will tell 
    // CellGrid to do its thing.

    public List<Cell> _spawnInCells;
    public List<Cell> spawnableCells;

    bool FriendlyUnitsSpawnedIn = false;// to be set as true when CellGrid invokes a EventHandler indicating that friend units are spawned in.

    private void Start()
    {
        _spawnInCells = new List<Cell>(3);
        List<Cell> spawnableCells = new List<Cell>(128);
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
                var _c = (SampleSquare)c;
                _c.MarkAsSpawnableCell();
                spawnableCells.Add(c);
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
        if(!_spawnInCells.Contains(cell))
        {
            if(_spawnInCells.Count < 3)
            {
                _spawnInCells.Add(cell);
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
        if (_spawnInCells.Count > 0)
        {
            var c = _spawnInCells[_spawnInCells.Count];
            _spawnInCells.RemoveAt(_spawnInCells.Count - 1);
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
        if(Input.GetKey(KeyCode.Y))
        {
            if(!FriendlyUnitsSpawnedIn && _spawnInCells.Count == 3)
            {
                FriendlyUnitsSpawnedIn = true;

                Debug.Log("Tell some UI that units are able to spawn in, activate UI button to do this");
                if (SpawnFriendUnits != null)
                {
                    SpawnFriendUnits.Invoke(this, new EventArgs());// Correct way is UI do this. Spawn manager
                                                                   // just tells the UI to activate the button to spawn
                    Debug.Log("SpawnManager: spawnInCells contains cells:");
                    foreach(Cell c in _spawnInCells)
                    {
                        Debug.Log(c.transform.position);
                    }
                
                }



            }
        }
    }

    // 
}

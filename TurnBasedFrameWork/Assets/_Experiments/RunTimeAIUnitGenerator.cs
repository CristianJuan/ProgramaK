using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class RunTimeAIUnitGenerator : MonoBehaviour {

    public Transform RunTimeAIUnitsParent;
    public GameObject RunTimeAIUnitsPrefab;
    public static int seed = 10;
    public CellGrid CellGrid;// for inspector snap to grid
    private System.Random _rnd = new System.Random(seed);
    public int AIUnitsToGenerate = 3;

    Queue<Cell> shuffledCells;

    public List<Unit> SpawnUnits(List<Cell> cellsFromCellGridObj)
    {
        Debug.Log("Called SpawnUnits of the RunTimeAIUnitGenerator");
        List<Unit> ret = new List<Unit>();
        List<Cell> freeCells = cellsFromCellGridObj.FindAll(h => h.GetComponent<Cell>().IsTaken == false);
        freeCells = freeCells.OrderBy(h => _rnd.Next()).ToList();
        shuffledCells = new Queue<Cell>(Utility.ShuffleArray(freeCells.ToArray(), seed));// This is a garantee that it will never try to intantiate a unit if the cell is taken.
        CreateUnitsInRunTimeAIUnitsParent();// fills up the RunTimeAIUnitsParent with RunTimeAIUnitsPrefab prefabs, these prefabs should contain the SampleUnit Script.

        for (int i = 0; i < RunTimeAIUnitsParent.childCount; i++)
        {
            var AIUnit = RunTimeAIUnitsParent.GetChild(i);// I need a function to create the units.
            var cell = cellsFromCellGridObj.OrderBy(h => Math.Abs((h.transform.position - AIUnit.transform.position).magnitude)).First();
            if (!cell.IsTaken)
            {
                cell.IsTaken = true;
                var bounds = getBounds(AIUnit);
                Vector3 offset = new Vector3(0, bounds.y-1f, 0);
                AIUnit.transform.position = cell.transform.position + offset;
                AIUnit.GetComponent<Unit>().PlayerNumber = 1;// 1 means the AI, in the example means the player with player number set to 1 from the Players Parent Transform.
                AIUnit.GetComponent<Unit>().Cell = cell.GetComponent<Cell>();
                AIUnit.GetComponent<Unit>().Initialize();
                ret.Add(AIUnit.GetComponent<Unit>() as Unit);// if instantiated prefab does not contain the SampleUnit Script this will fail.
            }
            else
            {
                Debug.Log("When spawing an objective found a cell which is taken");
                Destroy(AIUnit.gameObject);
            }

        }
        return ret;
    }

    public void CreateUnitsInRunTimeAIUnitsParent()
    {
        for (int i = 0; i < AIUnitsToGenerate; i++)
        {
            Vector3 randomCoord = GetRandomCellPosition();
            Transform newObjective = Instantiate(RunTimeAIUnitsPrefab.transform, randomCoord, Quaternion.identity) as Transform;
            newObjective.parent = RunTimeAIUnitsParent;
        }

    }

    public Vector3 GetRandomCellPosition()
    {
        Cell randomCell = shuffledCells.Dequeue();
        while (randomCell.IsTaken)
        {
            shuffledCells.Enqueue(randomCell);
            randomCell = shuffledCells.Dequeue();
        }
        // Out of while therefore, have a random cell that is not taken.
        Vector3 randomCoord = randomCell.transform.position;
        shuffledCells.Enqueue(randomCell);
        return randomCoord;
    }

    void Parent(GameObject parentOb, GameObject childOb)
    {
        childOb.transform.parent = parentOb.transform;
    }

    private Vector3 getBounds(Transform _transform)
    {
        var _renderer = _transform.GetComponent<Renderer>();
        var combinedBounds = _renderer.bounds;
        var renderers = _transform.GetComponentsInChildren<Renderer>();
        foreach (var childRenderer in renderers)
        {
            if (childRenderer != _renderer) combinedBounds.Encapsulate(childRenderer.bounds);
        }

        return combinedBounds.size;
    }

}

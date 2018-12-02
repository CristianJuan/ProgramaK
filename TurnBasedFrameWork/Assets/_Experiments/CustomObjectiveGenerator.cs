using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomObjectiveGenerator : MonoBehaviour {


    public Transform ObjectivesParent;
    public GameObject objectivePrefab;
    public CellGrid CellGrid;// for inspector snap to grid

    Queue<Cell> shuffledCells;
    public int seed = 10;

    public void RandomSpawnObjective(List<Cell> cellsFromCellGridObj)
    {
        List<Cell> freeCells = cellsFromCellGridObj.FindAll(h => h.GetComponent<Cell>().IsTaken == false);
        shuffledCells = new Queue<Cell>(Utility.ShuffleArray(freeCells.ToArray(), seed));
        CreateObjectiveInRandomCell();

        //for (int i = 0; i < ObjectivesParent.childCount; i++)
        //{
        //    var objective = ObjectivesParent.GetChild(i);// I need a function to Create the objectives Childs first.
        //    var cell = cellsFromCellGridObj.OrderBy(h => Math.Abs((h.transform.position - objective.transform.position).magnitude)).First();
        //    if (!cell.IsTaken)
        //    {
        //        //cell.IsTaken = true;
        //        cell.HasObjective = true;
        //        var bounds = getBounds(objective);
        //        Vector3 offset = new Vector3(0, bounds.y + 1f, 0);
        //        objective.localPosition = cell.transform.localPosition + offset;
        //    }
        //    else
        //    {
        //        Debug.Log("When spawing an objective found a cell which is taken");
        //        Destroy(objective.gameObject);
        //    }

        //}
    }

    //public Vector3 _SelectedCellCoord;
    public void CreateObjectiveInRandomCell()
    {
        Cell _randomCell = GetRandomCell();
        Vector3 SelectedCellCoord = _randomCell.transform.position;
       // _SelectedCellCoord = SelectedCellCoord;// to watch in the inspector
        Transform newObjective = Instantiate(objectivePrefab.transform, SelectedCellCoord, Quaternion.identity) as Transform;
        var bounds = getBounds(newObjective);
        Vector3 offset = new Vector3(0, bounds.y + 1f, 0);
        newObjective.localPosition = newObjective.localPosition + offset;
        _randomCell.HasObjective = true;
        // newObjective.parent = ObjectivesParent;
        newObjective.parent = _randomCell.transform;
    }

    public Cell GetRandomCell()
    {
        Cell randomCell = shuffledCells.Dequeue();
        shuffledCells.Enqueue(randomCell);
        return randomCell;

    }

    //public Vector3 GetRandomCellPosition()
    //{
    //    Cell randomCell = shuffledCells.Dequeue();
    //    while(randomCell.IsTaken)
    //    {
    //        shuffledCells.Enqueue(randomCell);
    //        randomCell = shuffledCells.Dequeue();
    //    }
    //    // Out of while therefore, have a random cell that is not taken.
    //    Vector3 randomCoord = randomCell.transform.position;
    //    shuffledCells.Enqueue(randomCell);
    //    return randomCoord;
    //}

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

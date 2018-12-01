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
        shuffledCells = new Queue<Cell>(Utility.ShuffleArray(cellsFromCellGridObj.ToArray(), seed));
        CreateObjectiveInObjectivesParent();

        for (int i = 0; i < ObjectivesParent.childCount; i++)
        {
            var objective = ObjectivesParent.GetChild(i);// I need a function to Create the objectives Childs first.
            var cell = cellsFromCellGridObj.OrderBy(h => Math.Abs((h.transform.position - objective.transform.position).magnitude)).First();
            if (!cell.IsTaken)
            {
                cell.IsTaken = true;
                var bounds = getBounds(objective);
                Vector3 offset = new Vector3(0, bounds.y + 1f, 0);
                objective.localPosition = cell.transform.localPosition + offset;
            }
            else
            {
                Debug.Log("When spawing an objective found a cell which is taken");
                Destroy(objective.gameObject);
            }

        }
    }

    public void CreateObjectiveInObjectivesParent()
    {
        Vector3 randomCoord = GetRandomCellPosition();
        Transform newObjective = Instantiate(objectivePrefab.transform, randomCoord, Quaternion.identity) as Transform;
        newObjective.parent = ObjectivesParent;
    }

    public Vector3 GetRandomCellPosition()
    {
        Cell randomCell = shuffledCells.Dequeue();
        while(randomCell.IsTaken)
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

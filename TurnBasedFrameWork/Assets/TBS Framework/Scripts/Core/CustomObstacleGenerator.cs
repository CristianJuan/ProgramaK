using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomObstacleGenerator : MonoBehaviour
{
    public Transform ObstaclesParent;
    public GameObject obstaclePrefab;
    public CellGrid CellGrid;// for inspector snap to grid

    Queue<Cell> shuffledCells;

    public int seed = 10;

    public int Height;
    public int Width;

    [Range(0, 1)] 
    public float obstaclePercent;

    Vector3 mapCentre;

    public void RandomSpawnObstacles(List<Cell> cellsFromCellGridObj)// this parameter is design to come from the Cell Grid Cells property.
    {
        shuffledCells = new Queue<Cell>(Utility.ShuffleArray(cellsFromCellGridObj.ToArray(), seed));
        List<Transform> cellsTransform = new List<Transform>();

        CreateRandomObstaclesInObstaclesParent();
        foreach (Cell c in cellsFromCellGridObj)
        {
            cellsTransform.Add(c.transform);
        }
        SnapToGrid(cellsTransform);

        for (int i = 0; i < ObstaclesParent.childCount; i++)
        {
            var obstacle = ObstaclesParent.GetChild(i);// I need a function to Create the Obstacles Childs first.

            var cell = cellsFromCellGridObj.OrderBy(h => Math.Abs((h.transform.position - obstacle.transform.position).magnitude)).First();
            if (!cell.IsTaken)
            {
                cell.IsTaken = true;
                var bounds = getBounds(obstacle);
                Vector3 offset = new Vector3(0, bounds.y, 0);
                obstacle.localPosition = cell.transform.localPosition + offset;
            }
            else
            {
                Destroy(obstacle.gameObject);
            }
        }
    }

    private void CreateRandomObstaclesInObstaclesParent()
    {
        int obstacleCount = (int)(Height * Width * obstaclePercent);
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 randomCoord = GetRandomCellPosition();
            Transform newObstacle = Instantiate(obstaclePrefab.transform, randomCoord, Quaternion.identity) as Transform;
            newObstacle.parent = ObstaclesParent;
        }
    }

    /// <summary>
    /// Method snaps obstacle objects to the nearest cell. Works in the Inspector Only right now.
    /// </summary>
    public void SnapToGrid()
    {
        List<Transform> cells = new List<Transform>();

        foreach (Transform cell in CellGrid.transform)
        {
            cells.Add(cell);
        }

        foreach (Transform obstacle in ObstaclesParent)
        {
            var bounds = getBounds(obstacle);
            var closestCell = cells.OrderBy(h => Math.Abs((h.transform.position - obstacle.transform.position).magnitude)).First();
            if (!closestCell.GetComponent<Cell>().IsTaken)
            {
                Vector3 offset = new Vector3(0, bounds.y, 0);
                obstacle.localPosition = closestCell.transform.localPosition + offset;
            }
        }
    }

    public void SnapToGrid(List<Transform> cells)
    {
        foreach (Transform obstacle in ObstaclesParent)
        {
            var bounds = getBounds(obstacle);
            var closestCell = cells.OrderBy(h => Math.Abs((h.transform.position - obstacle.transform.position).magnitude)).First();
            if (!closestCell.GetComponent<Cell>().IsTaken)
            {
                Vector3 offset = new Vector3(0, bounds.y, 0);
                obstacle.localPosition = closestCell.transform.localPosition + offset;
            }
        }
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

    void Parent(GameObject parentOb, GameObject childOb)
    {
        childOb.transform.parent = parentOb.transform;
    }

    public Vector3 GetRandomCellPosition()
    {
        Cell randomCell = shuffledCells.Dequeue();
        Vector3 randomCoord = randomCell.transform.position;
        shuffledCells.Enqueue(randomCell);
        return randomCoord;
    }
}


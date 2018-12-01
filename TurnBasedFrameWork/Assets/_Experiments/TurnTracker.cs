using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnTracker : MonoBehaviour {

    public int CurrentTurn = 0;
    public CellGrid _cellGrid;

    private void Start()
    {
        _cellGrid.GameStarted += IncreaseTurn;
        _cellGrid.TurnEnded += IncreaseTurn;
        _cellGrid.GameEnded += ResetTurnTracker;
    }

    private void ResetTurnTracker(object sender, EventArgs e)
    {
        Debug.Log("Called ResetTurnTracker");
        CurrentTurn = 0;
    }

    private void IncreaseTurn(object sender, EventArgs e)
    {
        CurrentTurn++;
        Debug.Log("Im increasing the current turn current turn value = " + CurrentTurn.ToString());
    }
}

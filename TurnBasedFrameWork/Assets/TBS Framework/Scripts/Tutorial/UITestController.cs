using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UITestController : MonoBehaviour {

    public CellGrid CellGrid;
    public NodeUI TurretUI;// to get the canvas.

    void Awake()
    {
        CellGrid.UnitAdded += OnUnitAdded;
    }

    private void OnUnitAdded(object sender, UnitCreatedEventArgs e)
    {
        RegisterUnit(e.unit);
    }

    private void RegisterUnit(Transform unit)
    {
        unit.GetComponent<Unit>().UnitClicked += OnUnitClick;
        unit.GetComponent<Unit>().UnitDeselected += OnUnitDeselected;
    }

    private void OnUnitDeselected(object sender, EventArgs e)
    {
        var unit = sender as SampleUnit;
        Debug.Log("From the UITest script, OnUnitDeselected is called, i see that sender was:" + unit.name);
        TurretUI.Hide();
    }

    private void OnUnitClick(object sender, EventArgs e)
    {
        var unit = sender as SampleUnit;
        Debug.Log("From the UITest script, OnUnitClick is called, i see that sender was:" + unit.name);
        var UnitWeaponSystemController = unit.getMyWeaponSystemController();
        Debug.Log("My equipped weapon system is: " + UnitWeaponSystemController.EquippedWeaponSystem.name);
        TurretUI.SetTarget(unit);
    }
}
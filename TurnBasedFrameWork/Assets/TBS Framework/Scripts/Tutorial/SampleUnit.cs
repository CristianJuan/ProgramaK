using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SampleUnit : Unit
{
    public Color LeadingColor;//from the inspector window of the prefab.
    public string unitname;
    //public MyTurret myTurret;
    WeaponSystemController myWeaponSystemController;

    public WeaponSystemController getMyWeaponSystemController()
    {
        return myWeaponSystemController;
    }

    public override void Initialize()
    {
        base.Initialize();
        transform.position += new Vector3(0, 1, 0);
        GetComponent<Renderer>().material.color = LeadingColor;// CJG 11/19/2018, this makes the material of the Unit equal to the Leading Color color selected from the inspector
    }

    public QuickOutline _thisOutlineScript;// CJG 11/19/2018

    // Start CJG 11/19/2018
    private void Start()
    {
        this.UnitMoved += ProcessingAfterUnitMoves;
        //Debug.Log("Unit: " + this.name + " called Start");
        _thisOutlineScript = GetComponent<QuickOutline>();
        _thisOutlineScript.enabled = false;
        if (fieldOfView == null)
        {
            UseFieldOfView = false;
        }

        myWeaponSystemController = this.GetComponent<WeaponSystemController>();
        weaponSystems.Add(myWeaponSystemController.EquippedWeaponSystem as IWeaponSystem);
       // myTurret = myWeaponSystemController.EquippedWeaponSystem as MyTurret;

    }

    // End CJG 11/19/2018

    public override void MarkAsAttacking(Unit other)
    {
        Debug.Log(unitname + " Called MarkAsAttacking");
        Debug.Log(unitname + " MarkAsAttacking passed as parameter the unit " + other.name);
        if(myWeaponSystemController != null)
        {
            myWeaponSystemController.ExecuteSystem(ref other);
            //myTurret.WeaponSystemProcess(ref other);
        }
        else
        {
            Debug.Log("myWeaponSystemController is null");// When ships have more than one turret, i will need to check which weapon system was selected from the UI.
        }

    }

    public override void MarkAsDefending(Unit other)
    {
        Debug.Log(unitname + " Called MarkAsDefending");
    }

    public override void MarkAsDestroyed()
    {      
    }

    public override void MarkAsFinished()
    {
    }

    public override void MarkAsFriendly()
    {

        //Debug.Log("Unit: "+ this.name + " called MarkAsFriendly");
        //GetComponent<Renderer>().material.color = LeadingColor + new Color(0.8f, 1, 0.8f); CJG 11/19/2018
        // start CJG 11/19/2018
        if (_thisOutlineScript != null)
        {
            _thisOutlineScript.enabled = true;
        }
        else
        {
            Debug.Log("Outline Script is null");
        }
        // END CJG 11/19/2018
    }

    public override void MarkAsReachableEnemy()
    {
        GetComponent<Renderer>().material.color = LeadingColor + Color.red ;
    }

    public override void MarkAsSelected()
    {
        GetComponent<Renderer>().material.color = LeadingColor + new Color(0.8f, 1, 0.8f);
    }

    public override void UnMark()
    {
        // START CJG 11/19/2018
        GetComponent<Renderer>().material.color = LeadingColor;
        if (_thisOutlineScript != null)
        {
            _thisOutlineScript.enabled = false;
        }
        else
        {
            Debug.Log("Outline Script is null");
        }
        //END CJG 11/19/2018
    }

    public override void OnUnitSelected()
    {

        base.OnUnitSelected();
    }

    public override bool IsUnitAttackable(Unit other, Cell sourceCell)
    {
        FieldOfView _FOV = this.GetComponent<FieldOfView>();
        _FOV.FindVisibleTargets();

        if (_FOV.visibleTargetsInUnits.Contains(other as SampleUnit))
            return true;
        else
            return false;
    }

    //public Component[] comp; To inspect in Unity using the inspector
    private void ProcessingAfterUnitMoves(object sender, MovementEventArgs e)
    {
        string Enemy = "Enemy";
        string Friend = "Friend";
        SampleUnit senderAsUnit = sender as SampleUnit;
        bool CellHasObjective = e.DestinationCell.HasObjective;
        if(CellHasObjective)//this means that there is a child in the cell with an objective script.
        {
            e.DestinationCell.HasObjective = false;
            if(senderAsUnit != null)
            {
                // Check if friend unit or enemy unit got in it
                if (senderAsUnit.tag == Friend)
                {
                    Debug.Log("Friendly unit got into the cell with the objective");
                    StartCoroutine(GetObjective(e, 2.1f));
                }
                else
                {
                    Debug.Log("Something else entered into the cell with the objective");
                    if (senderAsUnit.tag == "Enemy")
                    {
                        Debug.Log("An enemy unit entered into the cell with the objective");
                        StartCoroutine(GetObjective(e, 2.1f));
                    }
                }
            }
            else
            {
                Debug.Log("sender is null");
            }

        }

    }

    IEnumerator GetObjective(MovementEventArgs e, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Component[] cellComponents = e.DestinationCell.transform.GetComponentsInChildren<Objective>();
        GameObject objectiveGameObject = cellComponents[0].gameObject;
        Destroy(objectiveGameObject);
    }
}

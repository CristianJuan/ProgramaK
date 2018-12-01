using System;
using UnityEngine;

public class GUIController : MonoBehaviour
{
    public CellGrid CellGrid;
	
    void Awake()
    { 
        CellGrid.LevelLoading += onLevelLoading;
        CellGrid.LevelLoadingDone += onLevelLoadingDone;
    }

    private void onLevelLoading(object sender, EventArgs e)
    {
        Debug.Log("GUI controller onLevelLoading: sender.name ==" + sender.ToString()
                  + " EventArgs e:" + e.ToString());
        Debug.Log("Level is loading");
    }

    private void onLevelLoadingDone(object sender, EventArgs e)
    {
        Debug.Log("Level loading done");
        Debug.Log("Press 'n' to end turn");
    }

    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Saw key N pressed.");
            CellGrid.EndTurn();//User ends his turn by pressing "n" on keyboard.
        }
	}
}

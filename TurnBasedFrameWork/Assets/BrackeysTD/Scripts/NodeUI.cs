using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour {

	public GameObject ui;

	public Text upgradeCost;
	public Button upgradeButton;

	public Text sellAmount;

	private Unit targetUnit;

	public void SetTarget (Unit _target)
	{
        targetUnit = _target;

        transform.position = _target.transform.position + new Vector3(-0.3f,0.7f,0.4f);

		//if (!target.isUpgraded)
		//{
		//	upgradeCost.text = "$" + target.turretBlueprint.upgradeCost;
		//	upgradeButton.interactable = true;
		//} else
		//{
		//	upgradeCost.text = "DONE";
		//	upgradeButton.interactable = false;
		//}

		//sellAmount.text = "$" + target.turretBlueprint.GetSellAmount();

		ui.SetActive(true);
	}

	public void Hide ()
	{
		ui.SetActive(false);
	}


    public void Tool1Button()
    {
        Debug.Log("Clicked on Tool 1 button");
    }

    public void Tool2Button()
    {
        Debug.Log("Clicked on Tool 2 button");
    }

    //public void Upgrade ()
    //{
    //	target.UpgradeTurret();
    //	BuildManager.instance.DeselectNode();
    //}

    //public void Sell ()
    //{
    //	target.SellTurret();
    //	BuildManager.instance.DeselectNode();
    //}

}

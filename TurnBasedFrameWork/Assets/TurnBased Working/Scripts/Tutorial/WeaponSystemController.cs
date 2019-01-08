using UnityEngine;
using System.Collections;

public class WeaponSystemController : MonoBehaviour
{

    public Transform WeaponSystemPlaceInUnit;
    public IWeaponSystem startingGun;
    IWeaponSystem equippedWeaponSystem;

    public IWeaponSystem EquippedWeaponSystem { get { return equippedWeaponSystem; } }

    void Start()
    {
        if (startingGun != null)
        {
            EquipGun(startingGun);
        }
    }

    public void EquipGun(IWeaponSystem gunToEquip)
    {
        if (equippedWeaponSystem != null)
        {
            Destroy(equippedWeaponSystem.gameObject);
        }
        equippedWeaponSystem = Instantiate(gunToEquip, WeaponSystemPlaceInUnit.position, WeaponSystemPlaceInUnit.rotation) as IWeaponSystem;
        equippedWeaponSystem.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        equippedWeaponSystem.transform.parent = WeaponSystemPlaceInUnit;

    }

    public void ExecuteSystem(ref Unit other)
    {
        Debug.Log(this.name + " called the ExecuteSystem procedure from its WeaponSystemController Script");
        equippedWeaponSystem.WeaponSystemProcess(ref other);
    }

}

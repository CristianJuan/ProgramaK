using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IWeaponSystem : MonoBehaviour {

    public abstract void Shoot();
    public abstract void LockOnTarget();
    public abstract void UpdateTarget(ref Unit other);
    public abstract void WeaponSystemProcess(ref Unit other);
    public enum WeaponType { Turret, Laser, Missle, ShiledGenerator };
    public WeaponType weaponType;

    public WeaponType ThisWeaponType { get { return weaponType; } }
}

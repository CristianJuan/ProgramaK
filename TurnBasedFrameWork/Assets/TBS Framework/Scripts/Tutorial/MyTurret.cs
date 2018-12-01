using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MyTurret : IWeaponSystem
{

    private Transform target;
    private Enemy targetEnemy;
    public string turretName = "";

   // [Header("General")]

    //public float range = 15f;

    [Header("Use Bullets (default)")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;

    [Header("Use Laser")]
    public bool useLaser = false;

    public int damageOverTime = 30;
    public float slowAmount = .5f;

    public LineRenderer lineRenderer;
    public ParticleSystem impactEffect;
    public Light impactLight;

    [Header("Unity Setup Fields")]
    public Transform partToRotate;
    public float turnSpeed = 10f;

    public Transform firePoint;

    [Header("Weapon system fields")]
    public WeaponType myWeaponType = WeaponType.Turret;
    // Use this for initialization
    void Start()
    {
     //   InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    public override void UpdateTarget(ref Unit other)
    {
        Debug.Log(turretName + "Called UpdateTarget");
        Debug.Log(turretName + " In UpdateTarget the unit other is " + other.name);
        float distanceToEnemyUnit = Vector3.Distance(transform.position, other.transform.position);
        target = other.transform;
        targetEnemy = other.GetComponent<Enemy>();

    }

    public override void WeaponSystemProcess(ref Unit other)
    {
        Debug.Log(turretName + " Called WeaponSystemProcess");
        Debug.Log(turretName + " In WeaponSystemProcess the unit other is " + other.name);

        UpdateTarget(ref other);

        if (target == null)
        {
            Debug.Log(turretName + " In WeaponSystemProcess, the target is null ");
            return;
        }

        LockOnTarget();
        Shoot();
    }

    public override void LockOnTarget()
    {
        Debug.Log(turretName + "Called LockOnTarget");
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    public void Laser()
    {
        targetEnemy.TakeDamage(damageOverTime * Time.deltaTime);
        targetEnemy.Slow(slowAmount);

        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            impactEffect.Play();
            impactLight.enabled = true;
        }

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, target.position);

        Vector3 dir = firePoint.position - target.position;

        impactEffect.transform.position = target.position + dir.normalized;

        impactEffect.transform.rotation = Quaternion.LookRotation(dir);
    }

    public override void Shoot()
    {
        Debug.Log(turretName + "Called Shoot");
        Debug.Log("Shoot has a target called " + target.name);
        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Seek(target);
    }
}

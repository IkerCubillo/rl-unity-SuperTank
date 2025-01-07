using Unity.MLAgents;
using UnityEngine;

public class EnemyTurret : Agent
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 5f;
    //[SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private Vector3 posicionTorreta = new Vector3(-0.5f, 2.5f, -4);
    private float nextFireTime = 0f;

    private void Start()
    {

    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = posicionTorreta;
    }

    private void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        TurretProjectile projectileScript = projectile.AddComponent<TurretProjectile>();
        projectileScript.Initialize(projectileSpeed);
        //Debug.Log($"Torreta {gameObject.name} disparando!");
    }
} 
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float projectileSpeed = 20f;

    private float nextFireTime = 0f;

    private void Start()
    {
        // Si no hay un punto de disparo asignado, crear uno
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            firePoint = fp.transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = new Vector3(0, 0, 1); // Ajusta según el modelo de tu torreta
        }
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
        Debug.Log($"Torreta {gameObject.name} disparando!");
    }
} 
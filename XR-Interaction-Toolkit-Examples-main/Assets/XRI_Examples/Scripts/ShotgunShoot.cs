using UnityEngine;

public class ShotgunShoot : MonoBehaviour
{
    public Transform muzzle;

    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float spawnOffset = 0.15f;

    public void Fire()
    {

        if (muzzle == null || bulletPrefab == null)
            return;

        Vector3 spawnPosition = muzzle.position + muzzle.forward * spawnOffset;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, muzzle.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
            rb.linearVelocity = muzzle.forward * bulletSpeed;
    }
}
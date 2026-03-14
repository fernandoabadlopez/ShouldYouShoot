using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject bulletImpactPrefab;

    void OnCollisionEnter(Collision collision)
    {
        if (bulletImpactPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];

            GameObject impact = Instantiate(
                bulletImpactPrefab,
                contact.point + contact.normal * 0.01f,
                Quaternion.LookRotation(contact.normal) * Quaternion.Euler(0, 180, 0)
            );

            impact.transform.SetParent(collision.transform);
        }

        Destroy(gameObject);
    }
}
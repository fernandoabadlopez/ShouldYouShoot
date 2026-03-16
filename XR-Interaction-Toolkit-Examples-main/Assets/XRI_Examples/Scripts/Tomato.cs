using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class TomatoVR : MonoBehaviour
{
    public GameObject tomatoSplatParticles; // el objeto que contiene los Particle Systems
    private Rigidbody rb;
    private bool isActivated = false; // flag que indica si ya se ha cogido alguna vez

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // XR Grab Interactable
        var grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnRelease);

        // al principio, las partículas no se activan
        if (tomatoSplatParticles != null)
        {
            var systems = tomatoSplatParticles.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in systems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    // se llama al soltar el tomate
    void OnRelease(SelectExitEventArgs args)
    {
        // activar física si no lo estaba
        rb.isKinematic = false;

        // marcar que ya ha sido cogido
        isActivated = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        // solo genera partículas si ya se ha cogido el tomate al menos una vez
        if (!isActivated) return;

        ContactPoint contact = collision.contacts[0];

        // Splash grande
        Transform splash = tomatoSplatParticles.transform.Find("SplashPS");
        if (splash != null)
        {
            splash.position = contact.point + contact.normal * 0.01f;
            splash.rotation = Quaternion.LookRotation(contact.normal);
            ParticleSystem ps = splash.GetComponent<ParticleSystem>();
            ps.Clear();
            ps.Play();
            splash.SetParent(collision.transform);
        }

        // Gotas pequeñas
        ParticleSystem drops = tomatoSplatParticles.GetComponent<ParticleSystem>();
        drops.transform.position = contact.point;
        drops.transform.rotation = Quaternion.LookRotation(contact.normal);
        drops.Clear();
        drops.Play();
        drops.transform.SetParent(collision.transform);
    }
}
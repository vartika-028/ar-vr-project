using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera cam = null;                 // camera used for raycast
    [SerializeField] TMP_Text scoreText = null;         // TextMeshPro UI element for score
    [SerializeField] ParticleSystem muzzleFlash = null; // particle system at muzzle
    [SerializeField] Light muzzleLight = null;          // small point light for flash (optional)
    [SerializeField] AudioSource gunAudio = null;       // audio source (gun shot)
    [SerializeField] AudioClip gunShotClip = null;      // shot clip (optional)
    [SerializeField] ParticleSystem impactEffect = null;// small impact VFX (optional)
    [SerializeField] LayerMask hitMask = Physics.DefaultRaycastLayers; // which layers to hit

    [Header("Shooting")]
    [SerializeField] float range = 100f;
    [SerializeField] float forceOnRigidbody = 10f;
    [SerializeField] float fireCooldown = 0.15f; // seconds between shots

    int score = 0;
    float lastFireTime = -99f;

    void Reset()
    {
        // helpful defaults if you add the component via Inspector
        cam = Camera.main;
    }

    void Update()
    {
        if (CanFire() && IsFirePressed())
        {
            TryFire();
        }
    }

    bool CanFire()
    {
        return Time.time - lastFireTime >= fireCooldown;
    }

    bool IsFirePressed()
    {
        // PC / Editor - left mouse button
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        // Touchscreen - primary touch began
        if (Touchscreen.current != null)
        {
            // safer: check primaryTouch exists
            var primary = Touchscreen.current.primaryTouch;
            if (primary != null)
            {
                // Use the Input System Touch "wasPressedThisFrame" friendly check:
                if (primary.press.wasPressedThisFrame)
                    return true;

                // Alternatively compare phases if needed and avoid ambiguous TouchPhase
                // var ph = primary.phase.ReadValue();
                // if (ph == global::UnityEngine.InputSystem.TouchPhase.Began) return true;
            }
        }

        // also allow keyboard space/fire for quick testing
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            return true;

        return false;
    }

    void TryFire()
    {
        lastFireTime = Time.time;

        // play effects
        if (muzzleFlash != null) muzzleFlash.Play();
        if (gunAudio != null)
        {
            if (gunShotClip != null) gunAudio.PlayOneShot(gunShotClip);
            else gunAudio.Play();
        }
        if (muzzleLight != null) StartCoroutine(FlashLightCoroutine());

        // shoot
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask.value))
        {
            // impact VFX
            if (impactEffect != null)
            {
                var v = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                v.Play();
                Destroy(v.gameObject, 2f);
            }

            // apply physics or destroy
            Rigidbody rb = hit.rigidbody;
            if (rb != null)
            {
                rb.AddForce(-hit.normal * forceOnRigidbody, ForceMode.Impulse);
            }
            else
            {
                // if you want to only destroy targets with a tag "Target", check tag first:
                // if (hit.collider.CompareTag("Target")) Destroy(hit.collider.gameObject);
                Destroy(hit.collider.gameObject);
            }

            // update score
            score++;
            if (scoreText != null) scoreText.text = "Score: " + score;
        }
    }

    IEnumerator FlashLightCoroutine()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        muzzleLight.enabled = false;
    }
}

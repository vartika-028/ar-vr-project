using UnityEngine;

public class Target : MonoBehaviour
{
    public int points = 1;
    public bool addImpulseOnHit = true;
    public float impulse = 8f;

    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    public void OnHit(RaycastHit hit)
    {
        if (addImpulseOnHit && rb != null)
            rb.AddForce(-hit.normal * impulse, ForceMode.Impulse);

        ScoreManager.Add(points);
        Destroy(gameObject);
    }
}

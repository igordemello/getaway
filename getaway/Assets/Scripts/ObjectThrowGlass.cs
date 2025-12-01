using UnityEngine;

public class ObjectThrowGlass : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        ShatterableGlass glass = collision.collider.GetComponent<ShatterableGlass>();

        if (glass != null)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 hitDirection = collision.relativeVelocity.normalized;

            glass.Shatter3D(new ShatterableGlassInfo(hitPoint, hitDirection));
        }
    }
}

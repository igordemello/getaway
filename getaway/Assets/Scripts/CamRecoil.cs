using UnityEngine;

public class CamRecoil : MonoBehaviour
{
    private float rotationSpeed;
    private float returnSpeed;
    private Vector3 RecoilRotation;

    private Vector3 currentRotation;
    private Vector3 Rot;

    private void FixedUpdate()
    {
        currentRotation = Vector3.Lerp(currentRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        Rot = Vector3.Slerp(Rot, currentRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(Rot);
    }

    public void Fire()
    {
        currentRotation += new Vector3(-RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
    }

    public void SetRecoilSettings(float rotSpeed, float retSpeed, Vector3 recoil)
    {
        rotationSpeed = rotSpeed;
        returnSpeed = retSpeed;
        RecoilRotation = recoil;
    }
}
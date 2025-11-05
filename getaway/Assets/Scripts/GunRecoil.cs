using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Reference Points")]
    public Transform recoilPosition;

    [Header("Speed Settings")]
    public float positionalRecoilSpeed = 8f;
    public float positionalReturnSpeed = 18f;
    public float rotationalRecoilSpeed = 8f;
    public float rotationalReturnSpeed = 18f;

    [Header("Amount Settings")]
    public Vector3 RecoilKickBack = new Vector3(0.015f, 0f, -0.2f);
    public Vector3 RecoilRotation = new Vector3(5f, 2f, 2f);

    private Vector3 positionalRecoil;
    private Vector3 rotationalRecoil;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    private void Start()
    {
        initialLocalPos = recoilPosition.localPosition;
        initialLocalRot = recoilPosition.localRotation;
    }

    private void FixedUpdate()
    {
        positionalRecoil = Vector3.Lerp(positionalRecoil, Vector3.zero, positionalReturnSpeed * Time.deltaTime);
        rotationalRecoil = Vector3.Lerp(rotationalRecoil, Vector3.zero, rotationalReturnSpeed * Time.deltaTime);

        Vector3 targetPos = initialLocalPos + positionalRecoil;
        Quaternion targetRot = initialLocalRot * Quaternion.Euler(rotationalRecoil);

        recoilPosition.localPosition = Vector3.Slerp(recoilPosition.localPosition, targetPos, positionalRecoilSpeed * Time.fixedDeltaTime);
        recoilPosition.localRotation = Quaternion.Slerp(recoilPosition.localRotation, targetRot, rotationalRecoilSpeed * Time.fixedDeltaTime);
    }

    public void Fire()
    {
        positionalRecoil += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x), Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);
        rotationalRecoil += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y),Random.Range(-RecoilRotation.z, RecoilRotation.z));
    }
}

using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Reference Points")]
    public Transform recoilPosition;

    [Header("Speed Settings")]
    public float positionalRecoilSpeed = 8f;
    public float positionalReturnSpeed = 18f;

    [Header("Amount Settings")]
    public Vector3 RecoilKickBack = new Vector3(0.015f, 0f, -0.2f);

    Vector3 positionalRecoil;
    private Vector3 initialLocalPos;

    private void Start()
    {
        initialLocalPos = recoilPosition.localPosition;
    }

    private void FixedUpdate()
    {
        positionalRecoil = Vector3.Lerp(positionalRecoil, Vector3.zero, positionalReturnSpeed * Time.deltaTime);
        Vector3 targetPos = initialLocalPos + positionalRecoil;
        recoilPosition.localPosition = Vector3.Slerp(recoilPosition.localPosition, targetPos, positionalRecoilSpeed * Time.fixedDeltaTime);
        //recoilPosition.localPosition = Vector3.Slerp(recoilPosition.localPosition, positionalRecoil, positionalRecoilSpeed * Time.fixedDeltaTime);
    }

    public void Fire()
    {
        positionalRecoil += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x), Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);
    }
}

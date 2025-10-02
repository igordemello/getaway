using UnityEngine;

public class GunUIRecoil : MonoBehaviour
{
    public RectTransform gunSprite;

    [Header("Recoil Config")]
    public float recoilDistance = 1500;
    public float recoilScale = 2f;
    public float recoilSpeed = 6f;

    private Vector3 originalLocalPos;
    private Vector3 originalScale;
    private Vector3 targetLocalPos;
    private Vector3 targetScale;

    void Start()
    {
        originalLocalPos = gunSprite.localPosition;
        originalScale = gunSprite.localScale;
        targetLocalPos = originalLocalPos;
        targetScale = originalScale;
    }

    void Update()
    {
        gunSprite.localPosition = Vector3.Lerp(gunSprite.localPosition, targetLocalPos, Time.deltaTime * recoilSpeed);
        gunSprite.localScale = Vector3.Lerp(gunSprite.localScale, targetScale, Time.deltaTime * recoilSpeed);
    }

    public void DoRecoil()
    {
        Vector3 recoilDir = -gunSprite.up * recoilDistance;
        recoilDir.x = recoilDir.x * 8;
        targetLocalPos = originalLocalPos + recoilDir;

        targetScale = originalScale * recoilScale;

        
        CancelInvoke(nameof(ResetRecoil));
        Invoke(nameof(ResetRecoil), 0.1f);
    }

    void ResetRecoil()
    {
        targetLocalPos = originalLocalPos;
        targetScale = originalScale;
    }
}

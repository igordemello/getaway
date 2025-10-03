using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.EventSystems;

public class Gun : MonoBehaviour
{
    public enum FireMode { SemiAuto, FullAuto }

    [Header("Gun Settings")]
    public FireMode fireMode = FireMode.SemiAuto;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.2f;
    public float recoilAmount = 2f;
    public float recoilRecovery = 5f;

    [Header("References")]
    public Camera fpsCam;
    public GunUIRecoil uiRecoil;

    private PlayerControls controls;
    private bool fireInput;
    private float nextTimeToFire = 0f;
    private float currentRecoil = 0f;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Fire.performed += ctx => fireInput = true;
        controls.Player.Fire.canceled += ctx => fireInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Update()
    {
        switch(fireMode)
        {
            case FireMode.SemiAuto:
                if (fireInput)
                {
                    Shoot();
                    fireInput = false;
                }
                break;

            case FireMode.FullAuto:
                if (fireInput && Time.time >= nextTimeToFire)
                {
                    nextTimeToFire = Time.time + fireRate;
                    Shoot();
                }
                break;
        }

        if (currentRecoil > 0f)
        {
            float recover = recoilRecovery * Time.deltaTime;
            currentRecoil = Mathf.Max(0, currentRecoil - recover);
            fpsCam.transform.localRotation = Quaternion.Euler(-currentRecoil, 0f, 0f);
        }

    }

    void Shoot()
    {

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            
            Target target = hit.transform.GetComponent<Target>();

            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }

        // recoil
        if (uiRecoil != null)
        {
            uiRecoil.DoRecoil();
        }
        currentRecoil += recoilAmount;
        fpsCam.transform.localRotation = Quaternion.Euler(-currentRecoil, 0f, 0f);
    }
}

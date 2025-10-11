using DG.Tweening.Core.Easing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class Gun : MonoBehaviour
{
    public enum FireMode { SemiAuto, FullAuto, Shotgun }

    [Header("Gun Settings")]
    public FireMode fireMode = FireMode.Shotgun;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.2f;

    public int maxAmmo = 10;
    private int currentAmmo;

    [Header("References")]
    public Camera fpsCam;
    public GameObject impact;
    public Animator animator;
    public TextMeshProUGUI debugAmmo;

    private PlayerControls controls;
    private bool fireInput;
    private float nextTimeToFire = 0f;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Fire.performed += ctx => fireInput = true;
        controls.Player.Fire.canceled += ctx => fireInput = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        debugAmmo.text = $"Ammo:\n{currentAmmo}/{maxAmmo}";

        switch (fireMode)
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

            case FireMode.Shotgun:
                if (fireInput && Time.time >= nextTimeToFire)
                {
                    fireInput = false;
                    StartCoroutine(DoubleBarrelShot());
                }
                break;
        }

    }

    private IEnumerator DoubleBarrelShot()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("Sem bala lerdao");
            yield break;
        }

        nextTimeToFire = Time.time + fireRate;

        Shoot();
        Shoot();

        yield return new WaitForSeconds(0.1f);

        animator.SetTrigger("Shoot");

        yield return new WaitForSeconds(0.8f);

        nextTimeToFire = Time.time;
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("Sem bala lerdao");
            return;
        }


        currentAmmo--;

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            
            Target target = hit.transform.GetComponent<Target>();

            if (target != null)
            {
                target.TakeDamage(damage);
            }

            GameObject impactGO = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO,1f);
        }
    }
}

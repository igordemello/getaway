using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerMovement;

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

    [Header("Recoil Settings")]
    public float recoilRotationSpeed = 60f;
    public float recoilReturnSpeed = 10f;
    public Vector3 recoilAmount = new Vector3(8f, 8f, 8f);

    [Header("References")]
    public Camera fpsCam;
    public GameObject impact;
    public Animator animator;
    public TextMeshProUGUI debugAmmo;
    public CamRecoil camRecoil;
    public GunRecoil gunRecoil;
    public ParticleSystem muzzle;

    private PlayerControls controls;
    private bool fireInput;
    private float nextTimeToFire = 0f;

    private Queue<GameObject> impactPool = new Queue<GameObject>();
    private int impactPoolSize = 50;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Fire.performed += ctx => fireInput = true;
        controls.Player.Fire.canceled += ctx => fireInput = false;

        if (animator != null)
            animator.keepAnimatorStateOnDisable = true;
    }

    private int lastStateHash;

    private void OnDisable()
    {
        controls.Disable();
        if (animator != null)
            lastStateHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
    }

    private void OnEnable()
    {
        controls.Enable();
        if (animator != null)
        {
            animator.keepAnimatorStateOnDisable = true;
            //animator.Play(lastStateHash, 0, 0f); // reinicia a animação atual do começo
            animator.Play("Idle", 0, 0f);
            animator.Update(0); // aplica imediatamente
        }
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        if (camRecoil != null)
        {
            camRecoil.SetRecoilSettings(recoilRotationSpeed, recoilReturnSpeed, recoilAmount);
        }

        for (int i = 0; i < impactPoolSize; i++)
        {
            GameObject obj = Instantiate(impact);
            obj.SetActive(false);
            impactPool.Enqueue(obj);
        }
    }

    void Update()
    {
        debugAmmo.text = $"Ammo:\n{currentAmmo}/{maxAmmo}";

        switch (fireMode)
        {
            case FireMode.SemiAuto:
                if (fireInput && Time.time >= nextTimeToFire)
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

        yield return new WaitForSeconds(0.7f);

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

            GameObject impactGO = GetImpactFromPool();
            if (impactGO != null)
            {
                impactGO.transform.position = hit.point;
                impactGO.transform.rotation = Quaternion.LookRotation(hit.normal);
                impactGO.SetActive(true);
                StartCoroutine(ReturnImpactToPool(impactGO, 1f));
            }
        }

        muzzle.Play();
        camRecoil.Fire();
        gunRecoil.Fire();
    }

    GameObject GetImpactFromPool()
    {
        if (impactPool.Count > 0)
        {
            GameObject obj = impactPool.Dequeue();
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(impact);
            obj.SetActive(false);
            return obj;
        }
    }

    IEnumerator ReturnImpactToPool(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        impactPool.Enqueue(obj);
    }

}

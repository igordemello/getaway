using UnityEngine;

public class GunSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;

    private PlayerControls controls;
    private float scrollInput;
    private bool revolver;
    private bool shotgun;
    private bool bow;
    private bool knife;

    private float switchCooldown = 0.4f;
    private bool canSwitch = true;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Scroll.performed += ctx => scrollInput = ctx.ReadValue<Vector2>().y;
        controls.Player.Scroll.canceled += ctx => scrollInput = 0f;

        controls.Player.Revolver.performed += ctx => revolver = true;
        controls.Player.Revolver.canceled += ctx => revolver = false;

        controls.Player.Shotgun.performed += ctx => shotgun = true;
        controls.Player.Shotgun.canceled += ctx => shotgun = false;

        controls.Player.Bow.performed += ctx => bow = true;
        controls.Player.Bow.canceled += ctx => bow = false;

        controls.Player.Knife.performed += ctx => knife = true;
        controls.Player.Knife.canceled += ctx => knife = false;
        //controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Start()
    {
        SelectWeapon();
    }

    void Update()
    {
        if (!canSwitch) return;

        int previousSelectedWeapon = selectedWeapon;

        if (scrollInput > 0f)
        {
            //Debug.Log("Entrou aqui SCROLL PRA CIMA");
            
            if (selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
        }
        if (scrollInput < 0f)
        {
            //Debug.Log("Entrou aqui SCROLL PRA BAIXO");
            if (selectedWeapon <= 0)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon--;
            }
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
            StartCoroutine(SwitchCooldown());
        }

        if (revolver)
        {
            selectedWeapon = 0;
            SelectWeapon();
        }
        if (shotgun)
        {
            selectedWeapon = 1;
            SelectWeapon();
        }
        if (bow)
        {
            selectedWeapon = 2;
            SelectWeapon();
        }
        if (knife)
        {
            selectedWeapon = 3;
            SelectWeapon();
        }
    }

    private System.Collections.IEnumerator SwitchCooldown()
    {
        canSwitch = false;
        yield return new WaitForSeconds(switchCooldown);
        canSwitch = true;
    }

    public void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            if (selectedWeapon == -1)
            {
                weapon.gameObject.SetActive(false);
                continue;
            }

            i++;
        }
    }
}

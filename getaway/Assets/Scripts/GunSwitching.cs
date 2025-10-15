using UnityEngine;

public class GunSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;

    private PlayerControls controls;
    private float scrollInput;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Scroll.performed += ctx => scrollInput = ctx.ReadValue<Vector2>().y;
        controls.Player.Scroll.canceled += ctx => scrollInput = 0f;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Start()
    {
        SelectWeapon();
    }

    void Update()
    {
        

        int previousSelectedWeapon = selectedWeapon;

        if (scrollInput > 0f)
        {
            Debug.Log("Entrou aqui SCROLL PRA CIMA");
            
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
            Debug.Log("Entrou aqui SCROLL PRA BAIXO");
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
        }
    }

    void SelectWeapon()
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

                i++;
        }
    }
}

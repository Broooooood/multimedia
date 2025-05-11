using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("References")]
    public Camera fpsCam;
    public Transform weaponTransform;
    public ParticleSystem muzzleFlash;
    public Text ammoText;
    public AudioSource audioSource;

    [Header("Shooting")]
    public float range = 100f;
    public float damage = 20f;
    public float fireRate = 0.1f;
    public LayerMask hitMask;

    [Header("Recoil")]
    public float recoilIntensity = 2f;
    public float recoilSmoothTime = 0.1f;
    private Vector2 currentRecoil;
    private Vector2 recoilVelocity;

    [Header("Visual Kickback")]
    public float kickbackDistance = 0.1f;
    public float kickbackReturnSpeed = 5f;
    private Vector3 originalWeaponPosition;

    [Header("Ammo")]
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Sounds")]
    public AudioClip shootSound;    // Sound when firing
    public AudioClip reloadSound;   // Sound when reloading

    private float nextFireTime = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoText();

        if (weaponTransform != null)
            originalWeaponPosition = weaponTransform.localPosition;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo <= 0)
            {
                Debug.Log("Out of ammo!");
                return;
            }

            nextFireTime = Time.time + fireRate;
            Shoot();
            ApplyRecoil();
            ApplyKickback();
        }

        SmoothRecoil();
        ReturnKickback();
    }

    void Shoot()
{
    currentAmmo--;
    UpdateAmmoText();

    if (muzzleFlash != null)
        muzzleFlash.Play();

    if (audioSource != null && shootSound != null)
        audioSource.PlayOneShot(shootSound);

    Vector3 origin = fpsCam.transform.position;
    Vector3 direction = fpsCam.transform.forward;

    RaycastHit hit;
    if (Physics.Raycast(origin, direction, out hit, range, hitMask))
    {
        Debug.Log("Hit: " + hit.transform.name);

        // Verifica se o inimigo atingido tem o script Target e aplica dano
        Target target = hit.transform.GetComponent<Target>();
        if (target != null)
        {
            Debug.Log("Dano aplicado em: " + hit.transform.name);  // Mostra qual inimigo foi atingido
            target.TakeDamage(damage);
        }
    }
}

    void ApplyRecoil()
    {
        float xRecoil = Random.Range(-recoilIntensity, recoilIntensity);
        float yRecoil = recoilIntensity;
        currentRecoil += new Vector2(xRecoil, yRecoil);
    }

    void SmoothRecoil()
    {
        Vector2 targetRecoil = Vector2.SmoothDamp(currentRecoil, Vector2.zero, ref recoilVelocity, recoilSmoothTime);
        fpsCam.transform.localRotation *= Quaternion.Euler(-targetRecoil.y, targetRecoil.x, 0);
        currentRecoil = targetRecoil;
    }

    void ApplyKickback()
    {
        if (weaponTransform != null)
        {
            weaponTransform.localPosition -= new Vector3(0, 0, kickbackDistance);
        }
    }

    void ReturnKickback()
    {
        if (weaponTransform != null)
        {
            weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, originalWeaponPosition, Time.deltaTime * kickbackReturnSpeed);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoText();
    }

    void UpdateAmmoText()
    {
        if (ammoText != null)
            ammoText.text = "Ammo: " + currentAmmo.ToString();
    }
}

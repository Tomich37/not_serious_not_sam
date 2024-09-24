using UnityEngine;
using TMPro;

public class GunSystem : MonoBehaviour
{
    // Public
    // Integers
    public int damage = 100;
    public uint magazineSize = 30;
    public uint bulletsPerShot = 1;
    public uint bulletsPerTap = 1;

    // Floats
    public float shootingDelay = 0.1f;
    public float spread = 0.1f;
    public float range = 100f;
    public float reloadTimeout = 1f;
    public float timeBetweenShots = 0.1f;

    // Bools
    public bool holdAllowed = true;

    // Private
    // Unsigned integers
    private uint m_remainedBullets = 0;
    private uint bulletsShot = 0;

    // Bools
    private bool m_shooting = false;
    private bool m_triggerHold = false;
    private bool m_triggerJustPressed = false;
    private bool m_readyToShot = false;
    private bool m_reloading = false;

    // References
    public Camera fpsCamera;
    public Transform attackPoint;
    public RaycastHit rcHit;
    public LayerMask whatHited;

    // Visual and UI
    public GameObject muzzleFlash, bulletHoleGraphic;
    public CamShake camShake;
    public float camShakeMagnitude = 0.1f, camShakeDuration = 0.05f;
    public TextMeshProUGUI ammoText;

    // Methods
    public void PressTrigger()
    {
        m_triggerHold = true;
        m_triggerJustPressed = true;
    }

    public void ReleaseTrigger()
    {
        m_triggerHold = false;
    }

    public void Reload()
    {
        if (m_reloading || m_remainedBullets == magazineSize)
        {
            return;
        }

        m_reloading = true;
        Invoke("ReloadFinished", reloadTimeout);
    }

    private void ReloadFinished()
    {
        m_remainedBullets = magazineSize;
        m_reloading = false;
    }

    private void Shoot()
    {
        // Применяем разброс
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 raycastDirection = fpsCamera.transform.forward + new Vector3(x, y, 0);

        // Визуальные эффекты
        Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        if (Physics.Raycast(fpsCamera.transform.position, raycastDirection, out RaycastHit hitInfo, range, whatHited))
        {
            Instantiate(bulletHoleGraphic, hitInfo.point, Quaternion.Euler(0, 180, 0));
            Debug.Log("Hited was: " + hitInfo.collider.name);

            // Нанесение урона
            if (hitInfo.collider.CompareTag("Enemy"))
            {
                // Пример: нанесение урона объекту с тегом "Enemy"
                hitInfo.collider.GetComponent<ShootingAi>().TakeDamage(damage);
            }
        }

        // Тряска камеры
        if (camShake != null)
        {
            StartCoroutine(camShake.Shake(camShakeDuration, camShakeMagnitude));
        }

        m_remainedBullets--;
        bulletsShot--;

        if (bulletsShot > 0 && m_remainedBullets > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void HandleTriggerPulled()
    {
        if (holdAllowed)
        {
            m_shooting = m_triggerHold;
        }
        else
        {
            m_shooting = m_triggerJustPressed;
        }

        m_triggerJustPressed = false;

        if (!m_readyToShot || !m_shooting || m_reloading || m_remainedBullets < 1)
        {
            return;
        }

        m_readyToShot = false;
        bulletsShot = bulletsPerTap;

        // Выстреливаем все пули из выстрела
        for (uint i = 0; i < bulletsPerShot; i++)
        {
            Shoot();
        }

        Invoke("ResetShot", shootingDelay);
    }

    private void ResetShot()
    {
        m_readyToShot = true;
    }

    void Update()
    {
        // Обработка нажатий
        if (Input.GetKey(KeyCode.Mouse0))
        {
            PressTrigger();
        }
        else
        {
            ReleaseTrigger();
        }

        // Обновление состояния патронов в интерфейсе
        ammoText.SetText(m_remainedBullets + " / " + magazineSize);

        // Ручная перезарядка
        if (Input.GetKeyDown(KeyCode.R) && m_remainedBullets < magazineSize && !m_reloading)
        {
            Reload();
        }

        HandleTriggerPulled();
    }
}

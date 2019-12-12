using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviour {

    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Image powerBar;
    [SerializeField] private AudioSource fireSound;

    public float damage;
    private float currentDamage;
    [SerializeField] private float currentPower = 0;
    [SerializeField] private float startPower = 5f;
    [SerializeField] private float maxPower = 50f;
    private float currentPowerValue;
    [SerializeField] private float chargeSpeed = 30f;
    [SerializeField] private float lerpSpeed = 10f;
    private bool mouseButtonHeldDown;

    public float CurrentPower {
        get { return currentPower; }
        set { currentPower = Mathf.Clamp(value, 0, maxPower); }
    }

    private void Start() {
        currentPower = startPower;
    }

    private void Update() {
        currentDamage = currentPower;

        if (Input.GetMouseButtonDown(0)) {
            mouseButtonHeldDown = true;
        }

        if (Input.GetMouseButtonUp(0)) {
            damage = currentDamage;
            Shoot();
            fireSound.Play();
            mouseButtonHeldDown = false;
            currentPower = startPower;
        }

        if(mouseButtonHeldDown && currentPower <= maxPower) {
            currentPower += Time.deltaTime * chargeSpeed;
        }
        currentPowerValue = Map(CurrentPower, 0, maxPower, 0 - 0.1f, 1);
        powerBar.fillAmount = Mathf.Lerp(powerBar.fillAmount, currentPowerValue, Time.deltaTime * lerpSpeed);
    }

    private void Shoot() {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * currentPower, ForceMode.Impulse);
    }

    // This method maps a range of numbers into another range
    public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}

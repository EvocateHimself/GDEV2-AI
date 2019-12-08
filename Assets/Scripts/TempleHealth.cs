using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TempleHealth : MonoBehaviour {

    [Header("Health")]
	public Shooting shooting;
	public Image healthBar;
    public Material destroyedMat;
	public float currentHealth;
	private float currentHealthValue;
	public float maxHealth = 100f;
	public float lerpSpeed = 10f;

    public float CurrentHealth {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

    private void Start() {
		currentHealth = maxHealth;
    }

    private void Update() {
        currentHealthValue = Map(CurrentHealth, 0, maxHealth, 0, 1);
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, currentHealthValue, Time.deltaTime * lerpSpeed);

        // Die
		if (currentHealth <= 0) {
			//Destroy(gameObject, .2f);
            gameObject.GetComponent<MeshRenderer>().material = destroyedMat;
		}
    }

    // Take damage when hit
	private void OnTriggerEnter(Collider other) {
		if(other.CompareTag("Bullet")) {
			currentHealth -= shooting.damage;
			print(shooting.damage);
		}
	}

    // This method maps a range of numbers into another range
    public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealth : MonoBehaviour {

    [Header("Health")]
	[SerializeField] private Image healthBar;
	[SerializeField] private AudioSource takeHitSound;
	[SerializeField] private UnitGuard unitGuard;

	[SerializeField] private float currentHealth;
	private float currentHealthValue;
	[SerializeField] private float maxHealth = 100f;
	[SerializeField] private float lerpSpeed = 10f;

    public float CurrentHealth {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

    private void Start() {
		currentHealth = maxHealth;
    }

    private void Update() {
        if (healthBar != null) {
            currentHealthValue = Map(CurrentHealth, 0, maxHealth, 0, 1);
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, currentHealthValue, Time.deltaTime * lerpSpeed);
        }

        // Die
		if (CurrentHealth <= 0) {
			Destroy(gameObject, .2f);
        }
    }

    // Take damage when hit
	private void OnTriggerEnter(Collider other) {
		if(other.CompareTag("Fireball")) {
			CurrentHealth -= unitGuard.fireBallDamage;
			takeHitSound.Play();
		}
	}

    // This method maps a range of numbers into another range
    public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}

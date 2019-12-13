using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour {

    [SerializeField] private float speed = 15f;
    [SerializeField] private GameObject impactPrefab;
	[SerializeField] private AudioSource takeHitSound;

    private Transform player;
    private Vector3 target;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        target = new Vector3(player.position.x, 0, player.position.z);
    }

    // Move the fireball forward when instantiated
    private void FixedUpdate() {
        Vector3 velocity = this.transform.forward * speed;
        this.transform.position = this.transform.position + velocity * Time.deltaTime;
        //transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")) {
            DestroyFireball();
        } else if(other.CompareTag("Environment")) {
            DestroyFireball();
        }
    }

    // Destroy the fireball and instantiate an explosion effect
    private void DestroyFireball() {
        var explosion = Instantiate(impactPrefab, gameObject.transform.position, Quaternion.identity);
        takeHitSound.Play();

        Destroy(explosion, 1f);
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBullet : MonoBehaviour {

    public GameObject impactPrefab;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Environment")) {
            DestroyFireball();
        } 
        
        else if (other.CompareTag("Enemy")) {
            DestroyFireball();
        }
    }

    // Destroy the fireball and instantiate an explosion effect
    private void DestroyFireball() {
        var explosion = Instantiate(impactPrefab, gameObject.transform.position, Quaternion.identity);
        Destroy(explosion, 1f);
        Destroy(gameObject);
    }
}

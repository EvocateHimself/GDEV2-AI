using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBullet : MonoBehaviour {

    public GameObject impactPrefab;

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Enemy") || other.CompareTag("Environment")) {
            DestroyFireball();
        }
    }

    private void DestroyFireball() {
        var explosion = Instantiate(impactPrefab, gameObject.transform.position, Quaternion.identity);
        Destroy(explosion, 1f);
        Destroy(gameObject);
    }
}

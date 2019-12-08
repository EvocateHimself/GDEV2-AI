using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUIHandler : MonoBehaviour{

    public Transform target;
 
    void Update() {
        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);

        if(target.GetComponent<Unit>() != null) {
            if (target.GetComponent<Unit>().currentHealth <= 0) {
                Destroy(gameObject, .2f);
            }
        }

        if(target.GetComponent<TempleHealth>() != null) {
            if (target.GetComponent<TempleHealth>().currentHealth <= 0) {
                Destroy(gameObject, .2f);
            }
        }
    }
}

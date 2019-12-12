using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUIHandler : MonoBehaviour{

    public Transform target;
 
    void Update() {
        if (target != null) {
            transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
            
            if(target.GetComponent<UnitGuard>() != null) {
                if (target.GetComponent<UnitGuard>().CurrentHealth <= 0) {
                    Destroy(gameObject, .5f);
                }
            }

            if(target.GetComponent<TempleHealth>() != null) {
                if (target.GetComponent<TempleHealth>().CurrentHealth <= 0) {
                    Destroy(gameObject, .5f);
                }
            }
        }
    }
}

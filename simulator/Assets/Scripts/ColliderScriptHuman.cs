using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderScriptHuman : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        if (other.transform.parent.parent != null && other.transform.parent.parent.CompareTag("Player")) {
            transform.parent.gameObject.GetComponent<SpawnedObject>().CollisionDetected();
        }
    }
}
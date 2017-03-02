using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour {

    [SerializeField] private float lifeTime = 5.0f;
    private Coroutine coroutineLife;


    // Use this for initialization
    void Start () {
        coroutineLife = StartCoroutine(LifeTimeCoroutine());
	}

    private void OnTriggerEnter(Collider other) {
        if (other.transform.parent.parent != null && other.transform.parent.parent.CompareTag("Player")) {
            CollisionDetected();
        }
    }

    private IEnumerator LifeTimeCoroutine() {
        yield return new WaitForSeconds(lifeTime);
        Destroy(this.gameObject);
    }

    public void CollisionDetected() {
        GameObject.FindObjectOfType<Environment>().DetectedCollision();
        StopCoroutine(coroutineLife);
        Destroy(this.gameObject);
    }
}

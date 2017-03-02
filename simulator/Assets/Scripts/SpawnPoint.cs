using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    [SerializeField] private float timeBetweenSpawns = 5.0f;
    private bool hasSpawned = false;
	
    public void Spawn(GameObject spawnObject) {
        if (!hasSpawned) {
            hasSpawned = true;
            GameObject newBorn = Instantiate(spawnObject);
            newBorn.transform.position = transform.position;
            newBorn.transform.forward = transform.forward;
            StartCoroutine(WaitForSpawn());
        }
    }

    private IEnumerator WaitForSpawn() {
        yield return new WaitForSeconds(timeBetweenSpawns);
        hasSpawned = false;
    }
}

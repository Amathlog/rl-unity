using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGenerator : MonoBehaviour {

    // Keep the transform of each spawn points
    private List<SpawnPoint> humanSpawnPoints = null;
    private List<SpawnPoint> natureSpawnPoints = null;

    // Keep the prefabs that will be spawn
    [SerializeField] private GameObject[] humansGameObjects;
    [SerializeField] private GameObject[] natureGameObjects;

    private void Start() {
        RetriveSpawnPoints();
    }

    private void FixedUpdate() {
        if(humanSpawnPoints != null)
            foreach(SpawnPoint sp in humanSpawnPoints) {
                sp.Spawn(humansGameObjects[Random.Range(0, humansGameObjects.Length - 1)]);
            }
        if (natureSpawnPoints != null)
            foreach (SpawnPoint sp in natureSpawnPoints) {
                sp.Spawn(natureGameObjects[Random.Range(0, natureGameObjects.Length - 1)]);
            }
    }

    private void RetriveSpawnPoints() {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child.gameObject.name.Equals("HumanSpawns")) {
                humanSpawnPoints = new List<SpawnPoint>(child.GetComponentsInChildren<SpawnPoint>());
            } else if (child.gameObject.name.Equals("NatureSpawns")) {
                natureSpawnPoints = new List<SpawnPoint>(child.GetComponentsInChildren<SpawnPoint>());
            }
        }
    }
}

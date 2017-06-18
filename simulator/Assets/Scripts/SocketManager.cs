using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketManager : MonoBehaviour {

    private Socket socketInstance;
    private bool firstSending = true;

	// Use this for initialization
	void Start () {
        Debug.Log("New scene loaded... Getting the env");
        firstSending = true;
        socketInstance = Socket.Instance;
        socketInstance.SetupEnv(GameObject.Find("Env").GetComponent<Environment>());
    }

    // Update is called once per frame
    void Update() {
        if (socketInstance.IsTcpClientOk()) {
            if (socketInstance.GetT() == socketInstance.GetSkipFirst()) {
                //print("init send at t=" + t);
                socketInstance.Send(); // initial observation
            } else if (socketInstance.GetT() > socketInstance.GetSkipFirst()) {
                //print("interact");
                if (firstSending) {
                    Debug.Log("Receiving for the first time");
                }
                socketInstance.Receive();
                if (firstSending) {
                    Debug.Log("Sending for the first time");
                }
                socketInstance.Send();
            }

            socketInstance.IncrT();
            firstSending = false;
        }
    }

    void OnApplicationQuit() {
        socketInstance.Close();
    }
}

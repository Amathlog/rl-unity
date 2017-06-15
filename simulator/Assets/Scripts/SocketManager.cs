using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketManager : MonoBehaviour {

    private Socket socketInstance;

	// Use this for initialization
	void Start () {
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
                socketInstance.Receive();
                socketInstance.Send();
            }

            socketInstance.IncrT();
        }
    }

    void OnApplicationQuit() {
        socketInstance.Close();
    }
}

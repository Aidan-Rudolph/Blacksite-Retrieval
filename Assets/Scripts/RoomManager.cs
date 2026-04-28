using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks {
    public GameObject player;
    public Transform[] spawnPoints;
    void Start() {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() {
        base.OnJoinedLobby();
        PhotonNetwork.JoinOrCreateRoom("Development", null, null);
    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();

        GameObject photon_player = PhotonNetwork.Instantiate(player.name, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
        photon_player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().enabled = true;
        photon_player.transform.GetChild(0).gameObject.SetActive(true);
    }
}

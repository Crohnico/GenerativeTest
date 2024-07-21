using PlayerX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Conection : NetworkBehaviour
{
    public GameObject player;
    public Camera camera;

    public PX_Camera cameraScript;
    public NetworkObject NObject;

    public void Initialize()
    {
        Camera _camera = null;
        NObject = GetComponent<NetworkObject>();

        transform.position = new Vector3(1500 / 2, (100 + 100 / 2) + 10, 1500 / 2);
        GameObject user = Instantiate(player, transform);

        _camera = Instantiate(camera, transform);
        cameraScript = gameObject.GetComponentInChildren<PX_Camera>();
        cameraScript.Initialize(_camera);
        _camera.gameObject.SetActive(NObject.IsOwner);


        GridChunks.Istance.Initialize(user, _camera, NObject.IsOwner);
    }

    private void Start()
    {
        //   Initialize();
    }

    public override void OnNetworkSpawn()
    {
        Initialize();
        base.OnNetworkSpawn();
    }
}

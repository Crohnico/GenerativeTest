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

    private NetworkVariable<int> m_SomeValue = new NetworkVariable<int>();

    public void Initialize()
    {
        Camera _camera = null;
        NObject = GetComponent<NetworkObject>();
        if (NObject.IsOwner)
        {
            _camera = Instantiate(camera, transform);
        }

        GridChunks.Istance.Initialize(gameObject, _camera, NObject.IsOwner);


        if (IsHost)
        {
            var randomValue = Random.Range(0, 1000);
            m_SomeValue.Value = randomValue;
            Debug.Log(randomValue);
        }
        else
        {
            Debug.Log(m_SomeValue.Value);
        }


        cameraScript = gameObject.GetComponentInChildren<PX_Camera>();
        cameraScript.Initialize(_camera);
        //_camera.gameObject.SetActive(NObject.IsOwner);


        player.SetActive(true);
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

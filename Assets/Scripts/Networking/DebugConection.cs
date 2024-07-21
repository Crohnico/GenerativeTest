using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;

public class RelayManager : MonoBehaviour
{
    public GameObject[] todaslascosas;
    public TMP_Text tplayerID;
    private string playerID;
    public int port = 7777;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerID = AuthenticationService.Instance.PlayerId;
        Debug.Log($"Signed in. Player ID: {playerID}");
        tplayerID.text = playerID;
    }

    public async void Host(TMP_Text text)
    {
        try
        {
            string result = await StartHostWithRelay(5);
            text.text = result;
            foreach (GameObject cosas in todaslascosas)
            {
                cosas.SetActive(false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error hosting game: {ex.Message}");
        }
    }

    public async void Join(TMP_InputField text)
    {
        try
        {
            bool result = await StartClientWithRelay(text.text);
            if (result)
            {
                foreach (GameObject cosas in todaslascosas)
                {
                    cosas.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Failed to join game with provided join code.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error joining game: {ex.Message}");
        }
    }

    public async Task<string> StartHostWithRelay(int maxConnections = 5)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }


    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        bool startClientResult = NetworkManager.Singleton.StartClient();
        Debug.Log($"Client started: {startClientResult}, Join Code: {joinCode}, Player ID: {AuthenticationService.Instance.PlayerId}");
        return !string.IsNullOrEmpty(joinCode) && startClientResult;
    }

    public void OnHostButtonClicked()
    {
        TMP_Text hostText = GameObject.Find("HostText").GetComponent<TMP_Text>(); // Asegúrate de asignar el nombre correcto del objeto TMP_Text en tu escena
        Host(hostText);
    }
}
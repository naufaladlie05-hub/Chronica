using UnityEngine;
using Mirror;

public class UIManager : MonoBehaviour
{
    public GameObject canvasHost;
    public GameObject canvasClient;

    void Update()
    {
        // skip kalo network belum ready
        if (!NetworkClient.active && !NetworkServer.active) return;

        // setup UI buat host (proyektor)
        if (NetworkServer.active)
        {
            canvasHost.SetActive(true);
            canvasClient.SetActive(false);
        }
        // setup UI buat client (hp player)
        else if (NetworkClient.active && !NetworkServer.active)
        {
            canvasHost.SetActive(false);
            canvasClient.SetActive(true);
        }
    }
}
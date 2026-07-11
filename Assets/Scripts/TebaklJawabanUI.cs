using UnityEngine;
using Mirror;

public class TebakJawabanUI : MonoBehaviour
{
    public GameObject panelTebakJawaban;
    public GameObject panelTungguHasil;

    // dipanggil dari event onClick button unity, pass index opsinya ke sini
    public void KlikOpsi(int indexOpsi)
    {
        if (NetworkClient.localPlayer == null) return;

        // ambil komponen player lokal trus send tebakan ke server
        PlayerSetup playerKita = NetworkClient.localPlayer.GetComponent<PlayerSetup>();
        playerKita.CmdKirimTebakan(indexOpsi);

        // switch UI: hide panel tebak, show waiting screen
        panelTebakJawaban.SetActive(false);
        if (panelTungguHasil != null) panelTungguHasil.SetActive(true);
    }
}
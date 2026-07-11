using UnityEngine;
using TMPro;
using Mirror;

public class JoinGameUI : MonoBehaviour
{
    public TMP_InputField inputNama; 
    public GameObject formJoin;      // form uinya (buat di-hide abis join)

    public void KlikJoin()
    {
        // pastiin client udah beneran konek ke host
        if (NetworkClient.localPlayer == null) return;

        string namaPemain = inputNama.text;

        // cegah input nama kosong
        if (string.IsNullOrWhiteSpace(namaPemain)) return;

        // 1. get local player trus push nama ke server
        PlayerSetup playerKita = NetworkClient.localPlayer.GetComponent<PlayerSetup>();
        playerKita.CmdSendName(namaPemain);

        // 2. hide form join biar ga di-spam click (hindari bug duplicate name)
        formJoin.SetActive(false);

        // 3. switch UI hp ke waiting screen
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            // matiin lobby panel
            if (gm.panelLobby != null) gm.panelLobby.SetActive(false);

            // tampilin panel tunggu dan update teksnya
            if (gm.panelTunggu != null)
            {
                gm.panelTunggu.SetActive(true);
                TMP_Text teksTunggu = gm.panelTunggu.GetComponentInChildren<TMP_Text>();
                if (teksTunggu != null)
                {
                    teksTunggu.text = "Menunggu permainan dimulai...";
                }
            }
        }
    }
}
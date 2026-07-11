using UnityEngine;
using TMPro;
using Mirror;

public class SubmitAnswerUI : MonoBehaviour
{
    public TMP_InputField inputJawaban;
    public GameObject panelKetikBohong;
    public GameObject panelTunggu;

    public void KlikKirim()
    {
        if (NetworkClient.localPlayer == null) return;

        string jawaban = inputJawaban.text;

        // cegah player ngirim input kosong
        if (string.IsNullOrWhiteSpace(jawaban)) return;

        // get komponen local player buat nge-push jawaban palsu ke server
        PlayerSetup playerKita = NetworkClient.localPlayer.GetComponent<PlayerSetup>();
        playerKita.CmdKirimJawabanBohong(jawaban);

        // switch panel dari input form ke waiting screen
        panelKetikBohong.SetActive(false);
        if (panelTunggu != null) panelTunggu.SetActive(true);
    }
}
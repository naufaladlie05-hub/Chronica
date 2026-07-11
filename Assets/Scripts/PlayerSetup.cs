using UnityEngine;
using Mirror;
using TMPro;

public class PlayerSetup : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNameUpdated))]
    public string playerName = "Menunggu...";

    [SyncVar]
    public string jawabanBohong = "";

    [SyncVar]
    public int tebakanPilihan = -1;

    [SyncVar]
    public int skorTotal = 0;

    [SyncVar]
    public int jumlahMenipu = 0;

    [SyncVar]
    public int jumlahTertipu = 0;

    [Command]
    public void CmdSendName(string nameInput)
    {
        playerName = nameInput;

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            // add ke list player kalo belum kecatat
            if (!gm.daftarPemainTetap.Contains(this))
            {
                gm.daftarPemainTetap.Add(this);
            }
            gm.UpdateUILobby();
        }
    }

    void OnNameUpdated(string oldName, string newName)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            if (!gm.daftarPemainTetap.Contains(this))
            {
                gm.daftarPemainTetap.Add(this);
            }
            gm.UpdateUILobby();
        }
    }

    [Command]
    public void CmdKirimJawabanBohong(string jawaban)
    {
        // auto uppercase input dari client
        jawaban = jawaban.ToUpper();

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        // validasi: tolak kalo inputnya sama persis kyk jawaban asli
        if (jawaban == gm.bankSoal[gm.soalSaatIni].jawabanAsli)
        {
            TargetJawabanDitolak("Itu jawaban aslinya! Cari kebohongan lain.");
            return;
        }

        // validasi: cek duplikat biar ga ada kebohongan yg kembar
        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();
        foreach (var p in semuaPemain)
        {
            if (p != this && p.jawabanBohong == jawaban)
            {
                TargetJawabanDitolak("Sudah dipakai pemain lain! Cari yg beda.");
                return;
            }
        }

        // lolos validasi, save jawaban & cek progress
        jawabanBohong = jawaban;
        Debug.Log($"[SERVER] Pemain {playerName} mengirim kebohongan: {jawabanBohong}");
        gm.CekSemuaSudahJawab();
    }

    [Command]
    public void CmdKirimTebakan(int indexPilihan)
    {
        tebakanPilihan = indexPilihan;
        Debug.Log($"[SERVER] Pemain {playerName} menebak opsi index: {indexPilihan}");

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.CekSemuaSudahNebak();
    }

    // paksa client balik ke panel input kalo jawaban ditolak server
    [TargetRpc]
    public void TargetJawabanDitolak(string pesanPeringatan)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null && gm.panelKetikBohong != null)
        {
            // balikin UI dari waiting screen ke form input
            gm.panelTunggu.SetActive(false);
            gm.panelKetikBohong.SetActive(true);

            // clear input & set placeholder pake pesan error
            TMP_InputField input = gm.panelKetikBohong.GetComponentInChildren<TMP_InputField>();
            if (input != null)
            {
                input.text = "";
                TextMeshProUGUI placeholder = (TextMeshProUGUI)input.placeholder;
                if (placeholder != null) placeholder.text = pesanPeringatan;
            }
        }
    }

    [Server]
    public void ResetDataUntukSoalBaru()
    {
        jawabanBohong = "";
        tebakanPilihan = -1;
    }

    [TargetRpc]
    public void TargetTampilkanHasilDiHP(string pesan)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.UbahLayarClientKeHasil(pesan);
    }

    [Server]
    public void ResetTotalData()
    {
        skorTotal = 0;
        jumlahMenipu = 0;
        jumlahTertipu = 0;
        ResetDataUntukSoalBaru();
    }
}
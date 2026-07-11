using UnityEngine;
using Mirror;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[System.Serializable]
public class PertanyaanFibbage
{
    [TextArea(3, 5)]
    public string teksSoal;
    public string jawabanAsli;
}

// [NOTE] 
// Script utama, lumayan panjang, acak-acak. File ini handle state buat host & client.
// Next time dipisah UI manager sama Game manager pokoknya dirapihin lagi kalo sempet :D.

public class GameManager : NetworkBehaviour
{
    [Header("Database Soal")]
    public List<PertanyaanFibbage> bankSoal = new List<PertanyaanFibbage>(); // tempat nyimpen semua soal hasil load dari file csv
    public int soalSaatIni = 0; // tracker index buat tau sekarang lagi mainin soal ke-berapa

    [Header("Aset Ikon Master")]
    public Sprite[] asetIkonPemain; // isi 5 gambar icon di inspector berurutan (disamain sama urutan masuk room)

    [Header("UI Host - Fase Buka Jawaban (Visual)")]
    public GameObject wadahPemilihReveal; // parent obj yg nampilin siapa aja player yg milih opsi tsb
    public GameObject[] slotPemilihReveal; // wadah ui per-playernya
    public UnityEngine.UI.Image[] ikonPemilihReveal; // target image buat render avatar player yg milih
    public TMP_Text[] teksNamaPemilihReveal; // target text buat render nama playernya

    [Header("UI Host - Fase Skor (Visual Horizontal)")]
    public GameObject wadahRekapSkor; // parent UI leaderboard sementara
    public GameObject[] slotRekapSkor; // slot ui buat masing-masing posisi rank
    public TMP_Text[] teksPosisiRekap; // tulisan rank (cth: #1, #2, dst)
    public UnityEngine.UI.Image[] ikonRekap; // icon player di leaderboard
    public TMP_Text[] teksNamaRekap; // nama player di leaderboard
    public TMP_Text[] teksPoinRekap; // teks poin yg nanti bakal di-animasiin angkanya nambah (coroutine)

    // --- SETUP UI LOBBY ---
    [Header("UI Host - Lobi Visual Baru")]
    public TMP_Text teksTotalPemain;     // format dinamis: "Total Pemain: X/5"
    public GameObject wadahSlotPemain;   // container utama buat 5 slot standby di lobby
    public GameObject[] slotPemainLobby; // array obj slotnya (di-on/off sesuai jumlah player yg masuk)
    public TMP_Text[] teksNamaSlotLobby; // text nama di bawah icon standby

    // list player aktif/permanen (ibarat buku absen selama game jalan)
    public List<PlayerSetup> daftarPemainTetap = new List<PlayerSetup>();

    public GameObject tombolStartGame; // button play yg cuma bisa ditekan host
    public GameObject panelQRLobby; // panel UI yg ada gambar qr code-nya

    [Header("UI Host - Fase Soal")]
    public GameObject teksSoalObj; // wadah panel teks soal di layar proyektor
    public TMP_Text teksSoalTampil; // tempat nge-set string pertanyaannya
    public GameObject panelPilihanGanda; // parent buat tombol opsi-opsi jawaban (A,B,C, dst)
    public GameObject tombolTampilkanPilihan; // button "Tampilkan Pilihan" (nunggu semua player kelar ngetik)
    public TMP_Text[] teksOpsiHost; // teks opsi A, B, C, dst di layar host

    [Header("UI Host - Fase Buka Jawaban")]
    public GameObject tombolTampilkanHasil; // button "Tampilkan Hasil" (nunggu semua player kelar milih)
    public GameObject panelReveal; // panel utama buat fase pembukaan jawaban
    public TMP_Text teksRevealUtama; // text besar buat nampilin isi jawabannya apa
    public TMP_Text teksRevealDetail; // text kecil buat info (cth: "Dipilih oleh:" atau "TERTIPU!")

    [Header("UI Host - Tambahan Reveal & Animasi")]
    public UnityEngine.UI.Image latarReveal; // background warna (nanti otomatis ganti merah kalo itu jawaban bohong)
    public GameObject tombolBackLobby; // tombol darurat buat back ke lobby kalo ada bug

    [Header("UI Host - Fase Skor Akhir")]
    public GameObject panelHasil; // panel rekap skor sebelum masuk ke soal berikutnya
    public TMP_Text teksJawabanBenar; // text judul rekapnya
    public TMP_Text teksRekapSkor; // text tambahan (dikosongin aja buat jaga-jaga)
    public GameObject tombolLanjut; // button next soal
    public GameObject tombolRestart; // button restart game pas soal udah abis (tamat)

    [Header("UI Host - Fase HASIL AKHIR (Game Tamat)")]
    public GameObject panelHasilAkhir; // panel khusus pas semua soal di csv udah abis
    public CanvasGroup grupShowcaseAkhir; // komponen alpha buat efek fade in/out pas nampilin stat tiap player
    public UnityEngine.UI.Image ikonShowcaseAkhir; // gambar player yg lagi disorot (showcase)
    public TMP_Text teksNamaPoinAkhir; // text nama & skor total akhir
    public TMP_Text teksStatistikShowcase; // text stat nipu brp kali, ketipu brp kali
    public TMP_Text teksHonorableMention; // text gelar lucu-lucuan (cth: "Sang Penipu Handal")

    [Header("UI Client (HP)")]
    public GameObject panelLobby; // layar tunggu awal di hp pas baru join
    public GameObject panelKetikBohong; // form input keyboard buat ngetik kebohongan
    public GameObject panelTunggu; // layar idle "tunggu host / nunggu pemain lain"
    public GameObject panelTebakJawaban; // layar hp pas disuruh milih opsi A, B, C
    public GameObject[] tombolOpsiClient; // tombol A B C D di hp
    public GameObject panelTungguHasil; // layar hp nunggu host buka satu-satu jawaban
    public GameObject panelInfoHasil; // layar hp nampilin status dia bener/salah
    public TMP_Text teksInfoHasilClient; // text status umum di hp

    [Header("UI Client (HP) - Hasil Detail Baru")]
    public TMP_Text teksStatusTebakanClient; // text nampilin "BENAR!" atau "SALAH!"
    public TMP_Text teksPoinTebakanClient; // nampilin dapet +1000 atau +0

    public GameObject wadahKorbanClient; // panel info kalo tipuan dia dimakan orang
    public TMP_Text teksInfoKorbanClient; // text "Tipuanmu mengecoh 2 orang"
    public TMP_Text teksPoinNipuClient; // poin dari hasil nipu (jumlah korban x 500)

    public GameObject[] slotKorbanClient; // slot ui korban di hp si penipu
    public UnityEngine.UI.Image[] ikonKorbanClient; // icon korban di hp penipu
    public TMP_Text[] teksNamaKorbanClient; // nama korban di hp penipu

    // list temporary buat nyimpen urutan jawaban yg lagi dirender di opsi ganda
    public List<string> listJawabanTampil = new List<string>();

    // --- FUNGSI UPDATE UI LOBI BARU ---
    public void UpdateUILobby()
    {
        // clean up null reference (kalo ada player yg tiba2 diskonek/keluar)
        daftarPemainTetap.RemoveAll(item => item == null);

        if (teksTotalPemain != null)
            teksTotalPemain.text = $"Total Pemain: {daftarPemainTetap.Count}/5";

        if (slotPemainLobby != null && teksNamaSlotLobby != null)
        {
            for (int i = 0; i < slotPemainLobby.Length; i++)
            {
                if (i < daftarPemainTetap.Count)
                {
                    slotPemainLobby[i].SetActive(true);
                    teksNamaSlotLobby[i].text = daftarPemainTetap[i].playerName;
                }
                else
                {
                    slotPemainLobby[i].SetActive(false);
                }
            }
        }
    }

    public void KlikKickPemain(int indexSlot)
    {
        // safety check, cuma server yg boleh kick
        if (!isServer) return;

        daftarPemainTetap.RemoveAll(item => item == null);

        if (indexSlot < daftarPemainTetap.Count)
        {
            PlayerSetup pemainYangDiKick = daftarPemainTetap[indexSlot];

            if (pemainYangDiKick != null)
            {
                if (pemainYangDiKick.connectionToClient != null)
                {
                    // putus koneksi client
                    pemainYangDiKick.connectionToClient.Disconnect();
                }
                NetworkServer.Destroy(pemainYangDiKick.gameObject);

                // hapus dari list permanen
                daftarPemainTetap.Remove(pemainYangDiKick);

                Invoke(nameof(UpdateUILobby), 0.5f);
            }
        }
    }

    public void KlikMulaiGame()
    {
        if (!isServer) return;

        AudioManager.instance.PlayBGM(AudioManager.instance.bgmInGame);
        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();

        // minimal 2 player buat main biar logic gamenya jalan
        if (semuaPemain.Length < 2) return;

        MuatSoalDariCSV();

        // cegah error kalo csv kosong
        if (bankSoal.Count == 0) return;

        // matiin ui lobby
        if (teksTotalPemain != null) teksTotalPemain.gameObject.SetActive(false);
        if (wadahSlotPemain != null) wadahSlotPemain.SetActive(false);
        tombolStartGame.SetActive(false);
        if (panelQRLobby != null) panelQRLobby.SetActive(false);

        // tampilin soal pertama
        teksSoalObj.SetActive(true);
        teksSoalTampil.text = bankSoal[soalSaatIni].teksSoal;

        if (tombolTampilkanPilihan != null)
        {
            tombolTampilkanPilihan.SetActive(true);
            // set text button buat indikator jumlah player yg udah jawab
            tombolTampilkanPilihan.GetComponentInChildren<TMP_Text>().text = $"0/{semuaPemain.Length}";
        }

        tombolTampilkanHasil.SetActive(false);
        panelHasil.SetActive(false);

        RpcPindahKeLayarKetik();
    }

    [ClientRpc]
    void RpcPindahKeLayarKetik()
    {
        // switch ui di semua hp client ke input form
        if (panelTunggu != null) panelTunggu.SetActive(false);
        if (panelTebakJawaban != null) panelTebakJawaban.SetActive(false);
        if (panelLobby != null) panelLobby.SetActive(false);
        if (panelTungguHasil != null) panelTungguHasil.SetActive(false);
        if (panelInfoHasil != null) panelInfoHasil.SetActive(false);

        if (panelKetikBohong != null)
        {
            panelKetikBohong.SetActive(true);
            TMP_InputField inputField = panelKetikBohong.GetComponentInChildren<TMP_InputField>();
            // clear inputan lama
            if (inputField != null) inputField.text = "";
        }
    }

    public void CekSemuaSudahJawab()
    {
        if (!isServer) return;
        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();
        int jumlahPemain = semuaPemain.Length;
        int jumlahSudahJawab = semuaPemain.Count(p => !string.IsNullOrWhiteSpace(p.jawabanBohong));

        // update counter text di button host
        if (tombolTampilkanPilihan != null)
        {
            tombolTampilkanPilihan.GetComponentInChildren<TMP_Text>().text = $"{jumlahSudahJawab}/{jumlahPemain}";
        }
    }

    public void KlikTampilkanPilihan()
    {
        if (!isServer) return;

        listJawabanTampil.Clear();
        // masukin jawaban asli dulu
        listJawabanTampil.Add(bankSoal[soalSaatIni].jawabanAsli);

        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();

        // masukin jawaban bohong player yg lolos validasi (ga duplikat/kosong)
        foreach (var p in semuaPemain)
        {
            if (!string.IsNullOrWhiteSpace(p.jawabanBohong) && !listJawabanTampil.Contains(p.jawabanBohong))
                listJawabanTampil.Add(p.jawabanBohong);
        }

        // shuffle posisi jawaban biar ga ketebak mulu
        System.Random rng = new System.Random();
        listJawabanTampil = listJawabanTampil.OrderBy(a => rng.Next()).ToList();

        panelPilihanGanda.SetActive(true);
        tombolTampilkanPilihan.SetActive(false);

        // render opsi ke ui host
        for (int i = 0; i < teksOpsiHost.Length; i++)
        {
            if (i < listJawabanTampil.Count)
            {
                teksOpsiHost[i].transform.parent.gameObject.SetActive(true);
                char hurufOpsi = (char)(65 + i); // convert 0 ke A, 1 ke B, dst
                teksOpsiHost[i].text = hurufOpsi + ". " + listJawabanTampil[i];
            }
            else
            {
                teksOpsiHost[i].transform.parent.gameObject.SetActive(false);
            }
        }

        if (tombolTampilkanHasil != null)
        {
            tombolTampilkanHasil.SetActive(true);
            tombolTampilkanHasil.GetComponentInChildren<TMP_Text>().text = $"0/{semuaPemain.Length}";
        }

        // 1. force client buat show panel & reset button interaksi
        RpcPindahKeLayarTebak(listJawabanTampil.Count);

        // 2. lock button opsi punya dia sendiri biar ga self-vote (curang)
        foreach (var p in semuaPemain)
        {
            int indexSendiri = listJawabanTampil.IndexOf(p.jawabanBohong);
            if (indexSendiri != -1) // pastiin jawabannya ada di list
            {
                // send command khusus ke client ini doang
                TargetDisableOpsiSendiri(p.connectionToClient, indexSendiri);
            }
        }
    }

    [ClientRpc]
    void RpcPindahKeLayarTebak(int jumlahOpsi)
    {
        if (panelTunggu != null) panelTunggu.SetActive(false);
        if (panelTebakJawaban != null) panelTebakJawaban.SetActive(true);

        for (int i = 0; i < tombolOpsiClient.Length; i++)
        {
            if (i < jumlahOpsi)
            {
                tombolOpsiClient[i].SetActive(true);

                // RESET BUTTON: enable interactable buat ronde baru
                UnityEngine.UI.Button btn = tombolOpsiClient[i].GetComponent<UnityEngine.UI.Button>();
                if (btn != null) btn.interactable = true;

                // RESET TEXT: balikin ke format huruf abjad A, B, C, dst
                TMP_Text teksTombol = tombolOpsiClient[i].GetComponentInChildren<TMP_Text>();
                if (teksTombol != null)
                {
                    char huruf = (char)(65 + i);
                    teksTombol.text = huruf.ToString();
                }
            }
            else
            {
                tombolOpsiClient[i].SetActive(false);
            }
        }
    }

    // TargetRpc: cuma dijalanin di client spesifik, ga broadcast ke semua orang
    [TargetRpc]
    public void TargetDisableOpsiSendiri(NetworkConnection target, int indexTerlarang)
    {
        if (tombolOpsiClient != null && indexTerlarang >= 0 && indexTerlarang < tombolOpsiClient.Length)
        {
            GameObject tombolLarangan = tombolOpsiClient[indexTerlarang];

            // 1. disable button
            UnityEngine.UI.Button btn = tombolLarangan.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.interactable = false;

            // 2. ganti text jadi X buat tanda aja biar keliatan beda
            TMP_Text teksTombol = tombolLarangan.GetComponentInChildren<TMP_Text>();
            if (teksTombol != null) teksTombol.text = "X";
        }
    }

    public void CekSemuaSudahNebak()
    {
        if (!isServer) return;
        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();
        int jumlahPemain = semuaPemain.Length;
        int jumlahSudahNebak = semuaPemain.Count(p => p.tebakanPilihan != -1);

        if (tombolTampilkanHasil != null)
        {
            tombolTampilkanHasil.GetComponentInChildren<TMP_Text>().text = $"{jumlahSudahNebak}/{jumlahPemain}";
        }
    }

    // ini dipanggil pas button nampilin hasil di host diklik
    public void KlikTampilkanHasil()
    {
        if (!isServer) return;
        tombolTampilkanHasil.SetActive(false);
        StartCoroutine(ProsesRevealJawaban());
    }

    IEnumerator ProsesRevealJawaban()
    {
        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();
        string jawabanBenar = bankSoal[soalSaatIni].jawabanAsli;

        if (tombolBackLobby != null) tombolBackLobby.SetActive(false);
        panelPilihanGanda.SetActive(false);
        teksSoalObj.SetActive(false);
        panelReveal.SetActive(true);
        if (wadahPemilihReveal != null) wadahPemilihReveal.SetActive(false);

        // simpen skor sementara sblm diupdate buat animasi UI
        Dictionary<PlayerSetup, int> skorSebelumnya = new Dictionary<PlayerSetup, int>();

        // --- DATA TRACKER BUAT CLIENT ---
        Dictionary<PlayerSetup, bool> tebakanBenarDict = new Dictionary<PlayerSetup, bool>();
        Dictionary<PlayerSetup, List<PlayerSetup>> korbanTipuanDict = new Dictionary<PlayerSetup, List<PlayerSetup>>();

        foreach (var p in semuaPemain)
        {
            skorSebelumnya[p] = p.skorTotal;
            tebakanBenarDict[p] = false; // defaultnya salah dulu
            korbanTipuanDict[p] = new List<PlayerSetup>(); // prepare list kosong
        }

        // rekap siapa aja yg milih opsi tertentu
        Dictionary<string, List<PlayerSetup>> rekapPilihan = new Dictionary<string, List<PlayerSetup>>();
        foreach (var p in semuaPemain)
        {
            if (p.tebakanPilihan != -1 && p.tebakanPilihan < listJawabanTampil.Count)
            {
                string jawabanDipilih = listJawabanTampil[p.tebakanPilihan];
                if (!rekapPilihan.ContainsKey(jawabanDipilih))
                    rekapPilihan[jawabanDipilih] = new List<PlayerSetup>();
                rekapPilihan[jawabanDipilih].Add(p);
            }
        }

        // warna bg dinamis
        Color32 warnaDefault = new Color32(0, 0, 120, 20);
        Color32 warnaMerah = new Color32(120, 0, 0, 20);

        // --- PHASE 1: REVEAL JAWABAN BOHONG ---
        foreach (var item in rekapPilihan)
        {
            string teksJawaban = item.Key;
            List<PlayerSetup> pemilih = item.Value;

            // skip kalo ini jawaban yg bener (direveal paling akhir)
            if (teksJawaban == jawabanBenar) continue;

            PlayerSetup pembohong = semuaPemain.FirstOrDefault(p => p.jawabanBohong == teksJawaban);

            if (latarReveal != null) latarReveal.color = warnaDefault;
            teksRevealUtama.text = $"Pilihan:\n<color=yellow>\"{teksJawaban}\"</color>";
            teksRevealDetail.text = "Dipilih oleh:\n\n\n\n";

            // tunggu bentar biar ngasih suspense efek
            yield return new WaitForSeconds(1.5f);

            if (wadahPemilihReveal != null) wadahPemilihReveal.SetActive(true);
            if (slotPemilihReveal != null)
            {
                foreach (var slot in slotPemilihReveal) { if (slot != null) slot.SetActive(false); }
            }

            // show avatar/nama player yg ketipu
            int slotIndex = 0;
            foreach (var p in pemilih)
            {
                int indexAsli = daftarPemainTetap.IndexOf(p);
                if (slotPemilihReveal != null && slotIndex < slotPemilihReveal.Length)
                {
                    if (slotPemilihReveal[slotIndex] != null) slotPemilihReveal[slotIndex].SetActive(true);
                    if (ikonPemilihReveal != null && slotIndex < ikonPemilihReveal.Length && indexAsli >= 0 && indexAsli < asetIkonPemain.Length)
                        ikonPemilihReveal[slotIndex].sprite = asetIkonPemain[indexAsli];
                    if (teksNamaPemilihReveal != null && slotIndex < teksNamaPemilihReveal.Length)
                        teksNamaPemilihReveal[slotIndex].text = p.playerName;

                    slotIndex++;
                    yield return new WaitForSeconds(0.7f);
                }
            }

            yield return new WaitForSeconds(1.2f);

            // kalo emang ada pembohongnya 
            if (pembohong != null)
            {
                if (latarReveal != null) latarReveal.color = warnaMerah;
                teksRevealDetail.text += $"TERTIPU! Ini karangan: <color=red>{pembohong.playerName}</color>!";

                // kalkulasi poin (500 x jumlah korban)
                pembohong.skorTotal += (500 * pemilih.Count);
                pembohong.jumlahMenipu += pemilih.Count;

                // save info korban buat dikirim ke hp penipu nanti
                foreach (var tertipu in pemilih)
                {
                    korbanTipuanDict[pembohong].Add(tertipu);
                    tertipu.jumlahTertipu += 1;
                }
            }
            yield return new WaitForSeconds(4f);
            if (wadahPemilihReveal != null) wadahPemilihReveal.SetActive(false);
        }

        // --- PHASE 2: REVEAL JAWABAN ASLI ---
        if (latarReveal != null) latarReveal.color = warnaDefault;

        teksRevealDetail.text = "";
        teksRevealUtama.text = "Jawaban yang benar adalah";
        yield return new WaitForSeconds(0.7f);
        teksRevealUtama.text = "Jawaban yang benar adalah .";
        yield return new WaitForSeconds(0.7f);
        teksRevealUtama.text = "Jawaban yang benar adalah . .";
        yield return new WaitForSeconds(0.7f);
        teksRevealUtama.text = "Jawaban yang benar adalah . . .";
        yield return new WaitForSeconds(1f);

        teksRevealUtama.text = $"<color=green>\"{jawabanBenar}\"</color>";

        if (rekapPilihan.ContainsKey(jawabanBenar))
        {
            List<PlayerSetup> pemilihBenar = rekapPilihan[jawabanBenar];

            teksRevealDetail.text = "Hebat! Ditebak oleh:\n\n\n\n(+1000 Poin)";
            yield return new WaitForSeconds(1.5f);

            if (wadahPemilihReveal != null) wadahPemilihReveal.SetActive(true);
            if (slotPemilihReveal != null)
            {
                foreach (var slot in slotPemilihReveal) { if (slot != null) slot.SetActive(false); }
            }

            int slotIndex = 0;
            foreach (var p in pemilihBenar)
            {
                // flag true buat client yg berhasil nebak bener
                tebakanBenarDict[p] = true;

                int indexAsli = daftarPemainTetap.IndexOf(p);
                if (slotPemilihReveal != null && slotIndex < slotPemilihReveal.Length)
                {
                    if (slotPemilihReveal[slotIndex] != null) slotPemilihReveal[slotIndex].SetActive(true);
                    if (ikonPemilihReveal != null && slotIndex < ikonPemilihReveal.Length && indexAsli >= 0 && indexAsli < asetIkonPemain.Length)
                        ikonPemilihReveal[slotIndex].sprite = asetIkonPemain[indexAsli];
                    if (teksNamaPemilihReveal != null && slotIndex < teksNamaPemilihReveal.Length)
                        teksNamaPemilihReveal[slotIndex].text = p.playerName;

                    slotIndex++;
                    yield return new WaitForSeconds(0.7f);
                }

                // tambah 1000 poin
                p.skorTotal += 1000;
            }
        }
        else
        {
            teksRevealDetail.text = "Sayang sekali, tidak ada yang menebak benar!";
        }

        yield return new WaitForSeconds(4f);
        if (wadahPemilihReveal != null) wadahPemilihReveal.SetActive(false);

        // --- SEND RESULT DATA KE MASING2 CLIENT ---
        foreach (var p in semuaPemain)
        {
            if (p.connectionToClient != null)
            {
                var listKorban = korbanTipuanDict[p];
                string[] namaKorbanArr = new string[listKorban.Count];
                int[] ikonKorbanArr = new int[listKorban.Count];

                for (int i = 0; i < listKorban.Count; i++)
                {
                    namaKorbanArr[i] = listKorban[i].playerName;
                    ikonKorbanArr[i] = daftarPemainTetap.IndexOf(listKorban[i]);
                }

                // push array data korban ke target client
                TargetBukaHasilClient(p.connectionToClient, tebakanBenarDict[p], namaKorbanArr, ikonKorbanArr);
            }
        }


        // --- PHASE 3: LEADERBOARD SEMENTARA ---
        panelReveal.SetActive(false);
        if (panelHasil != null) panelHasil.SetActive(true);

        if (teksJawabanBenar != null) teksJawabanBenar.text = "REKAP SKOR SEMENTARA";
        if (teksRekapSkor != null) teksRekapSkor.text = "";
        if (wadahRekapSkor != null) wadahRekapSkor.SetActive(true);

        // sort player by skor descending (rank 1 di awal)
        var pemainDiurutkan = semuaPemain.OrderByDescending(p => p.skorTotal).ToList();

        if (slotRekapSkor != null)
        {
            foreach (var slot in slotRekapSkor) { if (slot != null) slot.SetActive(false); }

            for (int i = 0; i < pemainDiurutkan.Count; i++)
            {
                var p = pemainDiurutkan[i];
                int indexAsli = daftarPemainTetap.IndexOf(p);
                int skorLama = skorSebelumnya.ContainsKey(p) ? skorSebelumnya[p] : 0;

                if (i < slotRekapSkor.Length && slotRekapSkor[i] != null)
                {
                    slotRekapSkor[i].SetActive(true);

                    if (teksPosisiRekap != null && i < teksPosisiRekap.Length && teksPosisiRekap[i] != null)
                        teksPosisiRekap[i].text = $"#{i + 1}";

                    if (teksNamaRekap != null && i < teksNamaRekap.Length && teksNamaRekap[i] != null)
                        teksNamaRekap[i].text = p.playerName;

                    // eksekusi coroutine poin numpuk
                    if (teksPoinRekap != null && i < teksPoinRekap.Length && teksPoinRekap[i] != null)
                        StartCoroutine(AnimasiAngkaNaik(teksPoinRekap[i], skorLama, p.skorTotal, 1.5f));

                    if (ikonRekap != null && i < ikonRekap.Length && ikonRekap[i] != null && indexAsli >= 0 && indexAsli < asetIkonPemain.Length)
                    {
                        ikonRekap[i].sprite = asetIkonPemain[indexAsli];
                    }
                }
            }
        }


        // --- PHASE 4: NEXT ROUND / ENDING ---
        if (soalSaatIni < bankSoal.Count - 1)
        {
            if (tombolLanjut != null)
            {
                var teksLanjut = tombolLanjut.GetComponentInChildren<TMP_Text>();
                if (teksLanjut != null) teksLanjut.text = "Soal Selanjutnya >>";
                tombolLanjut.SetActive(true);
            }
            if (tombolRestart != null) tombolRestart.SetActive(false);
        }
        else
        {
            // end game
            if (panelHasil != null) panelHasil.SetActive(false);
            if (panelHasilAkhir != null) panelHasilAkhir.SetActive(true);

            if (tombolLanjut != null) tombolLanjut.SetActive(false);
            if (tombolRestart != null) tombolRestart.SetActive(true);

            if (AudioManager.instance != null) AudioManager.instance.PlayBGM(AudioManager.instance.bgmVictory);

            RpcGameSelesai();

            // start loop showcase player
            StartCoroutine(LoopShowcaseAkhir(pemainDiurutkan));
        }
    }

    public void KlikLanjut()
    {
        if (!isServer) return;
        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();

        foreach (var p in semuaPemain) p.ResetDataUntukSoalBaru();
        soalSaatIni++;
        KlikMulaiGame();
    }

    public void KlikRestartGame()
    {
        if (!isServer) return;

        // 1. hard reset data & skor
        soalSaatIni = 0;
        PlayerSetup[] semuaPemain = FindObjectsOfType<PlayerSetup>();
        foreach (var p in semuaPemain) p.ResetTotalData();

        // 2. reset ui lobby di background
        UpdateUILobby();
        if (teksTotalPemain != null) teksTotalPemain.gameObject.SetActive(true);
        if (wadahSlotPemain != null) wadahSlotPemain.SetActive(true);
        if (panelQRLobby != null) panelQRLobby.SetActive(true);
        if (tombolStartGame != null) tombolStartGame.SetActive(true);

        // 3. hide sisa panel ingame
        if (panelHasilAkhir != null) panelHasilAkhir.SetActive(false);
        if (panelHasil != null) panelHasil.SetActive(false);
        if (teksSoalObj != null) teksSoalObj.SetActive(false);

        // 4. call main menu manager buat balik ke awal
        MainMenuManager menuManager = FindObjectOfType<MainMenuManager>();
        if (menuManager != null)
        {
            menuManager.KlikBackKeMenu(); // bgm ganti di dalem method ini
        }
        else
        {
            // fallback kalo MainMenuManager ga ada di scene
            if (AudioManager.instance != null) AudioManager.instance.PlayBGM(AudioManager.instance.bgmMainMenu);
        }

        // 5. force semua client balik ke waiting screen
        RpcPindahKeLobby();
    }

    [ClientRpc]
    void RpcGameSelesai()
    {
        if (panelInfoHasil != null) panelInfoHasil.SetActive(false);
        if (panelTunggu != null)
        {
            panelTunggu.SetActive(true);
            TMP_Text teksTunggu = panelTunggu.GetComponentInChildren<TMP_Text>();
            if (teksTunggu != null) teksTunggu.text = "Game Selesai!\nLihat proyektor untuk hasil akhir!";
        }
    }

    [ClientRpc]
    void RpcPindahKeLobby()
    {
        if (panelInfoHasil != null) panelInfoHasil.SetActive(false);
        if (panelKetikBohong != null) panelKetikBohong.SetActive(false);
        if (panelTebakJawaban != null) panelTebakJawaban.SetActive(false);

        if (panelLobby != null) panelLobby.SetActive(false);

        if (panelTunggu != null)
        {
            panelTunggu.SetActive(true);
            TMP_Text teksTunggu = panelTunggu.GetComponentInChildren<TMP_Text>();
            if (teksTunggu != null) teksTunggu.text = "Menunggu Host memulai ronde baru...";
        }
    }

    [TargetRpc]
    public void TargetBukaHasilClient(NetworkConnection target, bool tebakBenar, string[] namaKorban, int[] ikonKorbanIdx)
    {
        if (panelTungguHasil != null) panelTungguHasil.SetActive(false);
        if (panelTebakJawaban != null) panelTebakJawaban.SetActive(false);
        if (panelInfoHasil != null) panelInfoHasil.SetActive(true);

        // 1. setup text status tebakan
        if (tebakBenar)
        {
            if (teksStatusTebakanClient != null) teksStatusTebakanClient.text = "<color=green>Pilihanmu BENAR!</color>";
            if (teksPoinTebakanClient != null) teksPoinTebakanClient.text = "+1000 Poin";
        }
        else
        {
            if (teksStatusTebakanClient != null) teksStatusTebakanClient.text = "<color=red>Pilihanmu SALAH!</color>";
            if (teksPoinTebakanClient != null) teksPoinTebakanClient.text = "+0 Poin";
        }

        // 2. setup info nipu korban
        if (namaKorban.Length > 0)
        {
            if (wadahKorbanClient != null) wadahKorbanClient.SetActive(true);
            if (teksInfoKorbanClient != null) teksInfoKorbanClient.text = $"Tipuanmu mengecoh {namaKorban.Length} orang!";
            if (teksPoinNipuClient != null) teksPoinNipuClient.text = $"+{namaKorban.Length * 500} Poin";

            // reset & render icon korban
            if (slotKorbanClient != null)
            {
                foreach (var slot in slotKorbanClient) { if (slot != null) slot.SetActive(false); }

                for (int i = 0; i < namaKorban.Length; i++)
                {
                    if (i < slotKorbanClient.Length && slotKorbanClient[i] != null)
                    {
                        slotKorbanClient[i].SetActive(true);
                        if (teksNamaKorbanClient != null && i < teksNamaKorbanClient.Length)
                            teksNamaKorbanClient[i].text = namaKorban[i];

                        if (ikonKorbanClient != null && i < ikonKorbanClient.Length && ikonKorbanIdx[i] >= 0 && ikonKorbanIdx[i] < asetIkonPemain.Length)
                        {
                            ikonKorbanClient[i].sprite = asetIkonPemain[ikonKorbanIdx[i]];
                        }
                    }
                }
            }
        }
        else
        {
            // hide panel kalo ga dapet korban
            if (wadahKorbanClient != null) wadahKorbanClient.SetActive(false);
        }
    }


    public void UbahLayarClientKeHasil(string pesan)
    {
        if (panelTungguHasil != null) panelTungguHasil.SetActive(false);
        if (panelTebakJawaban != null) panelTebakJawaban.SetActive(false);

        if (panelInfoHasil != null) panelInfoHasil.SetActive(true);
        if (teksInfoHasilClient != null) teksInfoHasilClient.text = pesan;
    }

    public void MuatSoalDariCSV()
    {
        // streaming asset folder path
        string pathFolder = Application.streamingAssetsPath;
        string pathFile = Path.Combine(pathFolder, "Template_Soal_TebakFakta.csv");

        if (!Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);

        // bikin csv dummy kalo blm ada
        if (!File.Exists(pathFile))
        {
            string clueDanContoh = "TULIS SOAL DI BAWAH INI (Abaikan baris ini);TULIS JAWABAN BENAR DI BAWAH INI\n" +
                                   "Siapa tokoh yang mengetik teks proklamasi kemerdekaan Indonesia?;Sayuti Melik\n" +
                                   "apa nama ibukota provinsi kalimantan barat;pontianak\n" +
                                   "Apa nama ibukota provinsi Sulawesi Tengah?;Palu\n";

            File.WriteAllText(pathFile, clueDanContoh);
            Debug.Log("File Template CSV berhasil dibuat otomatis!");
        }

        string[] barisData = File.ReadAllLines(pathFile);

        bankSoal.Clear();

        // start dari 1 soalnya baris 0 itu cuma header csv
        for (int i = 1; i < barisData.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(barisData[i])) continue;

            string[] kolom = barisData[i].Split(';');

            if (kolom.Length >= 2)
            {
                PertanyaanFibbage soalBaru = new PertanyaanFibbage();
                soalBaru.teksSoal = kolom[0];
                soalBaru.jawabanAsli = kolom[1].ToUpper();
                bankSoal.Add(soalBaru);
            }
        }
        Debug.Log($"Berhasil memuat {bankSoal.Count} soal dari CSV!");
    }


    // --- ANIMASI COUNTER POIN ---
    IEnumerator AnimasiAngkaNaik(TMP_Text teksTarget, int nilaiAwal, int nilaiTujuan, float durasi)
    {
        // skip animasi kalo poinya ga nambah
        if (nilaiAwal == nilaiTujuan)
        {
            teksTarget.text = $"{nilaiTujuan} \nPoin";
            yield break;
        }

        float waktu = 0;
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            int nilaiSaatIni = (int)Mathf.Lerp(nilaiAwal, nilaiTujuan, waktu / durasi);
            teksTarget.text = $"{nilaiSaatIni} \nPoin";
            yield return null;
        }
        teksTarget.text = $"{nilaiTujuan} \nPoin";
    }


    // --- SHOWCASE ENDING LOOP ---
    IEnumerator LoopShowcaseAkhir(List<PlayerSetup> pemainDiurutkan)
    {
        if (grupShowcaseAkhir == null) yield break;

        // --- SISTEM PEMBAGIAN TITLES/GELAR (UNIQUE) ---
        Dictionary<PlayerSetup, string> daftarGelar = new Dictionary<PlayerSetup, string>();

        // clone list buat nandain sapa yg udah dapet gelar biar ga double
        List<PlayerSetup> sisaPemain = new List<PlayerSetup>(pemainDiurutkan);

        // 1. Gelar Sang Juara (Fix buat rank 1)
        if (sisaPemain.Count > 0)
        {
            daftarGelar[sisaPemain[0]] = "Sang Juara";
            sisaPemain.RemoveAt(0); // pop dari list nominasi
        }

        // 2. Gelar Penipu Handal (cari stat nipu tertinggi dari sisa player)
        if (sisaPemain.Count > 0)
        {
            PlayerSetup p = sisaPemain.OrderByDescending(x => x.jumlahMenipu).First();
            daftarGelar[p] = "Sang Penipu Handal";
            sisaPemain.Remove(p);
        }

        // 3. Gelar Korban Hoaks (cari stat tertipu tertinggi)
        if (sisaPemain.Count > 0)
        {
            PlayerSetup p = sisaPemain.OrderByDescending(x => x.jumlahTertipu).First();
            daftarGelar[p] = "Korban Hoaks";
            sisaPemain.Remove(p);
        }

        // 4. Gelar Si Jenius (cari yg akurasi tebakannya paling gede)
        if (sisaPemain.Count > 0)
        {
            PlayerSetup p = sisaPemain.OrderByDescending(x => (x.skorTotal - (x.jumlahMenipu * 500))).First();
            daftarGelar[p] = "Si Jenius";
            sisaPemain.Remove(p);
        }

        // 5. Gelar Tim Hore (buat sisa player terakhir)
        if (sisaPemain.Count > 0)
        {
            daftarGelar[sisaPemain[0]] = "si normal";
            sisaPemain.RemoveAt(0);
        }


        // --- LOOPING RENDER ---
        while (panelHasilAkhir.activeSelf)
        {
            for (int i = 0; i < pemainDiurutkan.Count; i++)
            {
                if (!panelHasilAkhir.activeSelf) yield break;

                var p = pemainDiurutkan[i];
                int indexAsli = daftarPemainTetap.IndexOf(p);

                // 1. set visual icon
                if (indexAsli >= 0 && indexAsli < asetIkonPemain.Length)
                {
                    ikonShowcaseAkhir.sprite = asetIkonPemain[indexAsli];
                }

                // 2. hitung akurasi jawaban bener
                int totalBenar = (p.skorTotal - (p.jumlahMenipu * 500)) / 1000;
                int totalSoal = bankSoal.Count;

                // 3. render text
                teksNamaPoinAkhir.text = $"#{i + 1} {p.playerName} - {p.skorTotal} Poin";

                // kasih enter (\n) biar rapi di UI
                teksStatistikShowcase.text = $"Berhasil Nipu {p.jumlahMenipu}x | Kena Tipu {p.jumlahTertipu}x\nJawaban Benar: {totalBenar}/{totalSoal} Soal";

                // 4. set text gelar
                string gelar = daftarGelar.ContainsKey(p) ? daftarGelar[p] : "Pemain Hebat";
                teksHonorableMention.text = $"\"{p.playerName} {gelar}\"";

                // 5. play animasi fade in-out
                yield return StartCoroutine(FadeCanvasGroup(grupShowcaseAkhir, 0f, 1f, 0.5f));
                yield return new WaitForSeconds(3.5f);
                yield return StartCoroutine(FadeCanvasGroup(grupShowcaseAkhir, 1f, 0f, 0.5f));
            }
        }
    }

    // helper coroutine buat efek transisi alpha / fade
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float waktu = 0;
        cg.alpha = startAlpha;
        while (waktu < duration)
        {
            waktu += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, waktu / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }

}
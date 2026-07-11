using UnityEngine;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Panel UI")]
    public GameObject panelMainMenu;
    public GameObject panelLobbyUtama;

    [Header("Panel Tambahan")]
    public GameObject panelHelp;   
    public GameObject panelCredit; 

    [Header("Pengaturan Tutorial (Help)")]
    public UnityEngine.UI.Image tempatGambarTutorial; 
    public Sprite[] halamanTutorial; 
    private int halamanSaatIni = 0;

    void Start()
    {
        // pastiin cuma main menu doang yg aktif pas awal start
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
        if (panelLobbyUtama != null) panelLobbyUtama.SetActive(false);
        if (panelHelp != null) panelHelp.SetActive(false);
        if (panelCredit != null) panelCredit.SetActive(false);
    }

    public void KlikPlay()
    {
        panelMainMenu.SetActive(false);
        panelLobbyUtama.SetActive(true);
        AudioManager.instance.PlayBGM(AudioManager.instance.bgmLobby);
    }

    public void KlikCustomSoal()
    {
        string pathFolder = Application.streamingAssetsPath;
        string pathFile = Path.Combine(pathFolder, "Template_Soal_TebakFakta.csv");

        // kalo file template csv blm ada, generate folder & file defaultnya
        if (!File.Exists(pathFile))
        {
            if (!Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);
            string clueDanContoh = "TULIS SOAL DI BAWAH INI (Abaikan baris ini);TULIS JAWABAN BENAR DI BAWAH INI\n" +
                                   "Siapa tokoh yang mengetik teks proklamasi kemerdekaan Indonesia?;Sayuti Melik\n" +
                                   "Apa nama ibukota provinsi Jawa Barat?;Bandung";
            File.WriteAllText(pathFile, clueDanContoh);
        }

        // open file csv-nya (biasanya bakal kebuka otomatis di excel/notepad)
        Application.OpenURL("file://" + pathFile);
    }

    // --- HELP / TUTORIAL ---
    public void KlikHelp()
    {
        // reset ke index 0 (slide awal) tiap kali panel help dibuka
        halamanSaatIni = 0;

        if (halamanTutorial != null && halamanTutorial.Length > 0 && tempatGambarTutorial != null)
        {
            tempatGambarTutorial.sprite = halamanTutorial[halamanSaatIni];
        }

        panelHelp.SetActive(true);
    }

    public void TutupHelp()
    {
        panelHelp.SetActive(false);
    }

    // --- NAVIGASI SLIDE ---
    public void KlikNextTutorial()
    {
        // next slide kalo index belum mentok di akhir array
        if (halamanTutorial != null && halamanSaatIni < halamanTutorial.Length - 1)
        {
            halamanSaatIni++;
            if (tempatGambarTutorial != null) tempatGambarTutorial.sprite = halamanTutorial[halamanSaatIni];

            if (AudioManager.instance != null) AudioManager.instance.PlayKlikSFX();
        }
    }

    public void KlikPrevTutorial()
    {
        // prev slide kalo index belum mentok di awal (0)
        if (halamanTutorial != null && halamanSaatIni > 0)
        {
            halamanSaatIni--;
            if (tempatGambarTutorial != null) tempatGambarTutorial.sprite = halamanTutorial[halamanSaatIni];

            if (AudioManager.instance != null) AudioManager.instance.PlayKlikSFX();
        }
    }

    public void KlikCredit()
    {
        panelCredit.SetActive(true);
    }

    public void TutupCredit()
    {
        panelCredit.SetActive(false);
    }

    public void KlikBackKeMenu()
    {
        if (panelLobbyUtama != null) panelLobbyUtama.SetActive(false);
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
        AudioManager.instance.PlayBGM(AudioManager.instance.bgmMainMenu);
    }

    public void KlikQuit()
    {
        Debug.Log("Game ditutup.");
        Application.Quit();
    }
}
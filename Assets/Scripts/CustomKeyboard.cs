using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomKeyboard : MonoBehaviour
{
    [Header("Hubungkan Wadah Baris dari Hierarchy")]
    public Transform baris1;
    public Transform baris2;
    public Transform baris3;
    public Transform baris4;

    [Header("UI Kontainer Keyboard (Objek yang akan hilang/timbul)")]
    public GameObject visualKeyboard; // parent object buat nampilin/hide keyboard

    [Header("Kolom Input Aktif Saat Ini (Otomatis)")]
    public TMP_InputField targetInput;

    private string[] susunanBaris1 = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };
    private string[] susunanBaris2 = { "A", "S", "D", "F", "G", "H", "J", "K", "L" };
    private string[] susunanBaris3 = { "Z", "X", "C", "V", "B", "N", "M" };

    void Start()
    {
        SetupTombolBaris(baris1, susunanBaris1);
        SetupTombolBaris(baris2, susunanBaris2);
        SetupTombolBaris(baris3, susunanBaris3);
        SetupBarisSpesial4(baris4);
    }

    void Update()
    {
        TMP_InputField inputAktifSaatIni = null;

        // scan semua input field di scene
        TMP_InputField[] semuaInputDiScene = FindObjectsOfType<TMP_InputField>(true);

        foreach (var input in semuaInputDiScene)
        {
            if (input.gameObject.activeInHierarchy)
            {
                inputAktifSaatIni = input;
                break;
            }
        }

        // auto toggle keyboard
        if (inputAktifSaatIni != null)
        {
            targetInput = inputAktifSaatIni;

            // show keyboard kalo ada input field yg lagi aktif/fokus
            if (visualKeyboard != null && !visualKeyboard.activeSelf)
            {
                visualKeyboard.SetActive(true);
            }
        }
        else
        {
            targetInput = null;

            // hide keyboard kalo ga ada input field yg aktif sama sekali
            if (visualKeyboard != null && visualKeyboard.activeSelf)
            {
                visualKeyboard.SetActive(false);
            }
        }
    }

    void SetupTombolBaris(Transform barisObj, string[] daftarHuruf)
    {
        Button[] daftarTombol = barisObj.GetComponentsInChildren<Button>();
        for (int i = 0; i < daftarTombol.Length; i++)
        {
            if (i >= daftarHuruf.Length) break;
            string huruf = daftarHuruf[i];
            Button tombol = daftarTombol[i];

            tombol.gameObject.name = "Tombol_" + huruf;
            TMP_Text komponenTeks = tombol.GetComponentInChildren<TMP_Text>();
            if (komponenTeks != null) komponenTeks.text = huruf;

            tombol.onClick.RemoveAllListeners();
            tombol.onClick.AddListener(() => KetikHuruf(huruf));
        }
    }

    void SetupBarisSpesial4(Transform barisObj)
    {
        Button[] daftarTombol = barisObj.GetComponentsInChildren<Button>();
        if (daftarTombol.Length >= 1)
        {
            Button tombolSpasi = daftarTombol[0];
            tombolSpasi.gameObject.name = "Tombol_Spasi";
            TMP_Text txt = tombolSpasi.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = "_____";
            tombolSpasi.onClick.RemoveAllListeners();
            tombolSpasi.onClick.AddListener(() => Spasi());
        }
        if (daftarTombol.Length >= 2)
        {
            Button tombolHapus = daftarTombol[1];
            tombolHapus.gameObject.name = "Tombol_Backspace";
            TMP_Text txt = tombolHapus.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = "<---";
            tombolHapus.onClick.RemoveAllListeners();
            tombolHapus.onClick.AddListener(() => HapusHuruf());
        }
    }

    public void KetikHuruf(string huruf)
    {
        if (targetInput != null) targetInput.text += huruf;
    }

    public void HapusHuruf()
    {
        if (targetInput != null && targetInput.text.Length > 0)
        {
            targetInput.text = targetInput.text.Substring(0, targetInput.text.Length - 1);
        }
    }

    public void Spasi()
    {
        if (targetInput != null) targetInput.text += " ";
    }
}
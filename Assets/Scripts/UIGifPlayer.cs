using UnityEngine;
using UnityEngine.UI;

public class UIGifPlayer : MonoBehaviour
{
    [Header("Komponen Image UI")]
    public Image targetImage;

    [Header("Kumpulan Frame Gambar")]
    public Sprite[] daftarFrame;

    [Header("Kecepatan (FPS)")]
    public float fps = 15f; // tweak aja fps-nya kalo animasi kerasa kelamaan/kecepetan

    void Update()
    {
        // safety check biar ga kena null reference kalo array atau image-nya kosong
        if (daftarFrame.Length == 0 || targetImage == null) return;

        // logic flipbook: kalkulasi index frame by waktu berjalan
        int indexFrame = (int)(Time.time * fps) % daftarFrame.Length;

        // update sprite ke frame sekarang
        targetImage.sprite = daftarFrame[indexFrame];
    }
}
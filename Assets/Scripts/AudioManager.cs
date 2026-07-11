using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Komponen Speaker")]
    public AudioSource speakerBGM;
    public AudioSource speakerSFX;

    [Header("Koleksi Audio - BGM")]
    public AudioClip bgmMainMenu;
    public AudioClip bgmLobby;
    public AudioClip bgmInGame;
    public AudioClip bgmVictory;

    [Header("Koleksi Audio - SFX")]
    public AudioClip sfxKlik;

    [Header("Pengaturan UI Mute")]
    public Image ikonTombolAudio; 
    public Sprite spriteUnmute;
    public Sprite spriteMute;
    private bool isMuted = false;

    void Awake()
    {
        // setup singleton simpel
        if (instance == null) instance = this;
    }

    void Start()
    {
        // matiin BGM kalo di client (hp/webgl) biar ga dobel suaranya, host pc aja yg bunyi
        if (Application.isMobilePlatform || Application.platform == RuntimePlatform.WebGLPlayer)
        {
            speakerBGM.mute = true;
        }

        PlayBGM(bgmMainMenu);
        UpdateGambarIkon(); // set gambar default pas awal mulai
    }

    // assign ke onClick button toggle
    public void ToggleAudio()
    {
        isMuted = !isMuted;

        speakerBGM.mute = isMuted;
        speakerSFX.mute = isMuted;

        UpdateGambarIkon();
    }

    private void UpdateGambarIkon()
    {
        if (ikonTombolAudio != null)
        {
            if (isMuted) ikonTombolAudio.sprite = spriteMute;
            else ikonTombolAudio.sprite = spriteUnmute;
        }
    }

    public void PlayKlikSFX()
    {
        if (sfxKlik != null && !isMuted)
        {
            speakerSFX.PlayOneShot(sfxKlik);
        }
    }

    public void PlayBGM(AudioClip laguBaru)
    {
        if (laguBaru == null) return;

        // cegah lagu restart dari awal kalo clip yg mau diputar masih sama
        if (speakerBGM.clip == laguBaru) return;

        speakerBGM.Stop();
        speakerBGM.clip = laguBaru;
        speakerBGM.Play();
    }
}
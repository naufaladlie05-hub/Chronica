using UnityEngine;
using UnityEngine.UI; // akses RawImage
using TMPro;
using System.Net;
using System.Net.Sockets;
using ZXing; // library barcode/qr
using ZXing.QrCode;
using System.Net.NetworkInformation;

public class LobbyManager : MonoBehaviour
{
    public TMP_Text teksIPLaptop;
    public RawImage gambarQRCode; // target render buat qr
    public string ipLaptopSaatIni;

    void Start()
    {
        // ambil ip address lokal host
        ipLaptopSaatIni = DapatkanIPLocal();

        // set url game live server + param ip biar hp client bisa auto-connect
        string urlGame = "http://" + ipLaptopSaatIni + ":5500/?ip=" + ipLaptopSaatIni;

        // tampilin ke layar proyektor
        if (teksIPLaptop != null)
        {
            teksIPLaptop.text = "Scan untuk Join!\nAtau ketik: " + urlGame;
        }

        // generate qr dari url
        if (gambarQRCode != null)
        {
            gambarQRCode.texture = BikinQRCode(urlGame);
        }
    }

    // cari ip address fisik (bukan virtual)
    string DapatkanIPLocal()
    {
        string ipLokal = "127.0.0.1";
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            // skip interface mati, loopback, atau adapter virtual (vmware/vpn)
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                !ni.Description.ToLower().Contains("virtual") &&
                !ni.Description.ToLower().Contains("pseudo"))
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipLokal = ip.Address.ToString();
                        // kalo ketemu range ip lokal router biasa (192.168.x.x), langsung return aja
                        if (ipLokal.StartsWith("192.168"))
                        {
                            return ipLokal;
                        }
                    }
                }
            }
        }
        return ipLokal;
    }

    // generator qr pakai zxing
    Texture2D BikinQRCode(string url)
    {
        BarcodeWriter writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = 256,
                Width = 256,
                Margin = 1 // set margin 1 aja biar ga terlalu tebel border putihnya
            }
        };

        // encode string url jadi data warna
        Color32[] warnaPixel = writer.Write(url);

        // bikin texture kosong trus apply datanya
        Texture2D teksturQR = new Texture2D(256, 256);
        teksturQR.SetPixels32(warnaPixel);
        teksturQR.Apply();

        return teksturQR;
    }
}
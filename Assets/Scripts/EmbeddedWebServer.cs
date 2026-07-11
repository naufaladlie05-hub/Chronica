using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class EmbeddedWebServer : MonoBehaviour
{
    private HttpListener listener;
    private Thread serverThread;
    public int port = 5500;

    void Start()
    {
        // cuma run di host/pc, skip kalo di webgl
#if !UNITY_WEBGL
        string folderPath = Path.Combine(Application.streamingAssetsPath, "WebBuild");
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Folder WebBuild tidak ditemukan di StreamingAssets!");
            return;
        }

        string ipLokal = DapatkanIPLocal();

        listener = new HttpListener();
        // register prefix pakai ip lokal
        listener.Prefixes.Add($"http://{ipLokal}:{port}/");
        listener.Prefixes.Add($"http://127.0.0.1:{port}/"); // fallback buat testing lokal
        listener.Start();

        serverThread = new Thread(Listen);
        serverThread.Start();
        Debug.Log($"[Web Server Internal] Berhasil aktif di http://{ipLokal}:{port}");
#endif
    }

    private void Listen()
    {
        while (listener.IsListening)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception) { }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath.Substring(1);
        if (string.IsNullOrEmpty(filename)) filename = "index.html";

        string filePath = Path.Combine(Application.streamingAssetsPath, "WebBuild", filename);

        if (File.Exists(filePath))
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                context.Response.ContentType = GetMimeType(filePath);
                context.Response.ContentLength64 = fileBytes.Length;
                context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        context.Response.OutputStream.Close();
    }

    private string GetMimeType(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        if (ext == ".html") return "text/html";
        if (ext == ".js") return "application/javascript";
        if (ext == ".wasm") return "application/wasm";
        if (ext == ".data") return "application/octet-stream";
        if (ext == ".css") return "text/css";
        return "application/octet-stream";
    }

    string DapatkanIPLocal()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ipStr = ip.Address.ToString();
                        if (ipStr.StartsWith("192.168")) return ipStr;
                    }
                }
            }
        }
        return "127.0.0.1";
    }

    void OnApplicationQuit()
    {
        // cleanup listener & thread pas game di-close
        if (listener != null)
        {
            listener.Stop();
            listener.Close();
        }
        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Join(500);
        }
    }
}
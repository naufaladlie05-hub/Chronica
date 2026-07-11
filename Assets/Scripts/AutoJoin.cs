using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class AutoJoin : MonoBehaviour
{
    private NetworkManager manager;

    void Start()
    {
        manager = GetComponent<NetworkManager>();

#if UNITY_WEBGL

            WebGLInput.mobileKeyboardSupport = true;

            // ambil ip dari url buat konek client hp otomatis
            string url = Application.absoluteURL;
            if (url.Contains("?ip="))
            {
                string targetIP = url.Split(new string[] { "?ip=" }, System.StringSplitOptions.None)[1];
                // jaga-jaga kalo port live server vscode (5500) ikut ke-parse, potong aja
                if (targetIP.Contains(":")) targetIP = targetIP.Split(':')[0];
                
                manager.networkAddress = targetIP;
                manager.StartClient();
            }
#else
        // setup buat pc host
        // langsung start server aja soalnya pc cuma buat nampilin layar di proyektor
        manager.StartServer();
#endif
    }
}
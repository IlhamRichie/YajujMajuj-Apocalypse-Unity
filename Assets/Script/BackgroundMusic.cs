using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    // Variabel statis untuk menyimpan instance dari objek musik ini
    private static BackgroundMusic instance;

    void Awake()
    {
        // Cek apakah sudah ada instance dari BackgroundMusic
        if (instance == null)
        {
            // Jika belum ada, jadikan yang ini sebagai instance utama
            instance = this;
            // Jangan hancurkan objek ini saat pindah scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Jika sudah ada instance lain (misalnya saat kembali ke Main Menu),
            // hancurkan yang baru ini agar musik tidak jadi ganda.
            Destroy(gameObject);
        }
    }
}

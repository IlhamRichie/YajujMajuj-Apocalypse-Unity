using UnityEngine;

public class ManuscriptItem : MonoBehaviour
{
    [TextArea(5, 10)] // Atribut ini membuat field di Inspector jadi lebih besar, enak untuk teks panjang
    public string ayatContent; // Variabel untuk menyimpan isi ayat dari manuskrip ini

    public bool isCollected = false; // Status apakah sudah dikumpulkan (opsional, bisa berguna nanti)

    // Kamu bisa tambahkan variabel lain jika perlu, misalnya ID manuskrip, judul, dll.
}
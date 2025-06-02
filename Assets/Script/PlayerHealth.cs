using UnityEngine;
// Jika kamu mau menggunakan UI untuk health bar nanti, tambahkan:
// using UnityEngine.UI;
// using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    // (Opsional) Referensi ke UI health bar
    // public Image healthBarFill;
    // public TextMeshProUGUI healthText;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Player health: " + currentHealth + "/" + maxHealth);
        // UpdateHealthUI(); // Panggil jika ada UI
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; // Jangan lakukan apa-apa jika sudah mati

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0); // Pastikan health tidak kurang dari 0

        Debug.Log("Player terkena damage: " + damageAmount + ". Sisa health: " + currentHealth);
        // UpdateHealthUI(); // Panggil jika ada UI

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Pemain Mati!");
        // Logika kematian pemain:
        // 1. Nonaktifkan GameObject pemain
        // gameObject.SetActive(false);

        // 2. Tampilkan layar Game Over
        // FindObjectOfType<GameManager>().ShowGameOverScreen(); // Jika ada GameManager

        // 3. Restart scene (untuk game sederhana)
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // Untuk sekarang, kita bisa nonaktifkan movement player atau tampilkan pesan saja
        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null)
        {
            movementScript.enabled = false; // Matikan script movement
        }
        // Atau:
        // Destroy(gameObject); // Jika mau pemain hilang
    }

    // (Opsional) Fungsi untuk update UI Health Bar
    // void UpdateHealthUI()
    // {
    //     if (healthBarFill != null)
    //     {
    //         healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    //     }
    //     if (healthText != null)
    //     {
    //         healthText.text = currentHealth + " / " + maxHealth;
    //     }
    // }
}
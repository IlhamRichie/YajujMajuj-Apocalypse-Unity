using UnityEngine;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI answerButtonAText;
    public TextMeshProUGUI answerButtonBText;

    private PlayerHealth playerToHeal;
    private int healthAmount;
    private GameObject itemToDestroy;
    private bool isCorrectAnswerA;

    // Fungsi ini akan dipanggil oleh item nyawa
    // Di dalam QuizManager.cs
    public void StartQuiz(string question, string answerA, string answerB, bool isACorrect, PlayerHealth player, int healAmount, GameObject item)
    {
        questionText.text = question;
        answerButtonAText.text = answerA;
        answerButtonBText.text = answerB;
        isCorrectAnswerA = isACorrect;
        playerToHeal = player;
        healthAmount = healAmount;
        itemToDestroy = item;

        // Nonaktifkan gerakan pemain
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = false;

        // Nonaktifkan semua musuh
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.enabled = false;
        }

        // HAPUS ATAU KOMENTARI BARIS INI:
        // Time.timeScale = 0f; 

        quizPanel.SetActive(true);
    }

    // Fungsi ini akan dipanggil oleh Tombol Jawaban A
    public void OnAnswerAPressed()
    {
        if (isCorrectAnswerA)
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }
    }

    // Fungsi ini akan dipanggil oleh Tombol Jawaban B
    public void OnAnswerBPressed()
    {
        if (!isCorrectAnswerA)
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }
    }

    private void CorrectAnswer()
    {
        Debug.Log("Jawaban Benar!");
        playerToHeal.RestoreHealth(healthAmount);
        Destroy(itemToDestroy);
        CloseQuiz();
    }

    private void WrongAnswer()
    {
        Debug.Log("Jawaban Salah!");
        Destroy(itemToDestroy);
        CloseQuiz();
    }

    // Di dalam QuizManager.cs
    private void CloseQuiz()
    {
        // Aktifkan kembali gerakan pemain
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = true;

        // Aktifkan kembali semua musuh
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.enabled = true;
        }

        // HAPUS ATAU KOMENTARI BARIS INI:
        // Time.timeScale = 1f;

        quizPanel.SetActive(false);
    }
}
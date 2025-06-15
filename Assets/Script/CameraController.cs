using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform; // Assign Player di Inspector
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, 1, -10); // Offset posisi kamera dari pemain

    private bool isFollowingPlayer = true;

    void LateUpdate()
    {
        // Kamera akan mengikuti pemain jika tidak sedang dalam sekuens sinematik
        if (playerTransform != null && isFollowingPlayer)
        {
            Vector3 targetPosition = playerTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }

    // Coroutine untuk fokus ke target, lalu kembali ke pemain
    public IEnumerator FocusOnTargetAndReturn(Transform target, float focusDuration)
    {
        isFollowingPlayer = false; // Hentikan kamera mengikuti pemain

        // --- FASE 1: Bergerak dan Zoom ke Target (Gerbang) ---
        Vector3 originalPosition = transform.position;
        float originalOrthoSize = GetComponent<Camera>().orthographicSize;

        Vector3 targetCamPosition = new Vector3(target.position.x, target.position.y, offset.z);
        float targetOrthoSize = originalOrthoSize / 1.5f; // Zoom in sedikit (angka lebih kecil = lebih zoom)

        float elapsedTime = 0f;
        float moveDuration = 1.0f; // Durasi untuk bergerak ke target

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetCamPosition, elapsedTime / moveDuration);
            GetComponent<Camera>().orthographicSize = Mathf.Lerp(originalOrthoSize, targetOrthoSize, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Tunggu frame berikutnya
        }

        // Pastikan posisi dan zoom akurat di akhir
        transform.position = targetCamPosition;
        GetComponent<Camera>().orthographicSize = targetOrthoSize;

        // --- FASE 2: Tahan Fokus di Target ---
        yield return new WaitForSeconds(focusDuration); // Tahan kamera di gerbang selama durasi yang ditentukan

        // --- FASE 3: Kembali ke Pemain ---
        originalPosition = transform.position; // Posisi kamera sekarang ada di gerbang
        elapsedTime = 0f;
        // moveDuration bisa diatur lagi jika ingin kecepatan kembali yang berbeda

        while (elapsedTime < moveDuration)
        {
            Vector3 playerTargetPosition = playerTransform.position + offset; // Posisi pemain mungkin sudah berubah sedikit
            transform.position = Vector3.Lerp(originalPosition, playerTargetPosition, elapsedTime / moveDuration);
            GetComponent<Camera>().orthographicSize = Mathf.Lerp(targetOrthoSize, originalOrthoSize, elapsedTime / moveDuration); // Zoom out kembali
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isFollowingPlayer = true; // Kembalikan kamera ke mode mengikuti pemain
    }
}
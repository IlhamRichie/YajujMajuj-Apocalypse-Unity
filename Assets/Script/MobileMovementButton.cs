using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMovementButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerMovement playerMovement;
    public bool isLeftButton;

    public void OnPointerDown(PointerEventData eventData)
    {
        // TAMBAHKAN LOG INI UNTUK TESTING
        Debug.Log(gameObject.name + " -> OnPointerDown DITEKAN!"); 

        if (playerMovement != null)
        {
            playerMovement.Move(isLeftButton ? -1f : 1f);
        }
        else
        {
            // Tambahkan log error jika playerMovement belum di-assign
            Debug.LogError("PlayerMovement belum di-assign di tombol " + gameObject.name);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ... (kode OnPointerUp tetap sama)
        if (playerMovement != null)
        {
            playerMovement.Move(0f);
        }
    }
}
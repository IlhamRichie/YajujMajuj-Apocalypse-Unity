using UnityEngine;
using UnityEngine.EventSystems; // Penting untuk IPointerDownHandler dan IPointerUpHandler

public class MobileMovementButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerMovement playerMovement; // Drag GameObject Player ke sini
    public bool isLeftButton;             // Centang ini di Inspector untuk tombol Kiri

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playerMovement != null)
        {
            if (isLeftButton)
            {
                playerMovement.MoveLeft(true);
            }
            else // Tombol Kanan
            {
                playerMovement.MoveRight(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (playerMovement != null)
        {
            if (isLeftButton)
            {
                playerMovement.MoveLeft(false);
            }
            else // Tombol Kanan
            {
                playerMovement.MoveRight(false);
            }
        }
    }
}
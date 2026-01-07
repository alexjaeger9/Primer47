using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float verticalBoost = 13f; //JumpBoost
    [SerializeField] private float horizontalBoost = 25f; //SpeedBoost
    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerEnter(Collider other)
    {
        //nur Player boosten (keine Ghosts)
        if (!IsInLayerMask(other.gameObject, playerLayer)){
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        
        //boost berechnen und anwenden
        Vector3 boostDirection = CalculateBoostDirection(player);
        player.ApplyJumpPadBoost(boostDirection);
    }

    private Vector3 CalculateBoostDirection(PlayerController player)
    {
        Vector3 horizontalDirection;

        //prüfe ob Spieler sich bewegt
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        if (isMoving)
        {
            //Bewegungsrichtung des Spielers verstärken
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            horizontalDirection = player.transform.rotation * inputDirection;
        }
        else
        {
            //Blickrichtung der Kamera
            Camera mainCam = Camera.main;
            horizontalDirection = mainCam.transform.forward;
            horizontalDirection.y = 0f;
            horizontalDirection.Normalize();
        }

        //horizontal und vertikal kombinieren
        Vector3 finalDirection = (horizontalDirection * horizontalBoost) + (Vector3.up * verticalBoost);
        return finalDirection;
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) > 0;
    }
}
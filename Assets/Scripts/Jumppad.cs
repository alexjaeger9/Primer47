using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float verticalBoost = 13f; //JumpBoost
    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerEnter(Collider other)
    {
        //nur Player boosten (keine Ghosts)
        if (!IsInLayerMask(other.gameObject, playerLayer)){
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        
        //boost berechnen und anwenden
        Vector3 boostDirection = Vector3.up * verticalBoost;
        player.ApplyJumpPadBoost(boostDirection);
    }


    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) > 0;
    }
}
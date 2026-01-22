using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    [Header("Spawn Area Settings")]
    // Größe der Arena (X = Breite, Z = Tiefe)
    public Vector2 arenaSize = new Vector2(70f, 70f);
    //Randabstand
    public float edgeDistance = 3f;
    //Mindestabstand zu Jump Pads"
    public float jumpPadDistance = 5f;
    //Von welcher Höhe soll nach unten gecastet werden?
    public float raycastHeight = 15f;
    
    //Liste für letzten 3 Spawn Positionen
    private List<Vector3> lastThreeSpawns = new List<Vector3>();
    
    //hibt eine zufällige, gültige Spawn-Position zurück
    public Vector3 GetRandomSpawnPosition()
    {
        int maxAttempts = 50;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            //zufällige X/Z Position innerhalb der Arena (mit Abstand vom Rand)
            float randomX = Random.Range(
                transform.position.x - arenaSize.x / 2 + edgeDistance,
                transform.position.x + arenaSize.x / 2 - edgeDistance
            );
            float randomZ = Random.Range(
                transform.position.z - arenaSize.y / 2 + edgeDistance,
                transform.position.z + arenaSize.y / 2 - edgeDistance
            );
            
            //Raycast mit random x, casthöhe und random z
            Vector3 rayStart = new Vector3(randomX, raycastHeight, randomZ);
            RaycastHit hit;
            
            //wenn laser was getroffen hat
            //out hit = wenn etwas getroffen, raycastHeight *2 = maximale Länge des Lasers
            if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastHeight * 2))
            {
                //hit.point = Position wo laser getroffen hat + 0,5m über Boden (nicht stuck im boden)
                Vector3 spawnPosition = hit.point + Vector3.up * 0.5f;
                
                //zu nah an letzten 3 Spawns
                if (isTooCloseToLastSpawns(spawnPosition))
                    continue; //nächste Iteration
                
                //zu nah an Jump Pads
                if (IsTooCloseToJumpPad(spawnPosition))
                    continue; //nächste Iteration
                
                //Position speichern
                lastThreeSpawns.Add(spawnPosition);
                //wenn mehr als drei sind, lösche den ersten
                if (lastThreeSpawns.Count > 3)
                    lastThreeSpawns.RemoveAt(0);
                
                return spawnPosition;
            }
        }
        
        //nach 50 Versuchen nichts gefunden: Mitte
        return transform.position + Vector3.up * 2f;
    }
    
    //prüft ob zu nah an den letzten 3 Spawns (5 Meter)
    private bool isTooCloseToLastSpawns(Vector3 position)
    {
        //geht durch alte Spawns in Liste
        foreach (Vector3 oldSpawn in lastThreeSpawns)
        {
            //wenn Distanz zwischen neuer Position und alter Spawnposition kleiner als 5
            if (Vector3.Distance(position, oldSpawn) < 5f)
                return true; //zu nah!
        }
        return false; //genug weit weg
    }
    

    //prüft ob zu nah an einem Jump Pad
    private bool IsTooCloseToJumpPad(Vector3 position)
    {
        //alle Jump Pads
        JumpPad[] allJumpPads = FindObjectsByType<JumpPad>(FindObjectsSortMode.None);
        
        foreach (JumpPad pad in allJumpPads)
        {
            float distance = Vector3.Distance(position, pad.transform.position);
            if (distance < jumpPadDistance)
                return true; //zu nah
        }
        return false; //genug weit weg
    }
    
    //setzt Spawn History zurück
    public void ResetSpawnHistory()
    {
        lastThreeSpawns.Clear();
    }
}
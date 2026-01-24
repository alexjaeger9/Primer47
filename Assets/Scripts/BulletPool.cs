using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;
    public GameObject tracerPrefab;
    public int poolSize = 100;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    
    private void Awake()
    {
        Instance = this;
        
        //Pool mit Anzahl der Poolsize füllen
        for (int i = 0; i < poolSize; i++)
        {
            GameObject tracer = Instantiate(tracerPrefab); //erstellen
            tracer.SetActive(false); //deaktivieren (damit die nicht direkt schießen)
            tracer.transform.SetParent(transform); //für Übersichtlichkeit als Kind
            bulletPool.Enqueue(tracer); //in Warteschlange FIFO
        }
    }
    
    public GameObject GetBullet()
    {
        //falls mehr als Poolsize, keine mehr da -> mehr erstellen
        if (bulletPool.Count == 0)
        {
            GameObject newTracer = Instantiate(tracerPrefab);
            newTracer.transform.SetParent(transform);
            newTracer.SetActive(false);
            return newTracer;
        }
        
        GameObject tracer = bulletPool.Dequeue(); //holt erste Bullet
        tracer.transform.SetParent(null); //macht Tracer kinderlos
        return tracer; //Bullet zurückgeben (noch nicht aktiviert)
    }
    
    public void ReturnBullet(GameObject tracer)
    {
        tracer.SetActive(false); //deaktivieren
        tracer.transform.SetParent(transform);
        bulletPool.Enqueue(tracer); //zurück an die Schlange
    }
}
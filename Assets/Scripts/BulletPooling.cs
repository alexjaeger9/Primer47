using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance; //Singleton damit alle drauf zugreifen können
    public GameObject bulletPrefab;
    public int poolSize = 100; //Anzahl vorher erstellten Bullets
    //bulletPool = Lager für inaktive Bullets, Queue -> FIFO
    private Queue<GameObject> bulletPool = new Queue<GameObject>(); // Warteschlange für die Bullets
    
    private void Awake()
    {
        Instance = this;
        
        //Pool mit Bullets füllen
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false); //Bullets deaktivieren (unsichtbar aber im Speicher)
            bullet.transform.SetParent(transform); //als Kind von BulletPool setzen zur Übersicht
            bulletPool.Enqueue(bullet); //in Warteschlange rein
        }
    }
    
    //Bullet aus dem Pool holen
    public GameObject GetBullet()
    {
        //wenn Pool leer ist (mehr als poolsize), erstelle neues Bullet
        if (bulletPool.Count == 0)
        {
            GameObject newBullet = Instantiate(bulletPrefab);
            newBullet.transform.SetParent(transform); //Kind von BulletPool
            newBullet.SetActive(false); //deaktivieren
            return newBullet;
        }
        
        //Bullet aus Pool holen
        GameObject bullet = bulletPool.Dequeue();
        bullet.transform.SetParent(null); //kein Kind mehr von BulletPool
        return bullet;
    }
    
    //Bullet zurück legen
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false); //deaktivieren
        bullet.transform.SetParent(transform); //zurück als Kind setzen
        bulletPool.Enqueue(bullet); //zurück in Warteschlange
    }
}
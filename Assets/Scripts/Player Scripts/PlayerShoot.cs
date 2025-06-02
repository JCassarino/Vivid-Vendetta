using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    
public GameObject bulletPrefab;
public Transform bulletSpawnPoint;
public float bulletSpeed = 10f;
public GameObject crosshairPrefab;

private Camera mainCamera;
private GameObject crosshair;

void Start(){

    Cursor.visible = false;

    mainCamera = Camera.main;

    if (crosshairPrefab){
        crosshair = Instantiate(crosshairPrefab);
        Cursor.visible = false;
    }
    else
    {
        Debug.LogError("Crosshair prefab is not assigned in the Inspector.");
    }
}


    void Update()
    {

        HandleCrosshair();

        if (Input.GetButtonDown("Fire1")){
            Shoot();
        }

        HandleRotation();
    }


    void HandleCrosshair(){

        if (crosshair)
        {
        // Get mouse position in screen space
            Vector3 mousePosition = Input.mousePosition;
        
        // Convert it to world space with a z position of 0
            mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            mousePosition.z = 0f;  // Make sure the z is 0 so it's in the same plane as your player

        // Move crosshair to the mouse position
            crosshair.transform.position = mousePosition;
        }
        else
        {
            Debug.LogError("Crosshair is not instantiated.");
        }
    }


    void HandleRotation(){
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Vector3 aimDirection = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }


    void Shoot(){

         // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Calculate direction
        Vector2 aimDirection = (mousePosition - bulletSpawnPoint.position).normalized;

        // Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);

        // Set bullet velocity
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = aimDirection * bulletSpeed;
        }

    }
}

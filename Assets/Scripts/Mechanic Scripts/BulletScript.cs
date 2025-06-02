using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    SpriteRenderer bulletRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // added pre-existing vars
        bulletRenderer = GetComponent<SpriteRenderer>();
        //bulletCollider = GetComponent<Collider2D>();

        //change bullet color
        bulletRenderer.color = Random.ColorHSV();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}

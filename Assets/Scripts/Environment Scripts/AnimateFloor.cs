using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animatefloor : MonoBehaviour
{
    public GameObject animatedFloorTile;
    public int width = 5;
    public int height = 5;
    public float tileSize = 1f;

    void Start(){
        GenerateFloor();
    }

    // Update is called once per frame
    void GenerateFloor(){

        for (int x = 0; x < width; x++){
            
            for (int y = 0; y < height; y++){
                // Calculate the position for each tile
                Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
                
                // Instantiate the tile at the calculated position
                GameObject tile = Instantiate(animatedFloorTile, position, Quaternion.identity);

                // Set the tile as a child of the floor GameObject (for organization)
                tile.transform.parent = transform;
            }
        }
    }
}

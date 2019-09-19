using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellData
{
    public Vector3Int LocalPlace {get; set;}
    public Vector3 WorldLocation {get; set;}
    public TileBase TileBase {get; set;}
    public Tilemap TilemapMember {get; set;}
    public string name {get; set;}

    public int maturity {get; set;}
    public int fishProduction {get; set;}

    public string printData() {
        string output = "";
        output += ("LocalPlace: " + LocalPlace + "\n");
        output += ("WorldLocation: " + WorldLocation + "\n");
        output += ("TileBase: " + TileBase + "\n");
        output += ("TilemapMember: " + TilemapMember + "\n");
        output += ("name: " + name + "\n");
        output += ("maturity: " + maturity + "\n");
        output += ("fishProduction: " + fishProduction + "\n");
        return output;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AlgaeCellData
{
    public Vector3Int LocalPlace {get; set;}
    public Vector3 WorldLocation {get; set;}
    public TileBase TileBase {get; set;}
    public Tilemap TilemapMember {get; set;}
    public string name {get; set;}

    public float maturity {get; set;}
    public int fishProduction {get; set;}
    public int smallFishProduction {get; set;}
    public int bigFishProduction {get; set;}
    public int herbivorousFishProduction {get; set;}

    public string printData() {
        string output = "";
        output += ("LocalPlace: " + LocalPlace + "\n");
        output += ("WorldLocation: " + WorldLocation + "\n");
        output += ("TileBase: " + TileBase + "\n");
        output += ("TilemapMember: " + TilemapMember + "\n");
        output += ("name: " + name + "\n");
        output += ("maturity: " + maturity + "\n");
        output += ("fishProduction: " + fishProduction + "\n");
        output += ("smallFishProduction: " + smallFishProduction + "\n");
        output += ("bigFishProduction: " + bigFishProduction + "\n");
        output += ("herbivorousFishProduction: " + herbivorousFishProduction + "\n");
        return output;
    }

    public void addMaturity (float maturitySpeed) {
        maturity += maturitySpeed;
    }

    public bool willSurvive (float randNum, float groundViability) {
        float computedSurvivability = Mathf.Min((groundViability - 50.0f)/100.0f*maturity + 50.0f, groundViability);
        
        if (randNum <= computedSurvivability) return true;
        else return false;
    }
}

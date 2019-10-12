using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CoralCellData
{
    public Vector3Int LocalPlace {get; set;}
    public Vector3 WorldLocation {get; set;}
    public TileBase TileBase {get; set;}
    public Tilemap TilemapMember {get; set;}
    public string name {get; set;}

    public float maturity {get; set;}
    public int fishProduction {get; set;}
    public float carnivorousFishInterest {get; set;}
    public float herbivorousFishInterest {get; set;}

    public string printData() {
        string output = "";
        output += ("LocalPlace: " + LocalPlace + "\n");
        output += ("WorldLocation: " + WorldLocation + "\n");
        output += ("TileBase: " + TileBase + "\n");
        output += ("TilemapMember: " + TilemapMember + "\n");
        output += ("name: " + name + "\n");
        output += ("maturity: " + maturity + "\n");
        output += ("fishProduction: " + fishProduction + "\n");
        output += ("carnivorousFishInterest: " + carnivorousFishInterest + "\n");
        output += ("herbivorousFishInterest: " + herbivorousFishInterest + "\n");
        return output;
    }

    public void addMaturity (float maturitySpeed) {
        maturity += maturitySpeed;
    }

    public bool willSurvive (float randNum, float groundViability, float miscFactors) {
        float computedSurvivability = Mathf.Min((groundViability - 75.0f)/100.0f*maturity + 75.0f, groundViability);
        
        if (randNum <= computedSurvivability+miscFactors) return true;
        else return false;
    }

}

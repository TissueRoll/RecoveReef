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
    public int herbivorousFishProduction {get; set;}

    public AlgaeCellData () {
        // this shouldnt be called; setting name to ERROR by default
        name = "ERROR";
    }

    public AlgaeCellData (Vector3Int _position, Tilemap _tilemap, TileBase _tilebase, float _maturity, int _hFP) {
        LocalPlace = _position;
        WorldLocation = _tilemap.CellToWorld(_position);
        TileBase = _tilebase;
        TilemapMember = _tilemap;
        name = _position.x + "," + _position.y;
        maturity = _maturity;
        herbivorousFishProduction = _hFP;
    }

    public string printData() {
        string output = "";
        output += ("LocalPlace: " + LocalPlace + "\n");
        output += ("WorldLocation: " + WorldLocation + "\n");
        output += ("TileBase: " + TileBase + "\n");
        output += ("TilemapMember: " + TilemapMember + "\n");
        output += ("name: " + name + "\n");
        output += ("maturity: " + maturity + "\n");
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

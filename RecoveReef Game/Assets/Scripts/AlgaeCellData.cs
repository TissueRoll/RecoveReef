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
    public string uniqueName {get; set;}
    public int maturity {get; set;}
    public AlgaeData algaeData {get; set;}

    public AlgaeCellData () {
        // this shouldnt be called; setting uniqueName to ERROR by default
        uniqueName = "ERROR";
    }

    public AlgaeCellData (Vector3Int _position, Tilemap _tilemap, TileBase _tilebase, int _maturity, AlgaeData _algaeData) {
        LocalPlace = _position;
        WorldLocation = _tilemap.CellToWorld(_position);
        TileBase = _tilebase;
        TilemapMember = _tilemap;
        uniqueName = _position.x + "," + _position.y;
        maturity = _maturity;
        algaeData = _algaeData;
    }

    public string printData() {
        string output = "";
        output += ("LocalPlace: " + LocalPlace + "\n");
        output += ("WorldLocation: " + WorldLocation + "\n");
        output += ("TileBase: " + TileBase + "\n");
        output += ("TilemapMember: " + TilemapMember + "\n");
        output += ("uniqueName: " + uniqueName + "\n");
        output += ("maturity: " + maturity + "\n");
        output += ("algaeData: " + algaeData.dataToString() + "\n");
        return output;
    }

    public void addMaturity (int maturitySpeed) {
        maturity += maturitySpeed;
    }
}

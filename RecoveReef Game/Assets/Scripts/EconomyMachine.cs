﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyMachine
{

    private float actualHF;
    private float actualCF;
    private float tolerance;

    public EconomyMachine (float aHF, float aCF, float tol) {
        actualHF = aHF;
        actualCF = aCF;
        tolerance = tol;
    }

    public bool algaeWillSurvive (AlgaeCellData algaeCellData, float groundViability, float additiveFactors, float multiplicativeFactors) {
        bool result = true;
        float computedSurvivability = (groundViability - 75.0f)/100.0f*algaeCellData.maturity + 75.0f;
        result = (computedSurvivability <= UnityEngine.Random.Range(0.0f,100.0f));
        return result;
    }

    public bool coralWillSurvive (CoralCellData coralCellData, float groundViability, float additiveFactors, float multiplicativeFactors) {
        bool result = true;
        float computedSurvivability = (groundViability - 75.0f)/100.0f*coralCellData.maturity + 75.0f;
        result = (UnityEngine.Random.Range(0.0f,100.0f) <= computedSurvivability+additiveFactors);
        return result;
    }

    public bool coralWillPropagate (CoralCellData coralCellData, float additiveFactors, float multiplicativeFactors) {
        bool result = true;
        float basePropagationChance = UnityEngine.Random.Range(50.0f,60.0f); // USE CELL DATA FOR THIS
        float computedPropagationChance = basePropagationChance + UnityEngine.Random.Range(0.0f, 5.0f) + additiveFactors;
        computedPropagationChance = Mathf.Min(100.0f, computedPropagationChance*multiplicativeFactors);
        result = (UnityEngine.Random.Range(0.0f,100.0f) <= computedPropagationChance);
        return result;
    }

    public bool algaeWillPropagate (AlgaeCellData algaeCellData, float additiveFactors, float multiplicativeFactors) {
        bool result = true;
        float basePropagationChance = UnityEngine.Random.Range(50.0f,60.0f); // USE CELL DATA FOR THIS
        float computedPropagationChance = basePropagationChance + UnityEngine.Random.Range(0.0f, 5.0f) + additiveFactors;
        computedPropagationChance = Mathf.Min(100.0f, computedPropagationChance*multiplicativeFactors);
        result = (UnityEngine.Random.Range(0.0f,100.0f) <= computedPropagationChance);
        return result;
    }

    public void updateHFCF(float hf, float cf) {
        float diff = (Mathf.Max(cf,hf)-Mathf.Min(cf,hf));
        if (Mathf.Abs(diff) <= tolerance) {
            actualHF = hf;
            actualCF = (cf > hf ? cf-tolerance : cf);
        } else if (cf > hf) {
            actualCF = hf - diff;
            actualHF = actualCF + tolerance;
        } else {
            actualCF = Mathf.Max(hf-tolerance, cf);
            actualHF = hf - 0.5f*tolerance;
        }
        actualHF = Mathf.Max(0f, actualHF);
        actualCF = Mathf.Max(0f, actualCF);
    }

    public float getActualHF() {
        return actualHF;
    }

    public float getActualCF() {
        return actualCF;
    }

    public float getTotalFish() {
        return 0.2f*actualHF+0.3f*actualCF;
    }

}
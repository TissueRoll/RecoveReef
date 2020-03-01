using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

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

    public bool algaeWillSurvive (AlgaeCellData algaeCellData, int groundViability, int additiveFactors) {
        bool result = true;
        int computedSurvivability = groundViability
                                    + algaeCellData.maturity
                                    + algaeCellData.algaeData.survivability
                                    + additiveFactors;
        result = (UnityEngine.Random.Range(0,1001) <= computedSurvivability);
        return result;
    }

    public bool coralWillSurvive (CoralCellData coralCellData, int groundViability, int additiveFactors, string groundName) {
        bool result = true;
        int computedSurvivability = groundViability 
                                    + coralCellData.maturity 
                                    + coralCellData.coralData.survivability 
                                    + additiveFactors
                                    + (Regex.IsMatch(groundName, "." + coralCellData.coralData.prefTerrain + ".") ? 5 : -10);
        result = (UnityEngine.Random.Range(0,1001) <= computedSurvivability);
        return result;
    }

    public bool coralWillPropagate (CoralCellData coralCellData, int additiveFactors, string groundName) {
        bool result = true;
        int computedPropagatability = UnityEngine.Random.Range(30,41) // base
                                    + coralCellData.coralData.propagatability 
                                    + additiveFactors
                                    + (Regex.IsMatch(groundName, "." + coralCellData.coralData.prefTerrain + ".") ? 5 : -10);
        result = (UnityEngine.Random.Range(0,101) <= computedPropagatability);
        return result;
    }

    public bool algaeWillPropagate (AlgaeCellData algaeCellData, int additiveFactors, string groundName) {
        bool result = true;
        int computedPropagatability = UnityEngine.Random.Range(20,31) // base
                                    + algaeCellData.algaeData.propagatability 
                                    + additiveFactors;
        result = (UnityEngine.Random.Range(0,101) <= computedPropagatability);
        return result;
    }

    public void updateHFCF(float hf, float cf) {
        float diff = (Mathf.Max(cf,hf)-Mathf.Min(cf,hf));
        if (Mathf.Abs(diff) <= tolerance) { // within threshold
            actualHF = hf;
            actualCF = (cf > hf ? cf-diff : cf);
        } else if (cf > hf) { // more carnivorous
            actualCF = hf - diff;
            actualHF = actualCF + tolerance;
        } else { // more herbivorous
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

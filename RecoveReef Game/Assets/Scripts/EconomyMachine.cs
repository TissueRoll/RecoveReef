using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class EconomyMachine
{

    private float actualHF;
    private float actualCF;
    private float tolerance;
    private int coralNumberTolerance;
    private int cycle;
    private int cycleMax;
    private List<float> vals;
    private bool initialCycleComplete;
    private List<float> desired;
    float tolerancePercentage;

    public EconomyMachine (float aHF, float aCF, float tol, int cNT) {
        actualHF = aHF;
        actualCF = aCF;
        tolerance = tol;
        coralNumberTolerance = cNT;
        cycle = 0;
        cycleMax = 5;
        vals = new List<float>() {1.0f, 1.0f, 1.0f, 1.0f, 1.0f};
        initialCycleComplete = false;
        desired = new List<float>() {0.25f, 0.21f, 0.15f, 0.17f, 0.12f, 0.10f};
        tolerancePercentage = 0.05f;
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

    public float diversityMultiplier (List<int> coralNumbers) {
        float multiplier = 1.0f;
        int[] fast = new int[3]{0,1,3};
        int[] slow = new int[3]{2,4,5};
        float sum = 0;
        foreach (int x in coralNumbers) {
            sum += x;
        }

        for (int i = 0; i < coralNumbers.Count; i++) {
            float computedPercentage = ((float)coralNumbers[i])/sum;
            if (desired[i]-tolerancePercentage <= computedPercentage && computedPercentage <= desired[i]+tolerancePercentage) {
                multiplier += 0.5f/((float)coralNumbers.Count);
            }
        }

        vals[cycle] = multiplier;
        if (cycle == 4 && !initialCycleComplete) {
            initialCycleComplete = true;
        }
        cycle = (cycle+1)%cycleMax;

        float averageMultiplier = 0.0f;
        foreach (float x in vals) {
            averageMultiplier += x;
        }
        averageMultiplier /= ((float)cycleMax);

        // Debug.Log("---");
        // foreach(float x in vals)
        //     Debug.Log(x);
        // Debug.Log("---");

        return averageMultiplier;
    }

    public void updateHFCF(float hf, float cf) {
        float diff = (Mathf.Max(cf,hf)-Mathf.Min(cf,hf));
        if (Mathf.Abs(diff) <= tolerance) { // within threshold
            actualHF = hf;
            actualCF = (cf > hf ? cf-diff : cf);
        } else if (cf > hf) { // more carnivorous
            actualCF = hf - 1.5f*diff;
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

    public float getTotalFish(List<int> coralNumbers) {
        return (0.2f*actualHF+0.3f*actualCF)*diversityMultiplier(coralNumbers);
    }

    public bool isAverageGood() {
        float averageMultiplier = 0.0f;
        foreach (float x in vals) {
            averageMultiplier += x;
        }
        averageMultiplier /= ((float)cycleMax);
        return (averageMultiplier >= 1.35f);
    }

}

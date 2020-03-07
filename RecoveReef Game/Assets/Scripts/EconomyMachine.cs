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

    public EconomyMachine (float aHF, float aCF, float tol, int cNT) {
        actualHF = aHF;
        actualCF = aCF;
        tolerance = tol;
        coralNumberTolerance = cNT;
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
        Debug.Log("___");
        foreach (int num in coralNumbers) {
            Debug.Log(num);
        }
        Debug.Log("***");
        float multiplier = 1.0f;
        int[] fast = new int[3]{0,1,3};
        int[] slow = new int[3]{2,4,5};
        float fMax = 0;
        int fMaxIdx = 0;
        float fDist = 0.0f;
        float sMax = 0;
        int sMaxIdx = 2;
        float sDist = 0.0f;
        for (int i = 0; i < 3; i++) {
            fDist += coralNumbers[fast[i]];
            sDist += coralNumbers[slow[i]];
            if (coralNumbers[fast[i]] > fMax) {
                fMax = coralNumbers[fast[i]];
                fMaxIdx = fast[i];
            }
            if (coralNumbers[slow[i]] > sMax) {
                sMax = coralNumbers[slow[i]];
                sMaxIdx = slow[i];
            }
        }
        fDist -= fMax;
        fDist /= 2.0f;
        sDist -= sMax;
        sDist /= 2.0f;

        float interLimit = coralNumberTolerance/2.0f;
        float cNTtemp = coralNumberTolerance;
        if (fMax >= sMax) {
            if (fMax*3/5 - cNTtemp <= sMax && sMax <= fMax) { // within s/f threshold
                float fastDev = fDist-(fMax-cNTtemp-interLimit);
                float slowDev = sDist-(sMax-cNTtemp-interLimit);
                float worstDev = Mathf.Max(0.0f, Mathf.Min(fastDev,slowDev));
                multiplier = 1.0f + 0.5f*Mathf.Lerp(0.0f, interLimit, Mathf.Min(interLimit, worstDev));
            } else {
                multiplier = 1.0f;
            }
        } else {
            if ((sMax - fMax) <= cNTtemp) { // within s/f threshold
                float fastDev = fDist-(fMax-cNTtemp-interLimit);
                float slowDev = sDist-(sMax-cNTtemp-interLimit);
                float worstDev = Mathf.Max(0.0f, Mathf.Min(fastDev,slowDev));
                multiplier = 1.0f + 0.5f*Mathf.Lerp(0.0f, interLimit, Mathf.Min(interLimit, worstDev));
            } else {
                multiplier = 1.0f;
            }
        }

        return multiplier;
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

    public float getTotalFish() {
        return (0.2f*actualHF+0.3f*actualCF);
    }

    // public bool isDiverse(List<int> coralNumbers) {
    //     return (diversityMultiplier(coralNumbers) < 1.5f ? false : true);
    // }

}

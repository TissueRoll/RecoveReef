using System.Collections;
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

    public bool algaeWillSurvive () {
        bool result = true;

        return result;
    }

    public bool coralWillSurvive () {
        bool result = true;

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

}

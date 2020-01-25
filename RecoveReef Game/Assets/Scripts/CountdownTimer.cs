using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownTimer
{
    public float currentTime {get; set;}
    public float timeDuration {get; set;}
    public float percentComplete {get; set;}
    
    public CountdownTimer() {
        timeDuration = 5f;
        currentTime = timeDuration;
    }
    public CountdownTimer(float tD) {
        timeDuration = tD;
        currentTime = tD;
    }
    public void updateTime() {
        currentTime -= 1 * Time.deltaTime;
        if (currentTime <= 0)
            currentTime = 0;
        percentComplete = 1f-currentTime/timeDuration;
    }

    public bool isDone() {
        return currentTime == 0;
    }
}

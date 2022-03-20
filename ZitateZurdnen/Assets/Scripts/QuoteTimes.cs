using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuoteTimes : MonoBehaviour
{
    float quoteTime;
    float quoteStart;

    QuoteData currentQuote;

    public static QuoteData fastestQuote;
    public static QuoteData slowestQuote;

    float fastestTime = float.MaxValue;
    float slowestTime = 0;

   public void QuoteStart(QuoteData currentQuote)
    {
        quoteStart = Time.realtimeSinceStartup;

        this.currentQuote = currentQuote; // this bezieht sich auf die Klasse
    }

    public void QuoteEnd()
    {
        quoteTime = Time.realtimeSinceStartup - quoteStart;

        Debug.Log("Du hast " + quoteTime + " Sekunden gebraucht!");


        if(quoteTime > slowestTime)
        {
            slowestTime = quoteTime;
            slowestQuote = currentQuote;
        }

        if (quoteTime < fastestTime)
        {
            fastestTime = quoteTime;
            fastestQuote = currentQuote;
        }

    }
}

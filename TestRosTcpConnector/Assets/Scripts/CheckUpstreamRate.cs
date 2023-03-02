using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using TMPro;

public class CheckUpstreamRate : MonoBehaviour
{
    public MessageUpstreamRateCalculator Calculator;
    public TMP_Text OutputText;
    public string OutputFormat = "Up : {0:0.##} {1}";

    private void Awake()
    {
        Calculator.OnCalculateSpeed += Calc_OnCalculateSpeed;
    }

    private void OnDestroy()
    {
        Calculator.OnCalculateSpeed -= Calc_OnCalculateSpeed;
    }

    private void Calc_OnCalculateSpeed(ulong dataSize, double bitsPerSec, string unit)
    {
        OutputText.text = string.Format(OutputFormat, bitsPerSec, unit);
    }
}

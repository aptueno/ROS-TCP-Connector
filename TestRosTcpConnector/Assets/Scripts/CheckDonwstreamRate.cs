using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class CheckDonwstreamRate : MonoBehaviour
{
    public MessageDownstreamRateCalculator Calculator;
    public TMP_Text OutputText;
    public string OutputFormat = "Down : {0:0.##} {1}";

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

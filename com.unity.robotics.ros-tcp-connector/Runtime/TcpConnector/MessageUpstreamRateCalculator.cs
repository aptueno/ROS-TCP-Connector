using Codice.CM.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace Unity.Robotics.ROSTCPConnector
{
    public class MessageUpstreamRateCalculator : MonoBehaviour
    {
        public static MessageUpstreamRateCalculator Shared { private set; get; }

        public delegate void DataSpeedCalculatorDelegate(UInt64 dataSize, double bitsPerSec, string unit);
        public event DataSpeedCalculatorDelegate OnCalculateSpeed;

        public enum TargetRate : int
        {
            [InspectorName("1 Hz")]
            _1 = 1,
            [InspectorName("2 Hz")]
            _2 = 2,
            [InspectorName("4 Hz")]
            _4 = 4,
            [InspectorName("8 Hz")]
            _8 = 8
        }
        public TargetRate RefreshRate = TargetRate._1;
        private TargetRate? _RefreshRate;

        public TargetRate OutputRate = TargetRate._1;
        private TargetRate? _OutputRate;

        private UInt64 SplitSize => (UInt64)RefreshRate;
        private UInt64[] dataSizeBuffer = new UInt64[1];
        private int bufferIndex = 0;

        [SerializeField]
        private UInt64 dataSize = 0;
        [SerializeField]
        private double dataSpeed = 0;
        [SerializeField]
        private string dataSpeedUnit = "bps";

        private float calcCurrentTime = 0f;
        private float calcRefreshInverval;
        private float outputCurrentTime = 0f;
        private float outputRefreshInverval;

        private object dataLock = new object();

        private void Awake()
        {
            if (Shared != null)
            {
                Destroy(this);
                return;
            }
            Shared = this;
        }

        private void OnDestroy()
        {
            if (Shared == this)
                Shared = null;
        }

        // Start is called before the first frame update
        void Start() { }

        // Update is called once per frame
        void Update()
        {
            if (_RefreshRate != RefreshRate)
            {
                calcCurrentTime = 0;
                calcRefreshInverval = 1f / (float)RefreshRate;
                bufferIndex = 0;
                lock (dataLock)
                {
                    _RefreshRate = RefreshRate;
                    dataSizeBuffer = new UInt64[SplitSize];
                }
            }

            if (_OutputRate != OutputRate)
            {
                _OutputRate = OutputRate;
                outputCurrentTime = 0;
                outputRefreshInverval = 1f / (float)OutputRate;
            }

            var time = Time.deltaTime;
            calcCurrentTime += time;
            outputCurrentTime += time;

            if (calcCurrentTime >= calcRefreshInverval)
            {
                calcCurrentTime -= calcRefreshInverval;
                lock (dataLock)
                {
                    dataSize = dataSizeBuffer[bufferIndex] * SplitSize;
                    dataSizeBuffer[bufferIndex] = 0;
                    bufferIndex += 1;
                    if (bufferIndex >= dataSizeBuffer.Length) bufferIndex = 0;
                }
                var (speed, unit) = CalculateDataSpeed(dataSize);
                dataSpeed = speed;
                dataSpeedUnit = unit;
            }

            if (outputCurrentTime >= outputRefreshInverval)
            {
                outputCurrentTime -= outputRefreshInverval;
                OnCalculateSpeed?.Invoke(dataSize, dataSpeed, dataSpeedUnit);
            }
        }

        public void AddBit(UInt64 bitSize)
        {
            SetBuffer(bitSize);
        }

        public void AddByte(UInt64 byteSize)
        {
            SetBuffer(byteSize * 8);
        }

        private void SetBuffer(UInt64 bitSize)
        {
            lock (dataLock)
            {
                var size = bitSize / SplitSize;
                for (int i = 0; i < dataSizeBuffer.Length; i++)
                {
                    dataSizeBuffer[i] += size;
                }
            }
        }

        public UInt64 GetDataSize()
        {
            return dataSize;
        }

        public double GetDataSpeed()
        {
            return dataSpeed;
        }

        public string GetDataSpeedUnit()
        {
            return dataSpeedUnit;
        }

        private (double, string) CalculateDataSpeed(UInt64 value)
        {
            double dValue = value * 10;
            if (value < 1024)
            {
                return (value, "bps");
            }
            else if (dValue < Math.Pow(1024, 2) * 10)
            {
                return (Math.Round(dValue / 1024) / 10, "Kbps");
            }
            else if (dValue < Math.Pow(1024, 4) * 10)
            {
                return (Math.Round(dValue / Math.Pow(1024, 2)) / 10, "Mbps");
            }
            else if (dValue < Math.Pow(1024, 5) * 10)
            {
                return (Math.Round(dValue / Math.Pow(1024, 3)) / 10, "Gbps");
            }
            else if (dValue < Math.Pow(1024, 6) * 10)
            {
                return (Math.Round(dValue / Math.Pow(1024, 4)) / 10, "Tbps");
            }
            else
            {
                return (value, "bps");
            }
        }
    }
}

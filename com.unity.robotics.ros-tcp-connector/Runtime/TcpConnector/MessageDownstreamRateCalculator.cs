using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Robotics.ROSTCPConnector
{
    public class MessageDownstreamRateCalculator : MonoBehaviour
    {
        public static MessageDownstreamRateCalculator Shared { private set; get; }

        public delegate void DataSpeedCalculatorDelegate(UInt64 dataSize, double bitsPerSec, string unit);
        public event DataSpeedCalculatorDelegate OnCalculateSpeed;

        public enum RefreshTarget : int
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
        public RefreshTarget RefreshRate = RefreshTarget._1;
        private RefreshTarget? _RefreshRate;

        private UInt64 SplitSize => (UInt64)RefreshRate;
        private float refreshInverval;
        private UInt64[] dataSizeBuffer = new UInt64[1];
        private int bufferIndex = 0;

        public UInt64 DataSize = 0;
        public double DataSpeed = 0;
        public string DataSpeedUnit = "bps";

        private float currentTime = 0f;

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

                currentTime = 0;
                refreshInverval = 1f / (float)RefreshRate;
                bufferIndex = 0;
                lock (dataLock)
                {
                    _RefreshRate = RefreshRate;
                    dataSizeBuffer = new UInt64[SplitSize];
                }
            }

            currentTime += Time.deltaTime;

            if (currentTime >= refreshInverval)
            {
                currentTime -= refreshInverval;
                UInt64 size;
                lock (dataLock)
                {
                    size = dataSizeBuffer[bufferIndex] * SplitSize;
                    dataSizeBuffer[bufferIndex] = 0;
                    bufferIndex += 1;
                    if (bufferIndex >= dataSizeBuffer.Length) bufferIndex = 0;
                }
                DataSize = size;
                var (speed, unit) = CalculateDataSpeed(size);
                DataSpeed = speed;
                DataSpeedUnit = unit;
                OnCalculateSpeed?.Invoke(size, speed, unit);

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

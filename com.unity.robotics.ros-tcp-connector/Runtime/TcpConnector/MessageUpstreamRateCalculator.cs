using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Robotics.ROSTCPConnector
{
    public class MessageUpstreamRateCalculator : MonoBehaviour
    {
        private const float INTERVAL = 1.0f;

        public static MessageUpstreamRateCalculator Shared { private set; get; }

        public delegate void DataSpeedCalculatorDelegate(UInt64 dataSize, double bitsPerSec, string unit);
        public event DataSpeedCalculatorDelegate OnCalculateSpeed;

        public UInt64 DataSize = 0;
        public double DataSpeed = 0;
        public string DataSpeedUnit = "bps";

        private UInt64 dataSize = 0;

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
            currentTime += Time.deltaTime;

            if (currentTime >= INTERVAL)
            {
                currentTime -= INTERVAL;
                lock (dataLock)
                {
                    DataSize = dataSize;
                    dataSize = 0;
                }
                var (speed, unit) = CalculateDataSpeed(DataSize);
                DataSpeed = speed;
                DataSpeedUnit = unit;
                OnCalculateSpeed?.Invoke(DataSize, speed, unit);
                
            }
        }

        public void AddBit(UInt64 bitSize)
        {
            lock (dataLock)
            {
                dataSize += bitSize;
            }
        }

        public void AddByte(UInt64 byteSize)
        {
            lock(dataLock)
            {
                dataSize += byteSize * 8;
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

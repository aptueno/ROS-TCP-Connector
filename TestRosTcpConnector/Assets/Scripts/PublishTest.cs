using RosMessageTypes.Std;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;


public class PublishTest : MonoBehaviour
{
    [SerializeField] private string topicName = "test/data";
    public float Value = 1f;
    public float PublishRate = 1f;

    private ROSConnection ros;

    private Float64Msg message;

    private float timeElapsed = 0f;
    private float? targetRate;
    private float targetTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        this.ros = ROSConnection.GetOrCreateInstance();
        this.ros.RegisterPublisher<Float64Msg>(this.topicName);
        this.message = new Float64Msg();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.targetRate != this.PublishRate)
        {
            this.targetRate = this.PublishRate;
            this.targetTime = 1f / this.targetRate.Value;
        }

        this.timeElapsed += Time.deltaTime;

        if (this.timeElapsed >= this.targetTime && this.targetRate > 0)
        {
            this.timeElapsed -= this.targetTime;

            this.message.data = this.Value;
            this.ros.Publish(this.topicName, this.message);
        }

    }
}

using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsService;

public class MqttService
{
    private static MqttService instance;
    private static IMqttClient mqttClient;
    private static object lockObj = new object();

    public static MqttService Instance()
    {
        if (mqttClient == null)
        {
            lock (lockObj)
            {
                if (mqttClient == null)
                {
                    instance = new MqttService();
                }
            }
        }

        return instance;
    }

    private MqttService()
    {
        var mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateMqttClient();
    }
          
    public async Task ConnectAsync(string brokerAddress, int port)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
                                    .WithTcpServer(brokerAddress, port)
                                    .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        Console.WriteLine("The MQTT client is connected.");
    }

    public async Task SubsribeToTopicsAsync(List<string> topics)
    {
        topics.ForEach(async t => await mqttClient.SubscribeAsync(t));
        Console.WriteLine("MQTT client subscribed to topics.");
    }

    public async Task PublishMessage(string topic, string payload)
    {
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        Console.WriteLine("MQTT client published a message.");
    }

    public void AddApplicationMessageReceived(Func<MqttApplicationMessageReceivedEventArgs, Task> callback)
    {
        mqttClient.ApplicationMessageReceivedAsync += callback;
    }
}

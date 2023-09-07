using MQTTnet.Client;
using MQTTnet;
using System.Text;
using AnalyticsService;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Drawing;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

var sensorDummyTopic = "sensor_dummy/values";
var eKuiperTopic = "eKuiper/anomalies"; // broker.emqx.io
string address = "emqx";
var port = 1883;
var client = new InfluxDBClient(url: "http://influxdb:8086", "admin", "adminadmin");
int i = 1;

var mqttService = MqttService.Instance();

await mqttService.ConnectAsync(address, port);
await mqttService.SubsribeToTopicsAsync(new List<string> { sensorDummyTopic, eKuiperTopic });

async Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
{
    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
    if (e.ApplicationMessage.Topic == sensorDummyTopic)
    {
        mqttService.PublishMessage("analytics/values", payload);
        return;
    }

    Console.WriteLine($"eKuiper send: {payload}");
    var data = (JObject)JsonConvert.DeserializeObject(payload);
    string date = data.SelectToken("Date").Value<string>();
    double temperature = data.SelectToken("Temperature").Value<double>();
    /*double humidity = data.SelectToken("Humidity").Value<double>();
    double light = data.SelectToken("Light").Value<double>();
    double co2 = data.SelectToken("CO2").Value<double>();*/

    await WriteToDatabase(date, temperature);
}

async Task WriteToDatabase(string date, double temp)
{
    var point = PointData
        .Measurement("temperature")
        .Tag("date", date)
        .Field("celsius_degrees", temp)
        .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

    await client.GetWriteApiAsync().WritePointAsync(point, "iot2", "organization");
    Console.WriteLine($"Write in InfluxDb: temperature{i}");
    i++;
}

mqttService.AddApplicationMessageReceived(ApplicationMessageReceivedAsync);

while (true) ;
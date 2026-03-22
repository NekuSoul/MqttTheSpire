namespace MqttTheSpire;

public class MqttTheSpireConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string Topic { get; set; } = "slay_the_spire_2";

    public override string ToString()
    {
        return $"Host: {Host}, Port: {Port}, Username: {User}, Topic: {Topic}";
    }
}
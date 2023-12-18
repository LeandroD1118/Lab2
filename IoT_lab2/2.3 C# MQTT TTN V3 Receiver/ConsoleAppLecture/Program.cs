// See https://aka.ms/new-console-template for more information
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using System.Text;
using System.Configuration;
using Ttn.Library;

public class Program
{
    private static readonly bool CONTAINER = false;

    /// <summary>
    /// MQTTnet usage: https://blog.behroozbc.ir/mqtt-client-with-mqttnet-4-and-c
    /// </summary>
    static async Task Main(string[] args)
    {
        Console.WriteLine($"{Environment.NewLine}" +
            $"MQTTnet ConsoleApp - A The Things Network V3 C# App {Environment.NewLine}");

        var TTN_APP_ID = "";
        var TTN_API_KEY = "";
        var TTN_REGION = "";
        var TTN_BROKER = $"{TTN_REGION}.cloud.thethings.network";
        var TOPIC = "v3/+/devices/+/up";

        IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();

        // Create client options object with keep alive 1 sec
        var builder = new MqttClientOptionsBuilder()
                        .WithTcpServer(TTN_BROKER, 1883)
                        .WithCredentials(TTN_APP_ID, TTN_API_KEY)
                        .WithCleanSession(true)
                        .WithKeepAlivePeriod(TimeSpan.FromSeconds(1));

        // auto reconnect after 5 sec if disconnected
        var options = new ManagedMqttClientOptionsBuilder()
               .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
               .WithClientOptions(builder.Build())
               .Build();

        // Set up handlers
        _mqttClient.ApplicationMessageReceivedAsync += MqttOnNewMessageAsync;
        _mqttClient.ConnectedAsync += MqttOnConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttOnDisconnectedAsync;
        _mqttClient.ConnectingFailedAsync += MqttConnectingFailedAsync;

        var topics = new List<MqttTopicFilter>();
        var opts = new MqttTopicFilterBuilder()
            .WithTopic(TOPIC)
            .Build();
        topics.Add(opts);
        await _mqttClient.SubscribeAsync(topics);
        await _mqttClient.StartAsync(options);

        if (CONTAINER)
        {
            // use for testing when running as container
            Thread.Sleep(Timeout.Infinite);
        }
        else
        {
            Console.WriteLine("Press return to exit!");
            Console.ReadLine();
            Console.WriteLine("\nAloha, Goodbye, Vaarwel!");
            Thread.Sleep(1000);
        }
    }

 
    public static Task MqttOnNewMessageAsync(MqttApplicationMessageReceivedEventArgs eArg)
    {
        var obj = eArg.ApplicationMessage;
        var ttn = TtnMessage.DeserialiseMessageV3(obj);
        var data = ttn.Payload != null ? BitConverter.ToString(ttn.Payload) : string.Empty;
        var output = DecodeHexString(data);

        Console.WriteLine($"Timestamp: {ttn.Timestamp}, Device: {ttn.DeviceID}, Topic: {ttn.Topic}, Payload: {data}");
        Console.WriteLine($"{output}");

        return Task.CompletedTask;
    }

    public static string DecodeHexString(string hexString)
    {
    if (string.IsNullOrEmpty(hexString))
    {
        return string.Empty;
    }

    hexString = hexString.Replace("-", "").Replace(" ", "");

    if (hexString.Length % 2 != 0)
    {
        throw new FormatException("Invalid hex input length");
    }

    byte[] byteArray = new byte[hexString.Length / 2];
    for (int i = 0; i < byteArray.Length; i++)
    {
        string currentHex = hexString.Substring(i * 2, 2);
        byteArray[i] = Convert.ToByte(currentHex, 16);
    }

    string decodedString = Encoding.ASCII.GetString(byteArray);

    return decodedString;
    }

    private static Task MqttOnConnectedAsync(MqttClientConnectedEventArgs eArg)
    {
        Console.WriteLine($"MQTTnet Client -> Connected with result: {eArg.ConnectResult.ResultCode}");
        return Task.CompletedTask;
    }
    private static Task MqttOnDisconnectedAsync(MqttClientDisconnectedEventArgs eArg)
    {
        Console.WriteLine($"MQTTnet Client -> Connection lost! Reason: {eArg.Reason}");
        return Task.CompletedTask;
    }
    private static Task MqttConnectingFailedAsync(ConnectingFailedEventArgs eArg)
    {
        Console.WriteLine($"MQTTnet Client -> Connection failed! Reason: {eArg.Exception}");
        return Task.CompletedTask;
    }


    /// <summary>
    /// Test Docker volumes: https://docs.docker.com/storage/volumes/
    /// read and write conf files
    /// when debug 'C:\vm\conf\names.txt'.
    /// docker run -v path/to/your/file/on/host:path/to/the/file/on/container your_image
    /// docker run -v ////c/vm/conf:/vm/conf -it --rm --name my-container-app my-app-image
    /// </summary>
    public static void TestDockerVolume()
    {
        try
        {
            // write to file
            using (var sw = new StreamWriter("/vm/conf/names.txt", true))
            {
                string[] names = new string[] { "Lars", "Hans", "Tommy", "Magnus", "Oskar" };
                foreach (string s in names)
                {
                    sw.WriteLine(s);
                }
            }

            // read from file
            using (var sr = new StreamReader("/vm/conf/names.txt"))
            {
                string? line;
                // Read and display lines from the file until the end of the file is reached
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    Thread.Sleep(1000);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be written or read:");
            Console.WriteLine(e.Message);
        }
    }


    /// <summary>
    /// Use this method for App.config files outside the app folder: https://stackoverflow.com/questions/10656077/what-is-wrong-with-my-app-config-file
    /// </summary>
    /// <param name="appSettingKey"></param>
    /// <returns>Appsetting value</returns>
    public static string GetAppSettingValue(string appSettingKey)
    {
        try
        {
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "/vm/conf/App.config"
            };

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var value = configuration.AppSettings.Settings[appSettingKey].Value;

            //var value = ConfigurationManager.AppSettings[appSettingKey];
            if (string.IsNullOrEmpty(value))
            {
                var message = $"Cannot find value for appSetting key: '{appSettingKey}'.";
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }
        catch (Exception e)
        {
            Console.WriteLine($"The appSettingKey: {appSettingKey} could not be read!");
            Console.WriteLine($"Exception: {e.Message}");
            return "";
        }
    }


    /// <summary>
    ///  /// Use this method for App.config files outside the app folder: https://stackoverflow.com/questions/10656077/what-is-wrong-with-my-app-config-file
    /// </summary>
    /// <param name="connectionStringKey"></param>
    /// <returns>connectionString value</returns>
    public static string GetConnectionStringValue(string connectionStringKey)
    {
        try
        {
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "/vm/conf/App.config"
            };

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var value = configuration.ConnectionStrings.ConnectionStrings[connectionStringKey].ConnectionString;

            //var value = ConfigurationManager.ConnectionStrings[connectionStringKey].ToString();
            if (string.IsNullOrEmpty(value))
            {
                var message = $"Cannot find value for connectionString key: '{connectionStringKey}'.";
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }
        catch (Exception e)
        {
            Console.WriteLine($"The connectionStringKey: {connectionStringKey} could not be read!");
            Console.WriteLine($"Exception: {e.Message}");
            return "";
        }
    }
}

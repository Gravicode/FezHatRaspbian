using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

using GIS = GHI.UWP.Shields.FEZHAT;
namespace TestAzureIoT
{
    class Program
    {
        private static GIS.FEZHAT hat;
        static DeviceClient deviceClient;
        static string ConnStr = "HostName=XXX.azure-devices.net;DeviceId=XXX;SharedAccessKey=XXX";
        static void Main(string[] args)
        {
            hat = GIS.FEZHAT.Create();
            hat.S1.SetLimits(500, 2400, 0, 180);
            hat.S2.SetLimits(500, 2400, 0, 180);
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.CreateFromConnectionString(ConnStr, TransportType.Amqp);
            deviceClient.ProductInfo = "HappyPath_Simulated-CSharp";
            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }
        private static async void SendDeviceToCloudMessagesAsync()
        {
          
            Random rand = new Random();

            while (true)
            {
                double x, y, z;
                Console.WriteLine("masuk loop");
                hat.GetAcceleration(out x, out y, out z);

                var Light = hat.GetLightLevel();
                var Temp = hat.GetTemperature();
                var Accel = $"({x:N2}, {y:N2}, {z:N2})";
                var Button18 = hat.IsDIO18Pressed().ToString();
                var Button22 = hat.IsDIO22Pressed().ToString();
                var Analog1 = hat.ReadAnalog(GIS.FEZHAT.AnalogPin.Ain1).ToString("N2");
               
                var telemetryDataPoint = new
                {   
                    deviceId = "FezHat",
                    temperature = Temp,
                    light = Light,
                    acceleration = Accel,
                    button18 = Button18,
                    Button22=Button22,
                    analog=Analog1
                };
                Console.WriteLine("data telemetri");
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("temperatureAlert", (Temp > 30) ? "true" : "false");
                Console.WriteLine("siap kirim");
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(2000);
            }
        }
    }
}

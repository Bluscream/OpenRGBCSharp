using OpenRGBUnity;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System;
using System.Runtime.CompilerServices;
using Color = OpenRGB.NET.Models.Color;
using Newtonsoft.Json;
using System.Threading;

namespace Bluscream
{
    public class OpenRGBFlicker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static OpenRGBLib instance;


        public static void Main()
        {
            instance = new OpenRGBLib();
            instance.Connect();
            var org = instance.Devices;
            // var _org = JsonConvert.SerializeObject(instance.Devices.Select(t => t.Device.Colors));
            foreach (var device in instance.Devices) {   
                Console.WriteLine($"{device.Device.Name} #{device.Device.Serial}: {device.Device.Leds.Length} LEDs in {device.Device.Zones.Length} zones.");
                device.Backup();
                Thread.Sleep(50);
                instance.client.SetCustomMode(device.DeviceID);
            }
            while (!Console.KeyAvailable) {
                // if (Console.ReadKey(true).Key == ConsoleKey.Escape) {  }
                /*var line = Console.ReadLine();
                // while (new List<ConsoleKey>() { ConsoleKey.W, ConsoleKey.B, ConsoleKey.Escape }.Contains(Console.ReadKey().Key))
                switch (line.ToLowerInvariant().Trim())
                {
                    case "e":
                        Environment.Exit(0); break;
                    case "b":
                        color = LEDColor.Black; break;
                    case "w":
                        color = LEDColor.White; break;
                    default:
                        break;
                }*/
                foreach (var device in instance.Devices)
                {
                    // Console.WriteLine($"{device.Device.Name}");
                    /* if (device.Device.Name == "Logitech G213") device.SetColorLoop(color, 20, 2);
                    else */
                    device.SetColor(LEDColor.RandomWhite);
                }
                Thread.Sleep(OpenRGBLib.rng.Next(100, 300));
            }
            if (org != null)
            {
                // org = JsonConvert.DeserializeObject<List<Color[]>>(_org, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor });
                foreach (var device in org)
                {
                    device.Restore();
                    Thread.Sleep(1);
                }
            }
        }
        //private bool Connect()
        //{
        //    try
        //    {
        //        client = new OpenRGBClient(ip: Ip.ToString(), port: Port, name: "C# OpenRGB Library", timeout: 5000);
        //        if (!client.Connected) throw new Exception("Disconnected!");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Unable to connect to {Ip}:{Port} ({ex.Message})");
        //        return false;
        //    }
        //    oldDevices = client.GetAllControllerData().ToList();
        //    Console.WriteLine($"Connected to {Ip}:{Port}");
        //    Console.WriteLine($"Devices: {string.Join(", ", oldDevices.Select(d => d.Name))}");
        //    return true;
        //}

        //private static void FlickerTask()
        //{
        //    for (int i = 0; i < oldDevices.Count; i++)
        //    {
        //        FlickerLEDs(oldDevices[i], i);
        //    }
        //}

        //public static void FlickerLEDs(Device device, int deviceId)
        //{
        //    var min = MinIntervalMS; var max = MaxIntervalMS;
        //    while (isRunning)
        //    {
        //        DimLEDs(device, deviceId);
        //        // yield return new WaitForSeconds(UnityEngine.Random.Range(min, max));
        //        RestoreLEDs(device, deviceId);
        //        // yield return new WaitForSeconds(UnityEngine.Random.Range(min, max));
        //    }
        //}

        //private static void RestoreAll()
        //{
        //    for (int i = 0; i < oldDevices.Count; i++)
        //    {
        //        RestoreLEDs(oldDevices[i], i);
        //    }
        //}

        //private static void RestoreLEDs(Device oldDevice, int deviceId)
        //{
        //    client.UpdateLeds(deviceId, oldDevice.Colors);
        //}

    }
}

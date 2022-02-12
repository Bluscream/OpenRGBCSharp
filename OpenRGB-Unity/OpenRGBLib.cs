using OpenRGB.NET;
using OpenRGB.NET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace OpenRGBUnity
{
    public class RGBDevice
    {
        public Device Device;
        public int DeviceID;
        public string ProfileName;
        public static OpenRGBClient Client;

        public RGBDevice(Device device, int deviceid, OpenRGBClient client)
        {
            Device = device; DeviceID = deviceid; Client = client;
            ProfileName = $"tmp_{device.Name}";
        }

        public void SetColorLoop(Color color, long count = 15, int sleepMS = 1)
        {
            for (int _i = 0; _i < count; _i++)
            {
                SetColor(color);
                Thread.Sleep(sleepMS);
            }
        }
        public void SetColor(Color color)
        {
            var leds = Enumerable.Repeat(color, Device.Leds.Length).ToArray();
            Client.UpdateLeds(DeviceID, leds);
        }

        public void Backup(string profileName = null)
        {
            profileName = profileName ?? ProfileName;
            Client.SaveProfile(profileName);
        }
        public void Restore(string profileName = null, bool deleteAfterRestore = true)
        {
            profileName = profileName ?? ProfileName;
            Client.LoadProfile(profileName);
            if (deleteAfterRestore) Client.DeleteProfile(profileName);
        }

    }
    public static class LEDColor
    {
        public static Color Black = new(0, 0, 0);
        public static Color White = new(255, 255, 255);
        public static Color RandomWhite
        {
            get
            {
                var _ = (byte)OpenRGBLib.rng.Next(1, 255);
                return new Color(_, _, _);
            }
        }
        public static Color RandomBright { get
            {
                var _ = (byte)OpenRGBLib.rng.Next(156, 255);
                return new Color(_, _, _);
            }
        }
        public static Color RandomDark
        {
            get
            {
                var _ = (byte)OpenRGBLib.rng.Next(0, 154);
                return new Color(_, _, _);
            }
        }
    }
    public class OpenRGBLib
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static OpenRGBClient _client;
        public OpenRGBClient client;
        private IPAddress Ip { get; set; } = IPAddress.Loopback;
        private short Port { get; set; } = 6742;
        private string ClientName { get; set; } = "C# OpenRGB Application";
        private int ConnectTimeOutMS { get; set; } = 1500;

        private float MinIntervalMS => 250;
        private float MaxIntervalMS => 750;
        private bool isEnabled = false;
        private bool isRunning = false;
        private object[] Timers = new object[] { };
        public static Random rng = new Random();
        public List<RGBDevice> Devices;

        public OpenRGBLib() => new OpenRGBLib(Ip, Port, ClientName, ConnectTimeOutMS);

        public OpenRGBLib(IPAddress ip, short port = 6742, string name = "C# OpenRGB Application", int timeout = 2000)
        {
            Logger.Info($"Creating OpenRGBLib instance {ip}:{port} as \"{name}\". Waiting {timeout}ms");
            if (client is null) client = Create(ip, port, name, timeout);
            if (!client.Connected) Connect(ip, port, name, timeout);
        }

        public OpenRGBLib(string ip, short port = 6742, string name = "C# OpenRGB Application", int timeout = 2000)
        {
            if (!IPAddress.TryParse(ip, out var ipv4))
                ipv4 = IPAddress.Parse(Dns.GetHostEntry(ip).AddressList.First(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString());
            Create(ipv4, port, name, timeout);
        }

        public OpenRGBClient Create() => Create(Ip, Port, ClientName, ConnectTimeOutMS);

        public OpenRGBClient Create(IPAddress ip, short port = 6742, string name = "C# OpenRGB Application", int timeout = 2000)
        {
            Ip = ip; Port = port; ClientName = name; ConnectTimeOutMS = timeout;
            if (client != null) Logger.Warn("Client already exists. Forcing recreation!");
            client = new OpenRGBClient(ip: ip.ToString(), port: port, name: name, autoconnect: false, timeout: timeout, protocolVersion: 2);
            return client;
        }

        public bool Connect() => Connect(Ip, Port, ClientName, ConnectTimeOutMS);

        public bool Connect(IPAddress ip, short port = 6742, string name = "C# OpenRGB Application", int timeout = 2000)
        {
            Ip = ip; Port = port; ClientName = name; ConnectTimeOutMS = timeout;
            if (client is null) Create(ip, port, name, timeout);
            if (client.Connected) Logger.Warn("Client already connected. Forcing reconnect!");
            Logger.Info($"Trying to connect to {ip}:{port} as \"{name}\". Waiting {timeout}ms");
            try {
                client.Connect();
                if (!client.Connected)
                {
                    Logger.Error($"Not connected");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to connect to {Ip}:{Port} ({ex.Message})");
                return false;
            }
            var _ = client.GetAllControllerData();
            Devices = new();
            for (int i = 0; i < _.Length; i++) {
                Devices.Add(new RGBDevice(_[i], i, client));
            }
            Logger.Info($"Connected to {Ip}:{Port}");
            Logger.Debug($"Devices: {string.Join(", ", Devices.Select(d => d.Device.Name))}");
            return true;
        }
    }
}

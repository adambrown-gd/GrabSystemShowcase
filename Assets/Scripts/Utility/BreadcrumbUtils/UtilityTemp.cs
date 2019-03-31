using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Photon.Realtime;

public class UtilityTemp
{
    public static string GetLocalIPAddress()
    {
        // check if connected to a network
        if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        else
            throw new Exception("Could not connect to network!");
    }

    public static string GetExternalIPAddress()
    {
        // check if connected to a network
        if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            string externalIP = new WebClient().DownloadString("http://icanhazip.com");

            if (externalIP == "")
                throw new Exception("No external IP found.");
            else
            {
                // remove letters and symbols not related to IP addresses
                externalIP = externalIP.Replace("\n", "");
                return externalIP;
            }
        }
        else
            throw new Exception("Could not connect to network!");
    }

    public static string GetPhotonPlayerIP(Player player)
    {
        return "";
    }
}

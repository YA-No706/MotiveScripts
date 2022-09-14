using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;

using System.Xml;
using System.Collections.ObjectModel;

namespace MotiveRemoteTriggerReceiveTest
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient udpReceiver = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 1512);
            string receivedData = "";
            string receivedInfo = "";
            List<string> listMessages = new List<string>();

            // Listen for broadcast messages 
            try
            {
                udpReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpReceiver.Client.Bind(RemoteIpEndPoint);
                IPEndPoint ipepSender = new IPEndPoint(IPAddress.Any, 1510);

                while (true)
                {
                    Byte[] receiveBytes = udpReceiver.Receive(ref ipepSender);
                    receivedData = Encoding.ASCII.GetString(receiveBytes);
                    receivedInfo = ipepSender.Address.ToString() + ":" + ipepSender.Port.ToString();

                    // preserve message from server
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(receivedData);
                        XmlNode node = doc.SelectSingleNode("CaptureStart");
                        if (node != null)
                        {
                            string message = System.DateTime.Now.ToLongTimeString() + " [MotiveIP:" + receivedInfo + "] Capture Take Started\r\n";
                            string name = node["Name"].GetAttribute("VALUE");
                            message += " Take Name : " + name + "\r\n";
                            string timecode = node["TimeCode"].GetAttribute("VALUE");
                            message += " TimeCode : " + timecode + "\r\n";

                            listMessages.Add(message);
                        }

                        node = doc.SelectSingleNode("CaptureStop");
                        if (node != null)
                        {
                            string message = System.DateTime.Now.ToLongTimeString() + " [MotiveIP:" + receivedInfo + "] Capture Take Stopped\r\n";
                            string name = node["Name"].GetAttribute("VALUE");
                            message += " Take Name : " + name + "\r\n";
                            string timecode = node["TimeCode"].GetAttribute("VALUE");
                            message += " TimeCode : " + timecode + "\r\n";

                            listMessages.Add(message);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        listMessages.Add("XML parse exception : " + ex.Message);
                    }

                    //-----print message-----

                    if (listMessages.Count == 0)
                        continue;

                    foreach (var message in listMessages)
                    {
                        Console.WriteLine(message);
                    }
                    Console.WriteLine("-------------------------------------------");
                    listMessages.Clear();

                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception during receive message attempt : " + ex.ToString());
            }


            if (udpReceiver != null)
                udpReceiver.Close();

        }
    }
}

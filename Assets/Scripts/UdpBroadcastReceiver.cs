using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System;

public class UdpBroadcastReceiver : MonoBehaviour
{
    public bool isUsingLauncher = true; // 控制是否启动可执行文件的开关
    private UdpClient udpClient;
    private Thread receiveThread;
    private volatile bool isRunning = true; // 使用volatile确保线程间的可见性
    public int counter = 0;
    private Process executableProcess;

    void Start()
    {
        Application.runInBackground = true;
        
        if (isUsingLauncher)
        {
            // 只在使用启动器模式时执行这些操作
            KillExistingProcesses();
            StartExecutable();
        }

        // 无论如何都启动UDP监听
        StartUdpListener();
    }

    private void StartUdpListener()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            isRunning = false;
            receiveThread.Join();
        }

        isRunning = true;
        receiveThread = new Thread(new ThreadStart(ReceiveBroadcasts));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void StartExecutable()
    {
        string executablePath = Application.streamingAssetsPath + "/main";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // 启动可执行文件并保存进程引用
        executableProcess = new Process
        {
            StartInfo = startInfo
        };
        executableProcess.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
        executableProcess.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);
        executableProcess.Start();
        executableProcess.BeginOutputReadLine();
        executableProcess.BeginErrorReadLine();
    }

    // 接收广播的函数
    private void ReceiveBroadcasts()
    {
        udpClient = new UdpClient(65432);  // 使用和可执行文件相同的端口
        udpClient.EnableBroadcast = true;

        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 65432);
        UnityEngine.Debug.Log("Listening for UDP broadcasts...");

        while (isRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);
                UnityEngine.Debug.Log($"Received broadcast: {message}");
                counter++; // 每次接收到信息时增加计数器
            }
            catch (SocketException ex)
            {
                UnityEngine.Debug.LogError($"Socket error: {ex.Message}");
                isRunning = false;
            }
        }
    }

    private void KillExistingProcesses()
    {
        // 获取所有进程，检查是否有正在运行的同名进程
        Process[] allProcesses = Process.GetProcessesByName("main");
        foreach (Process proc in allProcesses)
        {
            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                    UnityEngine.Debug.Log("Killed existing process: " + proc.Id);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Error killing process: " + ex.Message);
            }
        }
    }

    private void OnDisable()
    {
        CleanupResources();
    }
    
    private void OnApplicationQuit()
    {
        CleanupResources();
    }

    private void CleanupResources()
    {
        // 停止UDP监听
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            try
            {
                udpClient?.Close(); // 关闭socket以中断Receive调用
                receiveThread.Join(1000); // 等待最多1秒
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error during thread cleanup: {ex.Message}");
            }
        }

        // 清理UDP客户端
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }

        // 只在使用启动器模式时清理进程
        if (isUsingLauncher)
        {
            if (executableProcess != null && !executableProcess.HasExited)
            {
                try
                {
                    executableProcess.Kill();
                    KillExistingProcesses();
                    UnityEngine.Debug.Log("Terminated the executable process.");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error killing process: {ex.Message}");
                }
            }
        }
    }
}
using System.Collections;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Text;
using System;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif


public class NetworkUtils : MonoBehaviour
{
    public event Action<string> OnMessageReceived = delegate { };

    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;

    private string portNumber;

    public async void StartServer(string portNumber)
    {
        this.portNumber = portNumber;
        try
        {
#if WINDOWS_UWP
            streamSocketListener = new StreamSocketListener();
            // The ConnectionReceived event is raised when connections are received.
            streamSocketListener.ConnectionReceived += StreamSocketListener_ConnectionReceived;
            // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
            await streamSocketListener.BindServiceNameAsync(portNumber);
#else
            tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
            tcpListenerThread.IsBackground = true;
            tcpListenerThread.Start();
#endif
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

#if WINDOWS_UWP
    private StreamSocketListener streamSocketListener;

    private async void StreamSocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        using (var dataReader = new DataReader(args.Socket.InputStream))
        {
            dataReader.InputStreamOptions = InputStreamOptions.Partial;
            while (true)
            {
                await dataReader.LoadAsync(256);
                if (dataReader.UnconsumedBufferLength == 0) break;
                IBuffer requestBuffer = dataReader.ReadBuffer(dataReader.UnconsumedBufferLength);
                string request = Windows.Security.Cryptography.CryptographicBuffer.ConvertBinaryToString(Windows.Security.Cryptography.BinaryStringEncoding.Utf8, requestBuffer);

                if (OnMessageReceived != null)
                {
                    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                    {
                        OnMessageReceived(request);
                    }, false);
                }
            }
        }
    }
#else
    private void ListenForIncommingRequests()
    {
        TcpListener server = null;
        try
        {
            // Set the TcpListener on port 13000.
            IPAddress localAddr = IPAddress.Any;

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, int.Parse(portNumber));

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            Debug.Log("Waiting for a connection... ");
            // Enter the listening loop.
            while (true)
            {
                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();

                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = Encoding.ASCII.GetString(bytes, 0, i);

                    UnityMainThreadDispatcher.Instance().Enqueue(SendMessageOnTheMainThread(data));
                }

                // Shutdown and end connection
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }
    }

    private IEnumerator SendMessageOnTheMainThread(string message)
    {
        OnMessageReceived?.Invoke(message);
        yield return null;
    }
#endif
}

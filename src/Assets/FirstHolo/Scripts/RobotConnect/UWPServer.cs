//    Copyright 2016 United States Government as represented by the
//    Administrator of the National Aeronautics and Space Administration.
//    All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.



using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using Xml2CSharp;
using FirstUtilities;

#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System;
using System.Threading.Tasks;
#elif UNITY_EDITOR // allow similation inside editor
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
#endif

[RequireComponent(typeof(ValuePublisher))]
public class UWPServer : Singleton<UWPServer> {

    public bool runAtStartup = true;
    public bool emulateServerConnection = false;
	public static ValuePublisher Publisher 
	{
		get { 
			if (Instance != null)
				return Instance._pub;
			else
				return null;
		}
	}

#if WINDOWS_UWP
    private DatagramSocket _udpSocket = null;
    private StreamSocketListener _tcpSocket = null;
    private List<StreamSocket> _connections = new List<StreamSocket>();
#elif UNITY_EDITOR // allow similation inside editor
    private TcpListener _tcpListener = null;
    private UdpClient _updListener = null;
    private Thread _thread = null;
#endif

	private bool _serverIsEnabled = false;
    private ValueItem[] _items = null;
    private Config _loadedConfig = null;
    private PROTOCOL _activeConnection = PROTOCOL.NONE;
	private ValuePublisher _pub = null;

#region MonoSingleton Implementation
    private void Awake()
    {

        TextAsset ta = (TextAsset)Resources.Load<TextAsset>("Config/message_config");
        if (ta)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            _loadedConfig = (Config)xs.Deserialize(new System.IO.StringReader(ta.text));
            _items = new ValueItem[_loadedConfig.Message.Value.Count];
		}

		_pub = GetComponent<ValuePublisher> ();

#if UNITY_EDITOR
        if (ta == null) throw new System.Exception("No config for UWPServer, Editor requires a config file.");
        if (emulateServerConnection) return;
#endif

        if (runAtStartup)
            Listen(_loadedConfig);
	}

    private void OnApplicationQuit()
	{
		Close();
	}
#endregion

    void Listen(string port)
    {
        Listen(port, PROTOCOL.UDP);
    }

    void Listen(Config config)
    {
        Listen(config.Dest_port.Value, config.Comm_mode.Value == "TCP" ? PROTOCOL.TCP : PROTOCOL.UDP);
    }

    void Listen(string port, PROTOCOL proto)
    {
        if (_activeConnection != PROTOCOL.NONE) return;

        _serverIsEnabled = true;

#if WINDOWS_UWP
        if (proto == PROTOCOL.TCP)
        {

            if (_tcpSocket == null)
            {
                _tcpSocket = new StreamSocketListener();
                _tcpSocket.ConnectionReceived += _tcpSocket_ConnectionReceived;
            }

            try
            {
                _tcpSocket.BindServiceNameAsync(port);
            }
            catch(ObjectDisposedException e)
            {
                _tcpSocket = new StreamSocketListener();
                _tcpSocket.BindServiceNameAsync(port);
            }
            catch(InvalidOperationException e)
            {
                // try again later
                StartCoroutine(TryAgain());
            }
            catch(System.Exception e)
            {
                throw e;
            }
            
            _activeConnection = PROTOCOL.TCP;
        }
        else
        {

            if (_udpSocket == null)
            {
                _udpSocket = new DatagramSocket();
                _udpSocket.MessageReceived += _udpSocket_MessageReceived;
            }

            _udpSocket.BindServiceNameAsync(port);

            _activeConnection = PROTOCOL.UDP;
        }
#elif UNITY_EDITOR // allow similation inside editor

        _activeConnection = proto;

        // Editor only uses loaded config
        ThreadStart waitForMsgTS = new ThreadStart(WaitForMsg);
        _thread = new Thread(waitForMsgTS);
        _thread.Start();
#endif
    }

    void Close()
    {
		if (emulateServerConnection)
			return;
		
        Debug.Log("Close connection");
        _serverIsEnabled = false;
#if UNITY_EDITOR // allow similation inside editor
        _thread.Join(500);
        _activeConnection = PROTOCOL.NONE;
#endif
        
	}

	System.Collections.IEnumerator TryAgain()
	{
		Close();
		// wait for server to close
		yield return new WaitForSeconds(0.3f);
		Listen(_loadedConfig);
	}

    /// <summary>
    /// On Hololens this is called when using bloom gesture (leaving app)
    /// </summary>
    /// <param name="pauseStatus"></param>
    void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_EDITOR
        if (emulateServerConnection) return;
#endif
        if(runAtStartup)
        {
            Debug.Log("UWPServer is now: " + (pauseStatus ? "paused" : "running"));
            if (pauseStatus) Close();
            else Listen(_loadedConfig);
        }
    }

    // Update is called once per frame
    void Update() {
		if (emulateServerConnection) {
			int i = 0;
			foreach (Value vi in _loadedConfig.Message.Value)
			{
				// maybe later use a range or 
				// take in a randomizing class
				_pub.Publish(i,new ValueItem(vi.Name, vi.Default, vi.Default));
				++i;
			}
		}
    }

#if WINDOWS_UWP
    private async void _udpSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        print(string.Format("Incoming UDP connection from {0}:{1}", args.RemoteAddress.DisplayName, args.RemotePort));
        if (!_serverIsEnabled)
        {
            await _udpSocket.CancelIOAsync();
            _udpSocket.Dispose();
            _activeConnection = PROTOCOL.NONE;
            return;
        }
        WaitForDataUDP(args.GetDataReader());
    }

    private void _tcpSocket_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        print(string.Format("Incoming TCP connection from {0}:{1}", args.Socket.Information.RemoteAddress.DisplayName, args.Socket.Information.RemotePort));
        _connections.Add(args.Socket);
        WaitForDataTCP(args.Socket);
    }
#elif UNITY_EDITOR // allow similation inside editor
    private void WaitForMsg()
    {
        if (_activeConnection == PROTOCOL.TCP)
        {
            try
            {
                print("TCP listen on: " + _loadedConfig.Dest_port.Value);
                _tcpListener = new TcpListener(IPAddress.Any, int.Parse(_loadedConfig.Dest_port.Value));
                _tcpListener.Start();
                while (_serverIsEnabled)
                {
                    if (!_tcpListener.Pending())
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        TcpClient client = _tcpListener.AcceptTcpClient();
                        NetworkStream ns = client.GetStream();
                        NetworkEndianReader reader = new NetworkEndianReader(ns);
                        ValueItem [] data = ParseData(reader);

                        for (int i = 0; i < data.Length; i++)
                        {
							_pub.Publish(i,data[i]);
                        }


                        reader.Close();
                        client.Close();
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                _tcpListener.Stop();
            }
        }
        else // UDP
        {
            try
            {
                print("UDP listen on: " + _loadedConfig.Dest_port.Value);
                _updListener = new UdpClient(int.Parse(_loadedConfig.Dest_port.Value));
                while (_serverIsEnabled)
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // Blocks until a message returns on this socket from a remote host.
                    byte[] receiveBytes = _updListener.Receive(ref RemoteIpEndPoint);

                    NetworkEndianReader reader = new NetworkEndianReader(new MemoryStream(receiveBytes));
                    
                    ValueItem[] data = ParseData(reader);

                    for (int i = 0; i < data.Length; i++)
                    {
						_pub.Publish(i,data[i]);
                    }

                    reader.Close();
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                _updListener.Close();
            }
        }
    }
#endif


#if WINDOWS_UWP
    async private void WaitForDataUDP(DataReader reader)
    {

        ValueItem[] data = await ParseData(reader);

        for (int i = 0; i < data.Length; i++)
        {
			_pub.Publish(i,data[i]);
        }
    }

    async private void WaitForDataTCP(StreamSocket socket)
    {
        DataReader reader = new DataReader(socket.InputStream);
        try
        {
            while (_serverIsEnabled)
            {
                ValueItem[] data = await ParseData(reader);

                for (int i = 0; i < data.Length; i++)
                {
					_pub.Publish(i,data[i]);
                }

            }
        }
        catch(System.Exception e)
        {
            // currently not handling errors
            throw e;
        }

        if (_tcpSocket != null)
        {
            await _tcpSocket.CancelIOAsync();
            _connections.Clear();
            _tcpSocket.ConnectionReceived -= _tcpSocket_ConnectionReceived;
            _tcpSocket.Dispose();
            _tcpSocket = null;
        }
      
        await socket.CancelIOAsync();
        socket.Dispose();

        _activeConnection = PROTOCOL.NONE;

    }

    private async Task<ValueItem[]> ParseData(DataReader reader)
    {
        reader.ByteOrder = ByteOrder.BigEndian;
        reader.InputStreamOptions = InputStreamOptions.Partial;

        var headerLength = await reader.LoadAsync(6);

        if (headerLength != 6)
        {
            print("header length not correct: " + headerLength);
            return new ValueItem[0];
        }

        List<ValueItem> dataParsed = new List<ValueItem>();

        reader.ReadInt16(); // two byte sync pattern
        ushort count = reader.ReadUInt16(); // message count
        ushort size = reader.ReadUInt16(); // message size

        size -= 6;

        var msgLength = await reader.LoadAsync((uint)size);
        if(msgLength != size)
        {
            print("msg length not correct: " + msgLength + " vs " + size);
            return new ValueItem[0];
        }

        foreach (var value in _loadedConfig.Message.Value)
        {
            switch (value.Type)
            {
                case "float":
                case "single":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadSingle(), float.Parse(value.Default)));
                    break;
                case "bool":
                case "boolean":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadBoolean(), bool.Parse(value.Default)));
                    break;
                case "double":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadDouble(), double.Parse(value.Default)));
                    break;
                case "short":
                case "int16":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadInt16(), short.Parse(value.Default)));
                    break;
                case "int":
                case "int32":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadInt32(), int.Parse(value.Default)));
                    break;
                case "long":
                case "int64":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadInt64(), long.Parse(value.Default)));
                    break;
                default:
                    return null;
            }
        }

        return dataParsed.ToArray();
    }
#elif UNITY_EDITOR // allow similation inside editor
    private ValueItem[] ParseData(NetworkEndianReader reader)
    {
        List <ValueItem>  dataParsed = new List<ValueItem>();

        reader.ReadInt16(); // two byte sync pattern
        ushort count = reader.ReadUInt16(); // message count
        reader.ReadInt16(); // message size

        foreach (var value in _loadedConfig.Message.Value)
        {
            switch (value.Type)
            {
                case "float":
                case "single":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadSingle(), float.Parse(value.Default)));
                    break;
                case "bool":
                case "boolean":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadBoolean(), bool.Parse(value.Default)));
                    break;
                case "double":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadDouble(), double.Parse(value.Default)));
                    break;
                case "short":
                case "int16":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadInt16(), short.Parse(value.Default)));
                    break;
                case "int":
                case "int32":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadInt32(), int.Parse(value.Default)));
                    break;
                case "long":
                case "int64":
                    dataParsed.Add(new ValueItem(value.Name, reader.ReadInt64(), long.Parse(value.Default)));
                    break;
                default:
                    return null;
            }
        }

        return dataParsed.ToArray();
    }
#endif

    protected UWPServer() { }
}

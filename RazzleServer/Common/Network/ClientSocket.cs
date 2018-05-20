using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.MapleCryptoLib;
using RazzleServer.Common.Packet;

namespace RazzleServer.Common.Network
{
	public class ClientSocket : IDisposable
	{
		private readonly Socket _socket;
		private readonly byte[] _socketBuffer;
        private readonly IPEndPoint _endpoint;
		private readonly string _host;
        private readonly byte[] _hostBytes;
		private readonly ushort _port;
		private readonly object _disposeSync;
        private readonly AClient _client;
        private readonly bool _toClient;
		private bool disposed;

		private ILogger Log = LogManager.Log;

		public MapleCipherProvider Crypto { get; private set; }
		public bool Connected => !disposed;
        public IPEndPoint Endpoint => _endpoint;
		public string Host => _host;
        public byte[] HostBytes => _hostBytes;
		public ushort Port => _port;

        public ClientSocket(Socket socket, AClient client, ushort currentGameVersion, ulong aesKey, bool toClient)
		{
			_socket = socket;
			_socketBuffer = new byte[1024];
            _endpoint = socket.RemoteEndPoint as IPEndPoint;
            _host = _endpoint.Address.ToString();
            _hostBytes = _endpoint.Address.GetAddressBytes();
			_port = (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
			_disposeSync = new object();
			_client = client;
            _toClient = toClient;

            Crypto = new MapleCipherProvider(currentGameVersion, aesKey);
            Crypto.PacketFinished += (data) => _client.Receive(new PacketReader(data));
			WaitForData();
		}
		private void WaitForData()
		{
            if (!disposed)
			{
				var error = SocketError.Success;

				var socketArgs = new SocketAsyncEventArgs
				{
					SocketFlags = SocketFlags.None
				};

				socketArgs.SetBuffer(_socketBuffer, 0, _socketBuffer.Length);
				socketArgs.Completed += PacketReceived;
				_socket.ReceiveAsync(socketArgs);

				if (error != SocketError.Success)
				{
					Disconnect();
				}
			}
		}

		private void PacketReceived(object sender, SocketAsyncEventArgs e)
		{
			if (!disposed)
			{
				var size = e.BytesTransferred;

                if (size == 0 || e.SocketError != SocketError.Success)
				{
					Disconnect();
                }
				else
				{
					Crypto.AddData(_socketBuffer, 0, size);
					WaitForData();
				}
			}
		}

		public void SendRawPacket(byte[] final)
		{
            if (!disposed)
			{
				var offset = 0;

                while (offset < final.Length)
				{
					var outError = SocketError.Success;
					var sent = _socket.Send(final, offset, final.Length - offset, SocketFlags.None, out outError);

					if (sent == 0 || outError != SocketError.Success)
					{
						Disconnect();
						return;
					}

					offset += sent;
				}
			}
		}

		public void Send(PacketWriter data)
		{
			if (!disposed)
			{
				var buffer = data.ToArray();
				Crypto.Encrypt(ref buffer, _toClient);
				SendRawPacket(buffer);
			}
		}

		public void Disconnect()
		{
			Log.LogInformation("Client Disconnected");
			Dispose();
		}

		public void Dispose()
		{
			lock (_disposeSync)
			{
				if (disposed)
				{
					return;
				}

				disposed = true;

				try
				{
					_socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
				}
				finally
				{
					_client.Disconnected();
				}
			}
		}
	}
}

/*
 * Copyright 2007,2008 Davelopware Ltd
 * 
 * http://www.davelopware.com/txtmnu/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed
 * under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 *
 */
using System;
using System.Net.Sockets;
using System.IO;

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// Collects all of the information about a client connection together in one place
	/// </summary>
	/// <remarks>
	/// In particular this provides a single place for both TcpClient & the synchronised
	/// in/out writers to be kept together and consistent. Makes it easy to provide
	/// multithreaded access to the writers outside of the code under out control, and
	/// also a simple interface for killing the connection (via the TcpClient)
	/// </remarks>
	public class ClientSocketConnection
	{
		private TcpClient _client;
		private TextWriter _outWriter;
		private TextReader _inReader;
		private Stream _rawStream;
		private NetworkStream _networkStream;
		private object _consistencyLock = new object();

		public ClientSocketConnection(TcpClient client)
		{
			_client = client;

			// Get synchronised input and output streams for the client
			_networkStream = client.GetStream();
			_rawStream = _networkStream;
			_outWriter = TextWriter.Synchronized(new StreamWriter(_networkStream));
			_inReader = TextReader.Synchronized(new StreamReader(_networkStream));
		}

		public TcpClient Client
		{
			get { lock (_consistencyLock) { return _client; } }
			set { lock (_consistencyLock) { _client = value; } }
		}

		public TextWriter OutWriter
		{
			get { lock (_consistencyLock) { return _outWriter; } }
			set { lock (_consistencyLock) { _outWriter = value; } }
		}

		public TextReader InReader
		{
			get { lock (_consistencyLock) { return _inReader; } }
			set { lock (_consistencyLock) { _inReader = value; } }
		}

		public Stream RawStream
		{
			get { lock (_consistencyLock) { return _rawStream; } }
			set { lock (_consistencyLock) { _rawStream = value; } }
		}

		public NetworkStream NetworkStream
		{
			get { lock (_consistencyLock) { return _networkStream; } }
			set { lock (_consistencyLock) { _networkStream = value; } }
		}

		public void Close()
		{
			lock (_consistencyLock)
			{
				if (_client != null)
				{
					try
					{
						_client.Close();
					}
					catch
					{}
				}

				_client = null;
				_inReader = null;
				_outWriter = null;
			}
		}
	}
}

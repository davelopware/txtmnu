/*
 * Copyright 2008 Davelopware Ltd
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
using System.IO;
using System.Net.Sockets;

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// 
	/// </summary>
	public class TelnetConnection : Stream
	{
		public const byte IAC = 255;

		public const byte CMD_SE = 240;
		public const byte CMD_NOP = 241;
		public const byte CMD_DataMark = 242;
		public const byte CMD_Break = 243;
		public const byte CMD_InterruptProcess = 244;
		public const byte CMD_AbortOutput = 245;
		public const byte CMD_AreYouThere = 246;
		public const byte CMD_EraseCharacter = 247;
		public const byte CMD_EraseLine = 248;
		public const byte CMD_GoAhead = 249;
		public const byte CMD_SB = 250;
		public const byte CMD_WILL = 251;
		public const byte CMD_WONT = 252;
		public const byte CMD_DO = 253;
		public const byte CMD_DONT = 254;

		public const byte OPTCODE_Echo = 1;
		public const byte OPTCODE_SupressGoAhead = 3;
		public const byte OPTCODE_TerminalType = 24;
		public const byte OPTCODE_NegotiateAboutWindowSize = 31;
		public const byte OPTCODE_TerminalSpeed = 32;

		public const string VT100_Escape = "\u001b";

		private class ConnectionClosedException : Exception
		{
			public ConnectionClosedException()
			{
			}
		}

		private const int PRE_RED_BUFFER_SIZE = 512;

		private TcpClient _client = null;
		private NetworkStream _stream = null;
		private bool _remoteConnectionClosed = false;
		private Byte[] _preReadBuffer = new byte[PRE_RED_BUFFER_SIZE];
		private long _preReadWritePosition = -1;
		private long _preReadReadPosition = -1;

		private object _streamSync = new object();

		private bool _echoBackToRemote = false;

		public delegate void TelnetSimplestCommandEventHandler();
		public delegate void TelnetSimpleCommandEventHandler(byte command);
		public delegate void TelnetOptionCodeCommandEventHandler(byte command, byte optionCode);

		public event TelnetSimplestCommandEventHandler OnWillEchoReceived;
		public event TelnetSimplestCommandEventHandler OnWontEchoReceived;
		public event TelnetSimplestCommandEventHandler OnDoEchoReceived;
		public event TelnetSimplestCommandEventHandler OnDontEchoReceived;

		public TelnetConnection(TcpClient client)
		{
			_client = client;
			_stream = _client.GetStream();
		}

		public void Negotiate()
		{
			if (_stream == null)
				return;

			SendTelnetCommand(CMD_WILL, OPTCODE_Echo);
			SendTelnetCommand(CMD_WILL, OPTCODE_SupressGoAhead);
			CheckForTelnetCommands();
		}

		public bool EchoBackToRemote
		{
			get { return _echoBackToRemote; }
			set { SetEchoBackToRemote(value); }
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get
			{
				lock (_streamSync)
				{
					if (_stream == null)
						return false;
					return _stream.CanSeek;
				}
			}
		}

		public override void Flush()
		{
			lock (_streamSync)
			{
				if (_stream == null)
					return;
				_stream.Flush();
			}
		}

		public override long Length
		{
			get
			{
				lock (_streamSync)
				{
					if (_stream == null)
						return -1;
					return _stream.Length;
				}
			}
		}

		public override long Position
		{
			get
			{
				lock (_streamSync)
				{
					if (_stream == null)
						return -1;
					return _stream.Position;
				}
			}
			set
			{
				lock (_streamSync)
				{
					if (_stream == null)
						return;
					_stream.Position = value;
				}
			}
		}

		public override int Read(byte[] buf, int start, int len)
		{
			System.Diagnostics.Debug.WriteLine("TelnetConnection: Read called with " + start.ToString() + ", " + len.ToString());

			if (_stream == null)
				return 0;

			//byte[] passThroughBuffer = new byte[len];
			byte[] passThroughBuffer = buf;
			int lenRetrieved = 0;

			while (lenRetrieved < len && !_remoteConnectionClosed)
			{
				try
				{
					lock (_streamSync)
					{
						byte b = GetNextReadByte();
						if (b != IAC || !ProcessIncommingTelnetCommand())
						{
							passThroughBuffer[lenRetrieved + start] = b;
							lenRetrieved++;
							if (_echoBackToRemote)
								_stream.WriteByte(b);
						}
						// once we've got something, only keep going if there's more data available
						if (lenRetrieved > 0 && !MoreReadBytesAvailable)
							break;
					}
				}
				catch (ConnectionClosedException)
				{
				}
			}

			return lenRetrieved;
		}

		public override Int64 Seek(long offset, SeekOrigin origin)
		{
			lock (_streamSync)
			{
				if (_stream == null)
					return -1;

				return _stream.Seek(offset, origin);
			}
		}

		public override void SetLength(long len)
		{
			lock (_streamSync)
			{
				if (_stream == null)
					return;

				_stream.SetLength(len);
			}
		}

		public override void Write(byte[] buf, int start, int len)
		{
			System.Diagnostics.Debug.WriteLine("TelnetConnection: Write called with " + start.ToString() + ", " + len.ToString());
			lock (_streamSync)
			{
				if (_stream == null)
					return;

				int pos = start;
				bool gotHat = false;
				while (pos < len)
				{
					byte b = buf[pos++];
					if (b == (byte)'^')
					{
						gotHat = true;
						continue;
					}
					else if (gotHat)
					{
						if (b == (byte)'[') // so we replace "^[" with VT100 Escape character
						{
							System.Diagnostics.Debug.WriteLine("TelnetConnection: sending an inline vt100 command");
							WriteVT100Escape();
							gotHat = false;
							continue;
						}
						else // got the '^' without a subsequent '[', so send the '^' we held back !
						{
							_stream.WriteByte((byte)'^');
							gotHat = false;
						}
					}
					_stream.WriteByte(b);
				}
			}
		}

		public void SendTelnetCommand(byte command)
		{
			lock (_streamSync)
			{
				if (_stream == null)
					return;

				System.Diagnostics.Debug.WriteLine("TelnetConnection: sending telnet command:" + command.ToString());
				_stream.WriteByte(IAC);
				_stream.WriteByte(command);
			}
		}

		public void SendTelnetCommand(byte command, byte optionCode)
		{
			lock (_streamSync)
			{
				if (_stream == null)
					return;

				System.Diagnostics.Debug.WriteLine("TelnetConnection: sending telnet command:" + command.ToString() + " optionCode:" + optionCode.ToString());
				_stream.WriteByte(IAC);
				_stream.WriteByte(command);
				_stream.WriteByte(optionCode);
			}
		}

		public void WriteVT100Escape()
		{
			lock (_streamSync)
			{
				if (_stream == null)
					return;

				byte[] buf = System.Text.Encoding.ASCII.GetBytes(VT100_Escape);
				Flush();
				Write(buf, 0, buf.Length);
				Flush();
			}
		}

		public void WriteVT100Command(string command)
		{
			lock (_streamSync)
			{
				if (_stream == null)
					return;

				byte[] buf = System.Text.Encoding.ASCII.GetBytes(VT100_Escape + command);
				Flush();
				Write(buf, 0, buf.Length);
				Flush();
			}
		}

		public override void Close()
		{
			lock (_streamSync)
			{
				base.Close ();
				try
				{
					if (_stream != null)
					{
						_stream.Flush();
						_stream.Close();
					}
					if (_client != null)
						_client.Close();
				}
				catch
				{
				}
				finally
				{
					_stream = null;
					_client = null;
				}
			}
		}


		private void CheckForTelnetCommands()
		{
			lock (_streamSync)
			{
				if (_stream.DataAvailable)
				{
					byte b = InnerReadByte();
					if (b != IAC || !ProcessIncommingTelnetCommand())
					{
						if (_preReadWritePosition == -1)
							_preReadWritePosition = 0;

						_preReadBuffer[_preReadWritePosition] = b;
						_preReadWritePosition++;

						if (_preReadReadPosition == -1)
							_preReadReadPosition = 0;

						if (_echoBackToRemote)
							_stream.WriteByte(b);
						if (_preReadWritePosition == PRE_RED_BUFFER_SIZE)
							return;
					}
				}
			}
		}

		private byte GetNextReadByte()
		{
			lock (_streamSync)
			{
				if (PreReadBufferDataAvailable)
				{
					return PopByteFromPreReadBuffer();
				}

				return InnerReadByte();
			}
		}

		private bool MoreReadBytesAvailable
		{
			get
			{
				lock (_streamSync)
				{
					if (PreReadBufferDataAvailable)
						return true;
					else
						return _stream.DataAvailable;
				}				
			}
		}

		private bool PreReadBufferDataAvailable
		{
			get { return _preReadReadPosition < _preReadWritePosition; }
		}

		private byte PopByteFromPreReadBuffer()
		{
			lock (_streamSync)
			{
				if (!PreReadBufferDataAvailable)
					throw new InvalidOperationException("Can't Pop a byte from the pre-read buffer if there isn't any data in it. Check .PreReadBufferDataAvailable first!");

				byte b = _preReadBuffer[_preReadReadPosition++];

				if (_preReadReadPosition == _preReadWritePosition)
				{
					// we've read out all of the pre-read data, so clear everything down to the start again
					_preReadReadPosition = -1;
					_preReadWritePosition = -1;
				}

				return b;
			}
		}

		private byte InnerReadByte()
		{
			lock (_streamSync)
			{
				int ib = _stream.ReadByte();
				if (ib == -1)
				{
					_remoteConnectionClosed = true;
					throw new ConnectionClosedException();
				}
				else
				{
					return (byte)ib;
				}
			}
		}

		private bool ProcessIncommingTelnetCommand()
		{
			lock (_streamSync)
			{
				byte command = InnerReadByte();
				// check if this is just a double IAC (ie escaping real IAC data in the transmission line)
				if (command == IAC)
					return false;

				ProcessReceivedTelnetCommand(command);
				return true;
			}
		}

		private void ProcessReceivedTelnetCommand(byte command)
		{
			lock (_streamSync)
			{
				System.Diagnostics.Debug.WriteLine("TelnetConnection: received telnet command:" + command.ToString());
				byte optionCode = 0;
				// first check if we need to get the optionCode...
				switch (command)
				{
					case CMD_WILL:
					case CMD_WONT:
					case CMD_DO:
					case CMD_DONT:
						optionCode = ReadTelnetCommandOptionCode();
						break;
				}

				switch (command)
				{
					case CMD_NOP:
						break;
					case CMD_WILL:
						OnReceivedWill(optionCode);
						break;
					case CMD_WONT:
						OnReceivedWont(optionCode);
						break;
					case CMD_DO:
						OnReceivedDo(optionCode);
						break;
					case CMD_DONT:
						OnReceivedDont(optionCode);
						break;
				}
			}
		}

		private byte ReadTelnetCommandOptionCode()
		{
			lock (_streamSync)
			{
				byte optionCode = InnerReadByte();
				System.Diagnostics.Debug.WriteLine("TelnetConnection: received telnet optionCode:" + optionCode.ToString());
				return optionCode;
			}
		}

		private void OnReceivedWill(byte optionCode)
		{
			switch (optionCode)
			{
				case OPTCODE_Echo:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Will Echo");
					if (OnWillEchoReceived != null)
						OnWillEchoReceived();
					break;
				case OPTCODE_SupressGoAhead:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Will Supress GoAhead");
					break;
			}

		}

		private void OnReceivedWont(byte optionCode)
		{
			switch (optionCode)
			{
				case OPTCODE_Echo:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Wont Echo");
					if (OnWontEchoReceived != null)
						OnWontEchoReceived();
					break;
				case OPTCODE_SupressGoAhead:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Wont Supress GoAhead");
					break;
			}
		}

		private void OnReceivedDo(byte optionCode)
		{
			switch (optionCode)
			{
				case OPTCODE_Echo:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Do Echo");
					if (OnDoEchoReceived != null)
						OnDoEchoReceived();
					SendTelnetCommand(CMD_WILL, OPTCODE_Echo);
					_echoBackToRemote = true;
					break;
				case OPTCODE_SupressGoAhead:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Do Supress GoAhead");
					SendTelnetCommand(CMD_WILL, OPTCODE_SupressGoAhead);
					break;
			}
		}

		private void OnReceivedDont(byte optionCode)
		{
			switch (optionCode)
			{
				case OPTCODE_Echo:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Dont Echo");
					if (OnDontEchoReceived != null)
						OnDontEchoReceived();
					break;
				case OPTCODE_SupressGoAhead:
					System.Diagnostics.Debug.WriteLine("TelnetConnection: received Dont Supress GoAhead");
					break;
			}
		}

		private void SetEchoBackToRemote(bool echoBackToRemote)
		{
			bool previousEchoBackToRemote = _echoBackToRemote;
			bool newEchoBackToRemote = _echoBackToRemote;
			_echoBackToRemote = echoBackToRemote;
		}

	}
}

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
using System.Threading;
using System.IO;
using System.Text;

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// Implements a basic Socket Listener and hooks incoming connections up to a menu
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class SocketListenerHelper
	{
		private class IncommingConnectionEventArgs : EventArgs
		{
			public ClientSocketConnection Connection;

			public IncommingConnectionEventArgs(ClientSocketConnection connection)
			{
				Connection = connection;
			}
		}

		public delegate void ListeningHasStartedHandler(SocketListenerHelper sender, int port);

		public event ListeningHasStartedHandler ListeningHasStarted;

		private System.Net.IPAddress _localaddr = System.Net.IPAddress.Any;
		private int _port;
		private bool _findFirstFreePort = true;
		private TcpListener _listener = null;
		private Thread _threadListening = null;
//		private Thread _menuClientThread;
		private Menu _menu = null;
//		private MenuClientHandlerThreaded _menuClientHandlerThreaded;
		private event EventHandler IncommingConnection;
		public event MenuSession.MenuSessionEventHandler SessionStarting;
		private bool _finishedListening = false;

		protected SocketListenerHelper(Menu menu)
		{
			_menu = menu;
		}

		public SocketListenerHelper(int port, Menu menu) : this(menu)
		{
			_port = port;
		}

		public SocketListenerHelper(int port, Menu menu, bool findFirstFreePort) : this(port, menu)
		{
			_findFirstFreePort = findFirstFreePort;
		}

		public SocketListenerHelper(System.Net.IPAddress localaddr, int port, Menu menu) : this(port, menu)
		{
			_localaddr = localaddr;
		}

		public SocketListenerHelper(System.Net.IPAddress localaddr, int port, Menu menu, bool findFirstFreePort) : this(localaddr, port, menu)
		{
			_findFirstFreePort = findFirstFreePort;
		}

		public System.Net.IPAddress ListeningIPAddress
		{
			get { return _localaddr; }
		}

		public int Port
		{
			get { return _port; }
		}

		public bool FindFirstFreePort
		{
			get { return _findFirstFreePort; }
		}

		public void StartListening()
		{
			_threadListening = new Thread(new ThreadStart(ListeningThread));
			_threadListening.Start();
		}

		protected void ListeningThread()
		{
			int currentAttemptPort = _port;
			while (!_finishedListening)
			{
				try
				{
					if (_listener == null)
						_listener = new TcpListener(_localaddr, currentAttemptPort);

					try
					{
						_listener.Start();
					}
					catch (SocketException sex)
					{
						if (_findFirstFreePort)
						{
							// we failed to grab that port but we're supposed to keep going up
							// until we find one that works
							currentAttemptPort++;
							_listener = null;
							continue;
						}
						else
							throw sex;
					}

					if (ListeningHasStarted != null)
						ListeningHasStarted(this, currentAttemptPort);


					System.Diagnostics.Debug.WriteLine("SocketListenerHelper.ListeningThread wait on accept");
					
					// TODO - add an interrupt handler and interrupt in StopListening()
					TcpClient client = _listener.AcceptTcpClient();
					System.Console.WriteLine("ListeningThread connection");

					RaiseIncommingConnectionEvent(client);
				}
				catch (ThreadInterruptedException)
				{
					System.Diagnostics.Debug.WriteLine("SocketListenerHelper.ListeningThread loop interrupted - listening ending");
					_finishedListening = true;
				}
				catch (SocketException sex)
				{
					if (_finishedListening)
						System.Diagnostics.Debug.WriteLine("Socket Exception - but just part of shutdown, so no problem...");

					// TODO - something here to stop us tightlooping on some exception situations
					// like a max error count or something?
					System.Diagnostics.Debug.WriteLine("SocketListenerHelper.ListeningThread loop - socket exception " + sex.Message);
					System.Diagnostics.Debug.WriteLine(sex.StackTrace);
				}
			}
		}



		public void StopListening()
		{
			if (_threadListening == null)
				return;

			_finishedListening = true;
			_listener.Stop();
			_threadListening.Interrupt();
		}

		private void RaiseIncommingConnectionEvent(TcpClient client)
		{
			ClientSocketConnection connection = new ClientSocketConnection(client);
			if (IncommingConnection != null)
			{
				IncommingConnectionEventArgs args = new IncommingConnectionEventArgs(connection);
				IncommingConnection(this, args);
			}
			if (_menu != null)
			{
				//MenuSession session = new MenuSession(connection.OutWriter, connection.InReader, connection.RawStream, connection.RawStream);
				TelnetConnection telnetConnection = new TelnetConnection(client);
				telnetConnection.Negotiate();
				MenuSession session = new MenuSession(telnetConnection);
				session.Tag = connection;
				session.FlushEveryLine = true;				
				session.SessionStarting += new MenuSession.MenuSessionEventHandler(session_SessionStarting);
				session.SessionFinished += new MenuSession.MenuSessionEventHandler(session_SessionFinished);
				session.NewThreadShow(_menu, false);
			}
		}

		private void MenuClientThread()
		{
		}

		private bool session_SessionStarting(MenuSession session)
		{
			if (SessionStarting != null)
				return SessionStarting(session);

			return true;
		}

		private bool session_SessionFinished(MenuSession session)
		{
			ClientSocketConnection connection = session.Tag as ClientSocketConnection;
			if (connection == null)
				return false;

			connection.Close();

			return true;
		}
	}
}

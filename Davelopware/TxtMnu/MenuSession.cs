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
using System.IO;
using System.Threading;
using System.Collections;

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// This represents a particular user session which can show menus
	/// </summary>
	/// <remarks>
	/// A Menu instance represents an actual menu. The MenuSession instance represents
	/// a particular pair of input/output streams through which menus can be rendered
	/// and user choices read back.
	/// </remarks>
	public class MenuSession
	{
		/// <summary>
		/// As menus can be recurssively shown (nested), we need a way to keep track
		/// of the session run-time data associated with a Session and each of the
		/// hierarchy of menus in its current context.
		/// </summary>
		/// <remarks>
		/// The _stackSessionMenuData member variable contains a stack of instances
		/// of this type
		/// </remarks>
		private class SessionMenuData
		{
			public bool FinishedShowingMenu = false;
		}

		public static string NewLine = Environment.NewLine;

		public delegate bool MenuSessionEventHandler(MenuSession session);
		public delegate void ErrorOccuredHandler(Menu sender, MenuSession session, Exception ex, string msg);

		public event MenuSessionEventHandler SessionStarting;
		public event MenuSessionEventHandler SessionFinishing;
		public event MenuSessionEventHandler SessionFinished;
		public event ErrorOccuredHandler ErrorOccured;

		private TextWriter _out = null;
		private TextReader _in = null;
		private TelnetConnection _telnetConnection = null;
		//		private Stream _outRawStream = null;
		//		private Stream _inRawStream = null;
		private string _crlf = "\n";
		private bool _flushEveryLine = false;
		private Thread _menuThread;
		private object _menuThreadControlLock = new object();
		private Stack _stackSessionMenuData = new Stack();
		private IDictionary _sessionData = new Hashtable();
		private object _tag;

		public MenuSession(TextWriter outWriter, TextReader inReader)
		{
			_out = outWriter;
			_in = inReader;
		}

		public MenuSession(TelnetConnection telnetConnection)
		{
			_telnetConnection = telnetConnection;
			_out = TextWriter.Synchronized(new StreamWriter(_telnetConnection));
			_in = TextReader.Synchronized(new StreamReader(_telnetConnection));
		}

		public bool FlushEveryLine
		{
			get { return _flushEveryLine; }
			set { _flushEveryLine = value; }
		}

		public string CRLF
		{
			get { return _crlf; }
			set { _crlf = value; }
		}

//		public TextReader In
//		{
//			get { return _in; }
//			set { _in = value; }
//		}
//
//		public TextWriter Out
//		{
//			get { return _out; }
//			set { _out = value; }
//		}
//
		public object Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		public bool TelnetMode
		{
			get { return (_telnetConnection != null); }
		}

		public void Write(object value)
		{
			if (_out == null)
				return;

			try
			{
				_out.Write(value);
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
		}
		public void Write(char c)
		{
			if (_out == null)
				return;

			try
			{
				_out.Write(c);
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
		}

		public void Write(string value)
		{
			if (_out == null)
				return;

			try
			{
				_out.Write(value);
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
		}

		public void WriteLine(string value)
		{
			if (_out == null)
				return;

			try
			{
				_out.WriteLine(value);
				if (_flushEveryLine)
					_out.Flush();
				System.Diagnostics.Debug.WriteLine("Wrote to socket: " + value);
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
		}


		public void WriteLine(char[] buffer, int index, int count)
		{
			if (_out == null)
				return;

			try
			{
				_out.WriteLine(buffer, index, count);
				if (_flushEveryLine)
					_out.Flush();
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
		}

		public void WriteLine(object value)
		{
			if (_out == null)
				return;

			try
			{
				_out.WriteLine(value);
				if (_flushEveryLine)
					_out.Flush();
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
		}

		public void Flush()
		{
			if (_out == null)
				return;

			try
			{
				_out.Flush();
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
		}

		public string ReadLine()
		{
			try
			{
				string result = _in.ReadLine();
				System.Diagnostics.Debug.WriteLine("Read from stream: " + result);
				return result;
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
			return string.Empty;
		}

		public string ReadPassword()
		{
			try
			{
				bool originalEcho = false;
				if (_telnetConnection != null)
				{
					originalEcho = _telnetConnection.EchoBackToRemote;
					_telnetConnection.EchoBackToRemote = false;
				}
				//				else
				//					VT100Cmd_HideYourInput();
				string result = _in.ReadLine();
				if (_telnetConnection != null)
					_telnetConnection.EchoBackToRemote = originalEcho;
				//				else
				//					VT100Cmd_NoEcho();
				System.Diagnostics.Debug.WriteLine("Read from socket: " + result);
				return result;
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				// probably means that whatever we're writing to is 'closed'
				CloseSession();
			}
			return string.Empty;
		}

		public void WriteVT100Cmd(string command)
		{
			if (TelnetMode)
			{
				try
				{
					_telnetConnection.WriteVT100Command(command);
				}
				catch (Exception ex)
				{
					RaiseError(ex);
				}
			}
		}

		public void NewThreadShow(Menu menu, bool useThreadPool)
		{
			try
			{
				MenuSessionThreadStarter starter = new MenuSessionThreadStarter(this, menu, useThreadPool);
			}
			catch (Exception ex)
			{
				RaiseError(ex);
			}
		}

		/// <summary>
		/// Renders a particular menu to this session
		/// </summary>
		/// <param name="menu">The menu to render</param>
		/// <remarks>
		/// This is a blocking call. It will not return until CloseMenu() is called,
		/// either from another thread, or by the user of the session via menu options
		/// in the specified menu. (see MenuBuildHelper.UseSimpleMenuClose(...)
		/// </remarks>
		public void Show(Menu menu)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("MenuSession.Show called");

				if (_out == null)
					return;

				if (SessionStarting != null && _stackSessionMenuData.Count == 0)
				{
					if (SessionStarting(this) == false)
						return;
				}

				if (menu == null)
					return;

				SessionMenuData sessionMenuData = new SessionMenuData();
				sessionMenuData.FinishedShowingMenu = false;
				_stackSessionMenuData.Push(sessionMenuData);
				try
				{
					lock (_menuThreadControlLock)
					{
						if (_menuThread == null)
							_menuThread = Thread.CurrentThread;
						else if (_menuThread != Thread.CurrentThread)
							throw new Exception("You can't make multiple calls to the same MenuSession.Show() from different threads!");
					}

					do 
					{
						try
						{
							menu.WriteMenu(this);
							menu.ProcessInput(this);
						}
						catch (ThreadInterruptedException tie)
						{
							// this is ok
							System.Diagnostics.Debug.WriteLine(tie.Message);
							System.Diagnostics.Debug.WriteLine("which is fine");
						}
					}
					while (!sessionMenuData.FinishedShowingMenu);
				}
				catch (Exception ex)
				{
					RaiseError(ex);
				}
				finally
				{
					_stackSessionMenuData.Pop();
				}
			}
			catch (Exception ex)
			{
				RaiseError(ex);
			}
		}

		/// <summary>
		/// Closes the menu that is current in this Session.
		/// </summary>
		public void CloseMenu()
		{
			lock (_menuThreadControlLock)
			{
				try
				{
					if (_menuThread == null)
						return;

					if (_stackSessionMenuData.Count > 0)
					{
						SessionMenuData sessionMenuData = _stackSessionMenuData.Peek() as SessionMenuData;
						if (sessionMenuData != null)
							sessionMenuData.FinishedShowingMenu = true;
					}

					if (_menuThread != Thread.CurrentThread)
						_menuThread.Interrupt();
				}
				catch (Exception ex)
				{
					RaiseError(ex);
				}
			}
		}

		public void SetSessionData(string key, object data)
		{
			_sessionData[key] = data;
		}

		public object GetSessionData(string key)
		{
			return _sessionData[key];
		}

		public string GetSessionDataString(string key)
		{
			if (!_sessionData.Contains(key))
				return null;

			object data = _sessionData[key];
			if (data == null)
				return null;

			return data.ToString();
		}

		private bool _insideCloseSession = false;
		public void CloseSession()
		{
			if (_insideCloseSession)
				return;

			try
			{
				_insideCloseSession = true;
				CloseMenu();
				FireSessionFinishing();
				CleanUp();
				FireSessionFinished();
			}
			catch (Exception ex)
			{
				RaiseError(ex);
			}
			finally
			{
				_insideCloseSession = false;
			}
		}

		private string SubstituteCRLF(string value)
		{
			if (_crlf == NewLine)
				return value;
		
			if (value == null)
				return string.Empty;

			return value.Replace(NewLine, _crlf);
		}

		private void CleanUp()
		{
			// TODO maybe output something to the client to tell them we're forced down

			try
			{
				if (_in != null)
					try { _in.Close(); }
					catch {}
				if (_out != null)
					try { _out.Flush(); }
					catch {}
				if (_out != null)
					try { _out.Close(); }
					catch {}
				if (TelnetMode)
				{
					try { (_telnetConnection).Close(); }
					catch {}
				}
			}
			catch (Exception ex)
			{
				RaiseError(ex);
			}
		}

		private void FireSessionFinishing()
		{
			try
			{
				if (SessionFinishing != null)
					SessionFinishing(this);
			}
			catch (Exception ex)
			{
				RaiseError(ex);
			}
		}

		private void FireSessionFinished()
		{
			try
			{
				if (SessionFinished != null)
					SessionFinished(this);
			}
			catch (Exception ex)
			{
				RaiseError(ex);
			}
		}

	
		public void RaiseError(Exception ex)
		{
			RaiseError(null, ex, ex.Message);
		}

		public void RaiseError(Menu menu, Exception ex)
		{
			RaiseError(menu, ex, ex.Message);
		}

		public void RaiseError(string msg)
		{
			RaiseError(null, null, msg);
		}

		public void RaiseError(Menu menu, string msg)
		{
			RaiseError(menu, null, msg);
		}

		public void RaiseError(Menu menu, Exception ex, string msg)
		{
			if (ErrorOccured != null)
				ErrorOccured(menu, this, ex, msg);
		}
	}
}

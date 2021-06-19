/*
 * Copyright 2007 Davelopware Ltd
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

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// A class that implements text menus
	/// </summary>
	public class Menu
	{
		private const string PERCENTAGE_SIGN_INTERIM_REPLACEMENT_STRING = "PeRcEnTaGeSiGn";

		public delegate void ErrorOccuredHandler(Menu sender, MenuSession session, Exception ex, string msg);

		public event ErrorOccuredHandler ErrorOccured;

		private MenuEntries _entries = new MenuEntries();
		// TODO - maybe some of the dynamic (runtime) menuentry data should be associated
		// with the session rather than the entry - such as whether the entry is visible!?!

		private string _header;
		private string _footer;
		private string _sepKeyName;
		private string _sepEntry;
		private string _help;
		private string _helpKey;

		public event Davelopware.TxtMnu.MenuBeforeShowingHandler MenuBeforeShowing;

		#region constructors

		public Menu()
		{
//			_out = Console.Out;
//			_in = Console.In;
			_helpKey = "?";
		}

		public Menu(string header, string entrySeperator, string keyNameSeperator, string footer)
			: this()
		{
			_header = header;
			_sepEntry = entrySeperator;
			_sepKeyName = keyNameSeperator;
			_footer = footer;
		}

//		public Menu(TextWriter txtOut, TextReader txtIn)
//		{
//			_out = txtOut;
//			_in = txtIn;
//			_helpKey = "?";
//		}

//		public Menu(/*TextWriter txtOut, TextReader txtIn,*/ string header, string entrySeperator, string keyNameSeperator, string footer)
////			: this(txtOut, txtIn)
//		{
//			_header = header;
//			_sepEntry = entrySeperator;
//			_sepKeyName = keyNameSeperator;
//			_footer = footer;
//		}

		#endregion

		#region member accessors

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
		public MenuEntries Entries
		{
			get { return _entries; }
		}

		public string Header
		{
			get { return _header; }
			set { _header = value; }
		}

		public string Footer
		{
			get { return _footer; }
			set { _footer = value; }
		}

		public string KeyNameSeperator
		{
			get { return _sepKeyName; }
			set { _sepKeyName = value; }
		}

		public string EntrySeperator
		{
			get { return _sepEntry; }
			set { _sepEntry = value; }
		}

		public string Help
		{
			get { return _help; }
			set { _help = value; }
		}

		public string HelpKey
		{
			get { return _helpKey; }
			set { _helpKey = value; }
		}

		#endregion

		#region methods related to input/output

		public void WriteMenu(MenuSession session)
		{
			try
			{
				if (MenuBeforeShowing != null)
					MenuBeforeShowing(session, this);

				session.Write(_header);
				int iCount = 1;
				foreach (IMenuEntry entry in _entries)
				{
					if (entry.Visible && entry.GetSessionVisible(session))
					{
						if (iCount > 1)
							session.Write(_sepEntry);

						session.Write(entry.Key);
						session.Write(_sepKeyName);
						session.Write(entry.GetName(session, this));
						iCount++;
					}
				}
				session.Write(_footer);
				session.Flush();
			}
			catch (Exception ex)
			{
				RaiseError(session, ex);
			}
		}

		public void ProcessInput(MenuSession session)
		{
			ProcessInput(session, ReadUserInput(session, ">"));
		}

		private string ReadUserInput(MenuSession session, string prompt)
		{
			try
			{
				session.Write(prompt);
				return session.ReadLine();
			}
			catch (Exception ex)
			{
				RaiseError(session, ex);
				return string.Empty;
			}
		}

		private void ProcessInput(MenuSession session, string input)
		{
			try
			{
				if (input.StartsWith(_helpKey))
				{
					// help requested
					if (input.Trim().CompareTo(_helpKey) == 0)
					{
						// general help request - show help for the menu
						string help = _help;
						help = help.Replace("%%",PERCENTAGE_SIGN_INTERIM_REPLACEMENT_STRING);
						help = help.Replace(PERCENTAGE_SIGN_INTERIM_REPLACEMENT_STRING,"%");
						session.WriteLine(help);
					}
					else
					{
						// could be asking for help on a menu entry...
						string key = input.Substring(_helpKey.Length).Trim();
						foreach (IMenuEntry entry in _entries)
						{
							if (entry.KeyCompare(key))
							{
								string help;
								MenuEntrySubMenu menuEntrySubMenu = entry as MenuEntrySubMenu;
								if (entry.Help == string.Empty && menuEntrySubMenu != null && menuEntrySubMenu.SubMenu != null)
									help = menuEntrySubMenu.SubMenu.Help;
								else
									help = entry.Help;
								help = help.Replace("%%",PERCENTAGE_SIGN_INTERIM_REPLACEMENT_STRING);
								help = help.Replace("%k",entry.Key);
								help = help.Replace("%s",_sepKeyName);
								help = help.Replace("%n",entry.GetName(session, this));
								help = help.Replace(PERCENTAGE_SIGN_INTERIM_REPLACEMENT_STRING,"%");
								session.WriteLine(help);
								break;
							}
						}
					}
				}
				else
				{
					// compare input to keys of menu entries
					foreach (IMenuEntry entry in _entries)
					{
						if (entry.KeyCompare(input))
						{
							entry.FireMenuEntrySelected(session, this);
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				RaiseError(session, ex);
			}
		}

		#endregion

		private bool bollox
		{
			get { return false; }
		}

		public void RaiseError(MenuSession session, Exception ex)
		{
			RaiseError(session, ex, ex.Message);
		}

		public void RaiseError(MenuSession session, string msg)
		{
			RaiseError(session, null, msg);
		}

		public void RaiseError(MenuSession session, Exception ex, string msg)
		{
			if (ErrorOccured != null)
				ErrorOccured(this, session, ex, msg);
		}
	}
}

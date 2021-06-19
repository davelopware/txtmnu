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

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// The simplest implementation of IMenuEntry
	/// </summary>
	public class MenuEntryDynamic : IMenuEntry
	{
		private const string SESSION_KEY_SESSION_VISIBLE = "SessionVisible";

		private string _key = string.Empty;
		private string _name = string.Empty;
		private string _help = string.Empty;
		private bool _visible = true;

		public delegate string GetDynamicMenuEntryHandler(MenuSession session, Menu menu, MenuEntryDynamic menuEntry);

		public event GetDynamicMenuEntryHandler GetNameEvent;
		public event GetDynamicMenuEntryHandler GetHelpEvent;

		#region constructors

		public MenuEntryDynamic()
		{
		}

		public MenuEntryDynamic(string key)
		{
			_key = key;
		}

		#endregion

		#region IMenuEntry Members

		public event Davelopware.TxtMnu.MenuEntrySelectedHandler MenuEntrySelected;

		public virtual string Key
		{
			get { return _key; }
			set { _key = value; }
		}

		public virtual string GetName(MenuSession session, Menu menu)
		{
			if (GetNameEvent != null)
				return GetNameEvent(session, menu, this);
			else
				return _name;
		}
		/// <summary>
		/// Gets the dynamic name if possible, otherwise the default, and sets the default
		/// </summary>
		[Obsolete("string Name is deprecated, please use GetName() instead.")]
		public virtual string Name
		{
			set { _name = value; }
		}

		/// <summary>
		/// Gets the dynamic help if possible, otherwise the default, and sets the default
		/// </summary>
		public virtual string Help
		{
			get
			{
				if (GetHelpEvent != null)
					return GetHelpEvent(null, null, this); // TODO - plug this up the same as GetName
				else
					return _help;
			}
			set { _help = value; }
		}

		public virtual bool Visible
		{
			get { return _visible; }
			set { _visible = value; }
		}

		public bool GetSessionVisible(MenuSession session)
		{
			object val = session.GetSessionData(SessionKey(SESSION_KEY_SESSION_VISIBLE));
			if (val == null)
				return true; // default is that the entry is visible

			return ((bool)val == true);
		}

		public void SetSessionVisible(MenuSession session, bool visible)
		{
			session.SetSessionData(SessionKey(SESSION_KEY_SESSION_VISIBLE), visible);
		}

		public virtual bool KeyCompare(string input)
		{
			return (_key.CompareTo(input) == 0);
		}

		public virtual void FireMenuEntrySelected(MenuSession session, Menu menu)
		{
			if (MenuEntrySelected != null)
				MenuEntrySelected(this, session, menu);
		}

		#endregion

		private string SessionKey(string innerKey)
		{
			return this.GetHashCode() + "med" + innerKey;
		}
	}
}

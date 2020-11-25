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
using System.Collections;
using System.Reflection;

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// </summary>
	public class MenuEntryReflector : MenuEntryDynamic
	{
		private object _obj;
		private Menu _reflectedMenu;
		private bool _reflectedMenuBuilt = false; // track lazy initialisation of _reflectedMenu
		private Hashtable _reflectedMenuEntries = new Hashtable();
		private string _fixedName = string.Empty; // todo - consider having a property for this
		private BindingFlags _bindFlagsForGettingProperties = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

		public MenuEntryReflector(string key, object objToReflect, string header, string entrySeperator, string keyNameSeperator, string footer) : base(key)
		{
			_fixedName = header;
			this.GetNameEvent += new GetDynamicMenuEntryHandler(MenuEntryReflector_GetNameEvent);
			this.GetHelpEvent += new GetDynamicMenuEntryHandler(MenuEntryReflector_GetHelpEvent);
			this.MenuEntrySelected += new MenuEntrySelectedHandler(MenuEntryReflector_MenuEntrySelected);
			ObjectToReflect = objToReflect;

			_reflectedMenu = new Menu(header, entrySeperator, keyNameSeperator, footer);
		}

		public MenuEntryReflector(string key, object objToReflect, BindingFlags bindFlagsForGettingProperties, string header, string entrySeperator, string keyNameSeperator, string footer) : this(key, objToReflect, header, entrySeperator, keyNameSeperator, footer)
		{
			_fixedName = header;
			_bindFlagsForGettingProperties = bindFlagsForGettingProperties;
			this.GetNameEvent += new GetDynamicMenuEntryHandler(MenuEntryReflector_GetNameEvent);
			this.GetHelpEvent += new GetDynamicMenuEntryHandler(MenuEntryReflector_GetHelpEvent);
			this.MenuEntrySelected += new MenuEntrySelectedHandler(MenuEntryReflector_MenuEntrySelected);
			ObjectToReflect = objToReflect;

			_reflectedMenu = new Menu(header, entrySeperator, keyNameSeperator, footer);
		}

		public object ObjectToReflect
		{
			get { return _obj; }
			set
			{
				_obj = value;
			}
		}

		protected void BuildReflectedMenuEntries()
		{
			if (_reflectedMenuBuilt) // lazy initialisation of _reflectedMenu so only run first time
				return;

			Type type = _obj.GetType();
			int i = 1;
			foreach (PropertyInfo prop in type.GetProperties(_bindFlagsForGettingProperties))
			{
				MenuEntryDynamic meDyn = new MenuEntryDynamic(i.ToString());
				meDyn.GetNameEvent += new GetDynamicMenuEntryHandler(meDyn_GetNameEvent);
				meDyn.MenuEntrySelected += new MenuEntrySelectedHandler(meDyn_MenuEntrySelected);
				_reflectedMenuEntries.Add(meDyn, prop);
				_reflectedMenu.Entries.Add(meDyn);
				i++;
			}
			MenuBuildHelper.UseSimpleMenuClose(_reflectedMenu, "x", "Close");
			_reflectedMenuBuilt = true;
		}

		private void MenuEntryReflector_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			// show the reflected menu
			BuildReflectedMenuEntries();
			session.Show(_reflectedMenu);
		}

		private string MenuEntryReflector_GetNameEvent(MenuSession session, Menu menu, MenuEntryDynamic menuEntry)
		{
			return _fixedName;
		}

		private string MenuEntryReflector_GetHelpEvent(MenuSession session, Menu menu, MenuEntryDynamic menuEntry)
		{
			return "Live object of type; " + _obj.GetType().FullName;
		}

		private string meDyn_GetNameEvent(MenuSession session, Menu menu, MenuEntryDynamic menuEntry)
		{
			string result = "{unable to access value}";
			PropertyInfo prop = _reflectedMenuEntries[menuEntry] as PropertyInfo;
			int indent = 0;
			if (prop != null)
			{
				result = prop.Name + "={" + ReflectedObjectRenderer.RenderObjectProperty(session, prop, _obj) + "}";
			}
			result = result.Replace("\r\n", "TMP_RN_PLACE_HOLDER");
			result = result.Replace("\n", "\\n}\n" + "".PadRight(indent, ' ') + "{");
			result = result.Replace("TMP_RN_PLACE_HOLDER", "\\r\\n}\r\n        {");

			return result;
		}

		private void meDyn_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			if (!_reflectedMenuEntries.ContainsKey(entry))
				return;

			PropertyInfo prop = _reflectedMenuEntries[entry] as PropertyInfo;
			if (prop == null)
				return;
			object value = prop.GetValue(_obj, null);

			session.WriteLine("You selected " + prop.Name + "=" + value.ToString());

			if (value.GetType().IsValueType)
			{
				session.WriteLine("Unable to browse into a 'value type'");
			}
			else
			{
				MenuEntryReflector meObjTmp = new MenuEntryReflector("_", value, menu.Header, menu.EntrySeperator, ".", menu.Footer);
				meObjTmp.FireMenuEntrySelected(session, null);
			}

		}
	}
}

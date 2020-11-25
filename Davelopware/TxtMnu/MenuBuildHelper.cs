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
	public class MenuBuildHelper
	{
		public static Menu MakeAndLinkSubMenu(Menu menu, string key, string name, out MenuEntrySubMenu menuEntrySubMenu)
		{
			Menu subMenu = new Menu();
			subMenu.Footer = menu.Footer;
			subMenu.Header = menu.Header;
//			subMenu._in = _in;
//			subMenu._out = _out;
			subMenu.EntrySeperator = menu.EntrySeperator;
			subMenu.KeyNameSeperator = menu.KeyNameSeperator;
			menuEntrySubMenu = new MenuEntrySubMenu(key, name, subMenu);
			menu.Entries.Add(menuEntrySubMenu);
			return subMenu;
		}

		public static IMenuEntry UseSimpleMenuClose(Menu menu, string key, string name)
		{
			MenuEntrySimple menuEntryClose = new MenuEntrySimple(key, name);
			menu.Entries.Add(menuEntryClose);
			menuEntryClose.MenuEntrySelected += new MenuEntrySelectedHandler(menuEntryClose_MenuEntrySelected);
			menuEntryClose.Help = "(%k%s%n) Close this menu";
			return menuEntryClose;
		}

		private static void menuEntryClose_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.CloseMenu();
		}
	}
}

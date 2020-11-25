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
	/// An extension of MenuEntrySimple that Menu uses to implement Sub Menus
	/// </summary>
	public class MenuEntrySubMenu : MenuEntrySimple
	{
		private Menu _subMenu;

		#region constructors

		public MenuEntrySubMenu()
		{
			this.MenuEntrySelected += new MenuEntrySelectedHandler(MenuEntrySubMenu_MenuEntrySelected);
		}

		public MenuEntrySubMenu(string key, string name, Menu subMenu) : base(key, name)
		{
			this.MenuEntrySelected += new MenuEntrySelectedHandler(MenuEntrySubMenu_MenuEntrySelected);
			_subMenu = subMenu;
		}

		#endregion

		public Menu SubMenu
		{
			get { return _subMenu; }
			set { _subMenu = value; }
		}

		private static void MenuEntrySubMenu_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			MenuEntrySubMenu menuEntrySubMenu = entry as MenuEntrySubMenu;
			if (menuEntrySubMenu != null)
			{
				if (menuEntrySubMenu.SubMenu != null)
				{
					// TOTO maybe make this session.Show(menu) ???
					// but need to resolve that with 
//					menuEntrySubMenu.SubMenu.Show(session);
					session.Show(menuEntrySubMenu.SubMenu);
				}
			}
		}

		private void menuEntrySubMenu_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
		}

	}
}

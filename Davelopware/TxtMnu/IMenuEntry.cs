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
	/// Summary description for IMenuEntry.
	/// </summary>
	public interface IMenuEntry
	{
		event MenuEntrySelectedHandler MenuEntrySelected;
		string Key { get; set; }
		string GetName(MenuSession session, Menu menu);

		[Obsolete("string Name is deprecated, please use GetName() instead.")]

		string Name { set; }
		string Help { get; set; }
		bool Visible { get; set; }
		bool GetSessionVisible(MenuSession session);
		void SetSessionVisible(MenuSession session, bool visible);
		bool KeyCompare(string input);
		void FireMenuEntrySelected(MenuSession session, Menu menu);
	}
}

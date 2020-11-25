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
	/// An item of text for display in the context of TxtMnu's
	/// </summary>
	public class Text
	{
		private string _txt;
//		private TextHAlign _halign = TextHAlign.Left;
//		private int _paddingTop = 0;
//		private int _paddingBottom = 0;
//		private int _paddingLeft = 0;
//		private int _paddingRight = 0;

		public Text()
		{
		}

		public Text(string txt)
		{
			_txt = txt;
		}

	}
}

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
	/// Summary description for MenuEntryFileShow.
	/// </summary>
	public class MenuEntryFileShow : MenuEntrySimple
	{
		private const int READ_BUFFER_SIZE = 1024;

		protected string _fullFileName;

		public MenuEntryFileShow(string key, string name, string fullFileName) : base(key, name)
		{
			_fullFileName = fullFileName;
			this.MenuEntrySelected += new MenuEntrySelectedHandler(MenuEntryFileSystem_MenuEntrySelected);
		}

		public string FullFileName
		{
			get { return _fullFileName; }
			set { _fullFileName = value; }
		}

		private void MenuEntryFileSystem_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("========================================");
			session.WriteLine("Begin display the file: " + _fullFileName);
			session.WriteLine("========================================");
			int totalBytesRead = 0;

			try
			{
				StreamReader fileStream = File.OpenText(_fullFileName);
				int bytesRead = 0;
				char[] readBuffer = new char[READ_BUFFER_SIZE];
				do
				{
					bytesRead = fileStream.Read(readBuffer, 0, READ_BUFFER_SIZE);
					totalBytesRead += bytesRead;
					session.WriteLine(readBuffer, 0, bytesRead);
				} while (bytesRead > 0);
			
			}
			catch (Exception ex)
			{
				session.WriteLine("========================================");
				session.WriteLine("*** an exception occured ***");
				session.WriteLine("Message:" + ex.Message );
				session.WriteLine("StackTrace:" + ex.StackTrace);
			}
			session.WriteLine("");
			session.WriteLine("========================================");
			session.WriteLine("Done display the file: " + _fullFileName);
			session.WriteLine(" total bytes = " + totalBytesRead.ToString());
			session.WriteLine("========================================");
		}
	}
}

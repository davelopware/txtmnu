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
	/// Summary description for MenuEntryFileSystem.
	/// </summary>
	public class MenuEntryFileSystem : MenuEntrySimple
	{
		protected string _rootDirectory;
		protected string _filter;

		public MenuEntryFileSystem(string key, string name, string rootDirectory) : base(key, name)
		{
			_rootDirectory = rootDirectory;
			_filter = string.Empty;
			this.MenuEntrySelected += new MenuEntrySelectedHandler(MenuEntryFileSystem_MenuEntrySelected);
		}

		public MenuEntryFileSystem(string key, string name, string rootDirectory, string filter) : this(key, name, rootDirectory)
		{
			_filter = filter;
		}

		public string RootDirectory
		{
			get { return _rootDirectory; }
			set { _rootDirectory = value; }
		}

		private void MenuEntryFileSystem_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			// TODO - probably want to handle files to - just doing directories now to prove the point
			Menu mnuDirectoryListing = new Menu(_rootDirectory + session.CRLF, menu.EntrySeperator, ".", menu.Footer);
			int menuEntryNumber = 1;

			string[] subdirectories;
			if (_filter == string.Empty)
				subdirectories = Directory.GetDirectories(_rootDirectory);
			else
				subdirectories = Directory.GetDirectories(_rootDirectory, _filter);
			if (subdirectories.Length > 0)
			{
				foreach (string subdirectory in subdirectories)
				{
					MenuEntryFileSystem meFileSystem;
					if (_filter == string.Empty)
						meFileSystem = new MenuEntryFileSystem(menuEntryNumber.ToString(), subdirectory, Path.Combine(_rootDirectory, subdirectory));
					else
						meFileSystem = new MenuEntryFileSystem(menuEntryNumber.ToString(), subdirectory, Path.Combine(_rootDirectory, subdirectory), _filter);
					mnuDirectoryListing.Entries.Add(meFileSystem);
					menuEntryNumber++;
				}
			}
			string[] containedFiles;
			if (_filter == string.Empty)
				containedFiles = Directory.GetFiles(_rootDirectory);
			else
				containedFiles = Directory.GetFiles(_rootDirectory, _filter);
			if (containedFiles.Length > 0)
			{
				foreach (string containedFile in containedFiles)
				{
					MenuEntryFileShow meFileShow = new MenuEntryFileShow(menuEntryNumber.ToString(), containedFile, Path.Combine(_rootDirectory, containedFile));
					mnuDirectoryListing.Entries.Add(meFileShow);
					menuEntryNumber++;
				}
			}

			if (menuEntryNumber == 1)
			{
				// no subdirectories or contained files!
				mnuDirectoryListing.Entries.Add(new MenuEntrySimple("", "This directory contains no files or subdirectories"));
			}

			MenuBuildHelper.UseSimpleMenuClose(mnuDirectoryListing, "x", "Close");
			session.Show(mnuDirectoryListing);
		}
	}
}

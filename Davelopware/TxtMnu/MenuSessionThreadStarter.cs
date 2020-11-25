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
using System.Threading;

namespace Davelopware.TxtMnu
{
	class MenuSessionThreadStarter
	{
		protected MenuSession _session;
		protected Menu _menu;
		
		public MenuSessionThreadStarter(MenuSession session, Menu menu, bool useThreadPool)
		{
			Session = session;
			Menu = menu;
			if (useThreadPool)
			{
				ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolWaitCallback));
			}
			else
			{
				Thread thread = new Thread(new ThreadStart(Start));
				thread.Start();
			}
		}

		public MenuSession Session
		{
			get { return _session; }
			set { _session = value; }
		}

		public Menu Menu
		{
			get { return _menu; }
			set { _menu = value; }
		}

		public void Start()
		{
			Session.Show(Menu);
			Session.CloseSession();
		}

		public void ThreadPoolWaitCallback(Object stateInfo)
		{
			Session.Show(Menu);
			Session.CloseSession();
		}
	}

}

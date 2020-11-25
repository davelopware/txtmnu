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
using System.Threading;
using System.Reflection;

using Davelopware.TxtMnu;

namespace MenuDevExperiment
{
	public class MainClass
	{
		static readonly string CRLF = MenuSession.NewLine;

		static SocketListenerHelper helper;

		public static void Main(string[] argv)
		{

			Menu mnu = new Menu(CRLF + "Simple Menu" + CRLF + " ", CRLF + " ", "=", CRLF);
			mnu.Help = "This is a simple menu to demonstrate how the menuing works";

			IMenuEntry meSayHello = mnu.Entries.Add(new MenuEntrySimple("s", "Say Hello"));
			meSayHello.MenuEntrySelected += new MenuEntrySelectedHandler(meSayHello_MenuEntrySelected);
			meSayHello.Help = "(%k%s%n) Says hello to the user";
			
			IMenuEntry meSayGoodnight = mnu.Entries.Add(new MenuEntrySimple("n", "Night Night"));
			meSayGoodnight.MenuEntrySelected += new MenuEntrySelectedHandler(meSayGoodnight_MenuEntrySelected);
			meSayGoodnight.Help = "(%k%s%n) wishes the user a good night";

			MenuEntrySubMenu meSubBoo;
			Menu mnuSubBoo = MenuBuildHelper.MakeAndLinkSubMenu(mnu, "b", "Boo sub menu", out meSubBoo);
			mnuSubBoo.KeyNameSeperator = ". ";
			mnuSubBoo.Help = "This sub menu has various interesting possibilities";

			IMenuEntry meBoo1 = mnuSubBoo.Entries.Add(new MenuEntrySimple("1", "First Boo"));
			meBoo1.MenuEntrySelected += new MenuEntrySelectedHandler(meBoo1_MenuEntrySelected);
			meBoo1.Help = "(%k%s%n) the first sort of boo that can be done";

			IMenuEntry meBoo2 = mnuSubBoo.Entries.Add(new MenuEntrySimple("2", "Second Boo"));
			meBoo2.MenuEntrySelected += new MenuEntrySelectedHandler(meBoo2_MenuEntrySelected);
			meBoo2.Help = "(%k%s%n) the second sort of boo that can be done (some say it's better)";

			IMenuEntry meVT100Stuff = mnuSubBoo.Entries.Add(new MenuEntrySimple("3", "VT100 Stuff"));
			meVT100Stuff.MenuEntrySelected += new MenuEntrySelectedHandler(meVT100Stuff_MenuEntrySelected);
			meVT100Stuff.Help = "(%k%s%n) VT100 test stuff";

			MenuEntryReflector meObj = new MenuEntryReflector("M", mnu, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static, CRLF + "Menu Object Menu" + CRLF + " ", CRLF + " ", ".", CRLF);
			mnu.Entries.Add(meObj);

			MenuEntryFileSystem meFileSys = new MenuEntryFileSystem("f", "Explore C drive", @"c:\");
			mnu.Entries.Add(meFileSys);

			MenuBuildHelper.UseSimpleMenuClose(mnuSubBoo, "x", "Close");

			MenuEntrySubMenu meSubVT100;
			Menu mnuSubVT100 = MenuBuildHelper.MakeAndLinkSubMenu(mnu, "v", "VT100 Control Test", out meSubVT100);
			mnuSubVT100.KeyNameSeperator = ". ";

			meSubVT100.Help = "This sub menu has VT100 Control sequence test options";
	
			IMenuEntry meVT100Simple = mnuSubVT100.Entries.Add(new MenuEntrySimple("1", "Simple Test"));
			meVT100Simple.MenuEntrySelected += new MenuEntrySelectedHandler(meVT100Simple_MenuEntrySelected);

			IMenuEntry meVT100SetCode = mnuSubVT100.Entries.Add(new MenuEntrySimple("2", "Set Code"));
			meVT100SetCode.MenuEntrySelected += new MenuEntrySelectedHandler(meVT100SetCode_MenuEntrySelected);

			IMenuEntry meVT100TestCode = mnuSubVT100.Entries.Add(new MenuEntrySimple("3", "Test Code"));
			meVT100TestCode.MenuEntrySelected += new MenuEntrySelectedHandler(meVT100TestCode_MenuEntrySelected);


			
			MenuBuildHelper.UseSimpleMenuClose(mnu, "x", "Close");

			helper = new SocketListenerHelper(8889, mnu);
			helper.ListeningHasStarted += new Davelopware.TxtMnu.SocketListenerHelper.ListeningHasStartedHandler(helper_ListeningHasStarted);
			helper.StartListening();

			MenuSession session = new MenuSession(Console.Out, Console.In);
			session.Show(mnu);
			helper.StopListening();
		}

		private static void meSayHello_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("HELLO - is that ok then - eh - pushy!");
		}

		private static void meSayGoodnight_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("sweet dreams or suming, oh and the port is:" + helper.Port);
			session.WriteLine("now enter a password:");
			string pwd = session.ReadPassword();
			session.WriteLine("thank you. the password you entered was:" + pwd);
		}

		private static void meQuit_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
		}

		private static void meBoo1_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("BOO WHOOOO");
		}

		private static void meBoo2_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("wibble wibble nuts turd breath BOOOOOO");
			menu.Entries["1"].Visible = !menu.Entries["1"].Visible;
		}

		private static bool toggle = false;
		private static void meVT100Stuff_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("Next line is numbers");
			session.WriteLine("1234567890123456789012345678901234567890");
			session.WriteVT100Cmd("[1A");
			session.WriteVT100Cmd("[4C");
			session.WriteLine("XXX");
//			session.TelnetCmd("[0m");
//			session.Out.WriteLine("1234567890123456789012345678901234567890");

//			session.Out.WriteLine("pretoggle");
//			if (toggle)
//				session.TelnetCmd("[31m");
//			else
//				session.TelnetCmd("[0m");
//			toggle = !toggle;
//			session.Out.WriteLine("posttoggle");
			
		}

		private static void helper_ListeningHasStarted(SocketListenerHelper sender, int port)
		{
			Console.Out.WriteLine("Listening on port:" + port);
		}

		private static void meVT100Simple_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("Next line is numbers");
			session.WriteLine("1234567890123456789012345678901234567890");
			session.WriteVT100Cmd("[1A");
			session.WriteVT100Cmd("[4C");
			session.WriteLine("XXX");
		}

		private static string vt100TestCode;
		private static void meVT100SetCode_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			session.WriteLine("Enter the code to test:");
			vt100TestCode = session.ReadLine();
			VT100TestCode(session);
		}

		private static void VT100TestCode(MenuSession session)
		{
			session.WriteLine("VT100 Code Test:");
			session.WriteLine("Pre Test...");
			session.WriteVT100Cmd(vt100TestCode);
			session.WriteLine("Post Test");
		}

		private static void meVT100TestCode_MenuEntrySelected(IMenuEntry entry, MenuSession session, Menu menu)
		{
			VT100TestCode(session);
		}
	}
}

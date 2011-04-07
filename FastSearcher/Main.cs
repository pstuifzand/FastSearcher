using System;
using Gtk;

namespace FastSearcher
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			win.SetSizeRequest(650, 800);
			win.WindowPosition = WindowPosition.CenterAlways;
			Application.Run ();
		}
	}
}
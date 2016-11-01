using Gtk;
using System;
using System.Collections.Generic;

// The GameApp class sets up the various callbacks for animation and interaction
namespace Game
{
	public class GameApp : Window
	{
		// Set this to false to turn off the timer
		private bool timer = true;

		// All rendering happens in darea
		private DrawingArea darea;

		// All objects to be rendered have to be added to drawable through the AddDrawable() method
		public List<Drawable> drawable = new List<Drawable>();

		// This is the Actor that does the actual animation and interaction
		private Actor actor;

		public GameApp() : base("Game")
		{
			// Set up a suitably-sized window
			SetDefaultSize(800, 600);
			SetPosition(WindowPosition.Center);
			DeleteEvent += delegate { Application.Quit(); };

			// Set up a timer to call every 20 ms
			GLib.Timeout.Add(20, new GLib.TimeoutHandler(OnTimer));

			// Set up the DrawingArea so that it is enabled for keyboard interaction
			darea = new DrawingArea();
			darea.ExposeEvent += OnExpose;
			darea.KeyPressEvent += OnKeyPress;
			darea.KeyReleaseEvent += OnKeyRelease;
			darea.ButtonPressEvent += OnButton;
			darea.CanFocus = true;
			darea.FocusInEvent += OnFocus;
			darea.FocusOutEvent += OffFocus;
			Add(darea);

			actor = new Actor(this);

			ShowAll();
		}

		bool OnTimer()
		{
			if (!timer) return false;

			// Let the Actor do it's thing
			actor.Do();

			// Render the updates to the Drawables
			darea.QueueDraw();
			return true;
		}

		// Grab the focus when the window is clicked
		void OnButton(object sender, ButtonPressEventArgs args)
		{
			darea.GrabFocus();
		}

		// Give the focus to the DrawingArea when mouse enters
		void OnFocus(object sender, FocusInEventArgs args)
		{
			darea.HasFocus = true;
		}

		// Drop focus when mouse exits
		void OffFocus(object sender, FocusOutEventArgs args)
		{
			darea.HasFocus = false;
		}

		// Render the dirtied window
		void OnExpose(object sender, ExposeEventArgs args)
		{
			DrawingArea area = (DrawingArea)sender;
			Cairo.Context cr = Gdk.CairoHelper.Create(area.GdkWindow);

			// Clear background
			cr.SetSourceRGB(1.0, 1.0, 1.0);
			cr.Paint();

			// Set the coordinate system origin at bottom left
			cr.Translate(0, area.Allocation.Height);
			cr.Scale(1.0, -1.0);

			// Render all Drawables, resetting the coordinate transform for each
			foreach (Drawable d in drawable)
			{
				cr.Save();
				d.Draw(cr);
				cr.Restore();
			}

			cr.GetTarget().Dispose();
			((IDisposable)cr).Dispose();

		}

		// Pass on handling of key presses to the Actor
		void OnKeyPress(object sender, KeyPressEventArgs args)
		{
			actor.OnKeyPress(args.Event.Key);
		}
		void OnKeyRelease(object sender, KeyReleaseEventArgs args)
		{
			actor.OnKeyRelease(args.Event.Key);
		}
		// Add and remove Drawables from the rendering list
		public void AddDrawable(Drawable d)
		{
			drawable.Add(d);
		}

		public bool RemoveDrawable(Drawable d)
		{
			return (drawable.Remove(d));
		}


		public static void Main()
		{
			Application.Init();
			new GameApp();
			Application.Run();
		}
	}
}
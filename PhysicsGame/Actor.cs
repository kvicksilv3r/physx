using System;
using System.Collections.Generic;

// The Actor class is where Drawable objects are added to the game and their positions updated.
namespace Game
{
	public class Actor
	{
		// This is a reference to the game object
		private GameApp game;

		// Keeping track of the time is useful timed updates
		private DateTime time = DateTime.Now;
		// Add your data structures below this comment
		private double elevation = 0.0;

		private double force = 0.0;
		private double powerBarLength = 10;
		private double maxForce = 40;
		private double forceIncrease = 0.8;

		private double gravity = -1;

		private Rectangle powerBar;
		private Rectangle powerBarBg;

		private GameObject target1;

		private double powerBarPos = 40;

		Rectangle barrel;
		Circle body;
		Vector2 tank_Position = new Vector2()
		{
			x = 15,
			y = 15
		};

		List<GameObject> gameObjects = new List<GameObject>();
		List<Bullet> bullets = new List<Bullet>();

		int[] powerColor = new int[] { 1, 0, 0 };

		private List<Gdk.Key> keys = new List<Gdk.Key>();
		private List<Gdk.Key> lastKeys = new List<Gdk.Key>();

		// Create initial data objects here
		public Actor(GameApp game)
		{
			this.game = game;  // Do not remove this line!

			// Add your initialisations below this comment

			powerBar = new Rectangle(powerBarPos, game.DefaultHeight - 40, 0, 20, powerColor);
			powerBarBg = new Rectangle(-4 + powerBarPos + (maxForce * powerBarLength) / 2, game.DefaultHeight - 40, maxForce * powerBarLength, 26);

			target1 = new GameObject();
			target1.Box = new Rectangle(400, 400, 50, 50);
			gameObjects.Add(target1);

			body = new Circle(tank_Position.x, tank_Position.y, 25);
			barrel = new Rectangle(body.X, body.Y, 150, 10);
			game.AddDrawable(body);
			game.AddDrawable(barrel);
			game.AddDrawable(powerBarBg);
			game.AddDrawable(powerBar);
			game.AddDrawable(target1.Box);
		}

		// The Do() method is called every 20 ms, do any animation here.
		// You may want to use the time field to adjust the speed of animation
		public void Do()
		{
			HandleKeys();
			barrel.Angle = elevation;

			for (int i = 0; i < bullets.Count; i++)
			{
				Vector2 velocity = bullets[i].Velocity;
				velocity.y += gravity;
				bullets[i].Velocity = velocity;
				bullets[i].Box.Translate(velocity.x, velocity.y);

				if (bullets[i].Box.Y <= -bullets[i].Box.Height || bullets[i].Box.X >= game.DefaultWidth + bullets[i].Box.Width)
				{
					game.RemoveDrawable(bullets[i].Box);
					bullets.Remove(bullets[i]);
				}
			}

			powerBar.Width = force / maxForce * (powerBarBg.Width-15) ;
			powerBar.X = powerBarPos + powerBar.Width / 2;

			CheckCollisions();
		}

		public void HandleKeys()
		{

			if (keys.Contains(Gdk.Key.Left))
			{
				elevation += 0.1;

				if (elevation > 1.6)
					elevation = 1.6;
			}
			if (keys.Contains(Gdk.Key.Right))
			{
				elevation -= 0.1;

				if (elevation < 0)
					elevation = -0;
			}

			if (force > maxForce)
			{
				force = maxForce;
			}
			if (force < 0)
			{
				force = 0;
			}

			if (keys.Contains(Gdk.Key.space))
			{
				force += forceIncrease;
			}
			else if (force > 0)
			{
				FireCannon();
				force = 0;
			}
			else
			{
				force = 0;
			}

			lastKeys = keys;
		}
		// The HandleKey() method is called if the user presses any keyboard key 
		public void OnKeyPress(Gdk.Key key)
		{
			if (!keys.Contains(key))
			{
				keys.Add(key);
			}
		}

		public void OnKeyRelease(Gdk.Key key)
		{
			if (keys.Contains(key))
			{
				keys.Remove(key);
			}
		}

		public void FireCannon()
		{
			Vector2 barrelTip;
			barrelTip.x = barrel.X + barrel.Width / 2 * Math.Cos(barrel.Angle);
			barrelTip.y = barrel.Y + barrel.Width / 2 * Math.Sin(barrel.Angle);
			Bullet bullet = new Bullet(barrelTip, barrel.Angle, force);
			game.AddDrawable(bullet.Box);
			bullets.Add(bullet);
		}

		public void CheckCollisions()
		{
			for (int i = 0; i < bullets.Count; i++)
			{
				for (int j = 0; j < gameObjects.Count; j++)
				{
					if (IsColliding(bullets[i].Box, gameObjects[j].Box))
					{
						Console.WriteLine("Butt");
						//FIXME: Do physicstuff here maybe
					}
				}
			}
		}

		public bool IsColliding(Rectangle r1, Rectangle r2)
		{
			if (r1.X + r1.Width / 2 > r2.X - r2.Width && r1.X - r1.Width / 2 < r2.X + r2.Width)
			{
				if (r1.Y + r1.Height / 2 > r2.Y - r2.Height && r1.Y - r1.Height / 2 < r2.Y + r2.Height)
				{
					return true;
				}
			}
			return false;
		}

		class GameObject
		{
			private Rectangle box;

			public GameObject()
			{
				box = new Rectangle(0, 0, 20, 20);
			}

			public Rectangle Box
			{
				get { return box; }
				set { box = value; }
			}

		}

		class Bullet : GameObject
		{
			private Vector2 velocity;

			public Bullet(Vector2 position, double direction, double force)
			{
				Box = new Rectangle(position.x, position.y, 10, 10);
				velocity.x = force * Math.Cos(direction);
				velocity.y = force * Math.Sin(direction);
			}

			public Vector2 Velocity
			{
				get
				{
					return velocity;
				}
				set
				{
					velocity = value;
				}
			}
		}

		struct Vector2
		{
			public double x;
			public double y;

			public static Vector2 operator +(Vector2 v1, Vector2 v2)
			{
				Vector2 newVector = new Vector2()
				{
					x = v1.x + v2.x,
					y = v1.y + v2.y
				};
				return newVector;
			}

			public static Vector2 operator *(Vector2 v1, double d)
			{
				Vector2 newVector = new Vector2()
				{
					x = v1.x * d,
					y = v1.y * d
				};
				return newVector;
			}
		}
	}
}

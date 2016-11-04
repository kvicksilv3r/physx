using System;
using System.Collections.Generic;
using Cairo;

//FIXME:
// Köra tanken?

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
		private double elevation = 1.57 / 2;
		private double TimeToSpawn = 3;

		private double force = 0.0;
		private double powerBarLength = 8;
		private double maxForce = 60;
		private double forceIncrease = 1;

		private double gravity = -1;

		private Rectangle powerBar;
		private Rectangle powerBarBg;

		private int targetSize = 50;

		private Random random = new Random();

		private GameObject orangePortal;
		private GameObject bluePortal;

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

		double[] colorRed = new double[] { 1, 0, 0 };
		double[] colorBlue = new double[] { 31 / 255.0, 175 / 255.0, 242 / 255.0 };
		double[] colorOrange = new double[] { 249 / 255f, 161 / 255f, 29 / 255f };

		private List<Gdk.Key> keys = new List<Gdk.Key>();
		private List<Gdk.Key> lastKeys = new List<Gdk.Key>();

		// Create initial data objects here
		public Actor(GameApp game)
		{
			this.game = game;  // Do not remove this line!

			// Add your initialisations below this comment

			Console.WriteLine("Aim the cannon with left and right arrow keys");
			Console.WriteLine("Hold down space to adjust power. Release to shoot");

			bluePortal = new GameObject("portal2");
			bluePortal.Box = new Rectangle(40, 450, 50, 200, colorBlue);
			bluePortal.Active = false;
			game.AddDrawable(bluePortal.Box);
			gameObjects.Add(bluePortal);

			orangePortal = new GameObject("portal");
			orangePortal.Active = false;
			orangePortal.Box = new Rectangle(1240, 450, 50, 200, colorOrange);
			game.AddDrawable(orangePortal.Box);
			gameObjects.Add(orangePortal);

			powerBar = new Rectangle(powerBarPos, game.DefaultHeight - 40, 0, 20, colorRed);
			powerBarBg = new Rectangle(-4 + powerBarPos + (maxForce * powerBarLength) / 2, game.DefaultHeight - 40, maxForce * powerBarLength, 26);

			target1 = new GameObject("target");
			target1.Box = new Rectangle(400, 400, targetSize, targetSize);
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

			SpawnBoxes();

			for (int i = 0; i < gameObjects.Count; i++)
			{
				if (gameObjects[i].Active)
				{
					Vector2 velocity = gameObjects[i].Velocity;
					velocity.y += gravity;
					gameObjects[i].Velocity = velocity;
					gameObjects[i].Box.Translate(velocity.x, velocity.y);

					if (gameObjects[i].Box.Y <= -gameObjects[i].Box.Height ||
						gameObjects[i].Box.X >= game.DefaultWidth + gameObjects[i].Box.Width)
					{
						game.RemoveDrawable(gameObjects[i].Box);
						gameObjects.Remove(gameObjects[i]);
					}
				}
			}

			powerBar.Width = force / maxForce * (powerBarBg.Width - 15);
			powerBar.X = powerBarPos + powerBar.Width / 2;

			CheckCollisions();
		}

		public void HandleKeys()
		{

			if (keys.Contains(Gdk.Key.Left))
			{
				elevation += 0.05;

				if (elevation > 1.57)
					elevation = 1.57;
			}
			if (keys.Contains(Gdk.Key.Right))
			{
				elevation -= 0.05;

				if (elevation < 0)
					elevation = -0;
			}

			if (keys.Contains(Gdk.Key.Escape))
			{
				Environment.Exit(0);
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
			Bullet bullet = new Bullet(barrelTip, barrel.Angle, force, "bullet");
			game.AddDrawable(bullet.Box);
			gameObjects.Add(bullet);
		}

		public void CheckCollisions()
		{
			for (int i = 0; i < gameObjects.Count; i++)
			{
				for (int j = 0; j < gameObjects.Count; j++)
				{
					if (i != j && IsColliding(gameObjects[i].Box, gameObjects[j].Box))
					{
						if (gameObjects[j].Type == "target" && !gameObjects[j].Active)
						{
							gameObjects[j].Velocity = gameObjects[i].Velocity * 0.66;
							gameObjects[i].Velocity = gameObjects[i].Velocity * 0.33;
							gameObjects[j].Active = true;
						}
						else if(gameObjects[i].Type == "bullet" && gameObjects[j].Type == "bullet")
						{
							gameObjects[j].Velocity = gameObjects[i].Velocity * 0.66;
							gameObjects[i].Velocity = gameObjects[i].Velocity * 0.33;
						}
						else if (gameObjects[j].Type == "portal")
						{
							Teleport(i, false);
						}
						else if (gameObjects[j].Type == "portal2")
						{
							Teleport(i, true);
						}
					}
				}
			}
		}

		public void Teleport(int index, bool blue)
		{
			double offset;

			if (blue)
			{
				offset = gameObjects[index].Box.X - bluePortal.Box.X;
				gameObjects[index].Box.Y = orangePortal.Box.Y + orangePortal.Box.Height / 2 + gameObjects[index].Box.Height / 2 + 1;
				gameObjects[index].Box.X = orangePortal.Box.X + offset;
			}

			else
			{
				offset = gameObjects[index].Box.Y - orangePortal.Box.Y;

				if (offset > orangePortal.Box.Height / 2 - 50 || offset < -orangePortal.Box.Height / 2 + 10)
				{
					gameObjects[index].Box.Y = bluePortal.Box.Y - bluePortal.Box.Height / 2 - gameObjects[index].Box.Height / 2 - 1;
					offset = gameObjects[index].Box.X - orangePortal.Box.X;
					gameObjects[index].Box.X = bluePortal.Box.X + offset;

				}
				else
				{
					gameObjects[index].Box.X = bluePortal.Box.X + bluePortal.Box.Width / 2 + gameObjects[index].Box.Width / 2 + 1;
					gameObjects[index].Box.Y = bluePortal.Box.Y + offset;
				}
			}
		}

		public void SpawnBoxes()
		{
			TimeToSpawn -= 1/20f;
			if(TimeToSpawn <= 0)
			{
				TimeToSpawn = 3;

				GameObject g = new GameObject("target");
				g.Box = new Rectangle(random.Next(200, game.DefaultWidth - 200), random.Next(300, game.DefaultHeight - 200), targetSize, targetSize,
					new double[] { random.Next(256)/255f, random.Next(256) / 255f, random.Next(256) / 255f });
				game.AddDrawable(g.Box);
				gameObjects.Add(g);
			}
		}

		public bool IsColliding(Rectangle r1, Rectangle r2)
		{
			if (r1.X + r1.Width / 2 > r2.X - r2.Width / 2 && r1.X - r1.Width / 2 < r2.X + r2.Width / 2)
			{
				if (r1.Y + r1.Height / 2 > r2.Y - r2.Height / 2 && r1.Y - r1.Height / 2 < r2.Y + r2.Height / 2)
				{
					return true;
				}
			}
			return false;
		}

		class GameObject
		{
			protected string type;
			protected Vector2 velocity;
			protected Rectangle box;
			protected bool active = false;

			public GameObject(string type)
			{
				box = new Rectangle(0, 0, 20, 20);
				this.type = type;
			}

			public Rectangle Box
			{
				get { return box; }
				set { box = value; }
			}

			public string Type
			{
				get { return type; }
			}

			public Vector2 Velocity
			{
				get { return velocity; }
				set { velocity = value; }
			}

			public bool Active
			{
				get { return active; }
				set { active = value; }
			}

		}

		class Bullet : GameObject
		{
			public Bullet(Vector2 position, double direction, double force, string type) : base(type)
			{
				Box = new Rectangle(position.x, position.y, 10, 10);
				velocity.x = force * Math.Cos(direction);
				velocity.y = force * Math.Sin(direction);
				active = true;
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

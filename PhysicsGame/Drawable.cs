using System;
using Cairo;

// The Drawable class is very simple, it can draw black-filled rectangles and filled circles.
// If you want to extend this with additional objects or other colours, feel free; check the documentation
// at http://docs.go-mono.com/?link=T%3aCairo.Context for the types of vector objects that can be rendered.

namespace Game
{
	public class Drawable
	{
		protected double x = 0.0, y = 0.0;
		protected double angle = 0.0;


		public Drawable(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		public virtual void Draw(Context cr)
		{

		}

		public void Translate(double xoffset, double yoffset)
		{
			x += xoffset;
			y += yoffset;
		}

		public void Rotate(double angleoffset)
		{
			angle += angleoffset;
		}

		public double X
		{
			get { return x; }
			set { x = value; }
		}

		public double Y
		{
			get { return y; }
			set { y = value; }
		}

		public double Angle
		{
			get { return angle; }
			set { angle = value; }
		}
	}

	public class Rectangle : Drawable
	{
		private double width, height;
		private int[] color;

		public Rectangle(double x, double y, double width, double height) : base(x, y)
		{
			color = new int[] { 0, 0, 0 };
			this.width = width;
			this.height = height;
		}
        public Rectangle(double x, double y, double width, double height, int[] color) : base(x, y)
		{
			this.width = width;
			this.height = height;
			this.color = color;
		}

		// Rectangles are positioned around their centre point
		public override void Draw(Context cr)
		{

			cr.SetSourceRGB(color[0], color[1], color[2]);
			cr.Translate(x, y);
			cr.Rotate(angle);
			cr.Rectangle(new PointD(-width / 2, -height / 2), width, height);
			cr.Fill();
		}
		
		public double Width
		{
			get { return width; }
			set { width = value; }
		}

		public double Height
		{
			get { return height; }
			set { height = value; }
		}
	}

	public class Circle : Drawable
	{
		protected double radius;

		public Circle(double x, double y, double radius) : base(x, y)
		{
			this.radius = radius;
		}

		public override void Draw(Context cr)
		{
			cr.SetSourceRGB(0.0, 0.0, 0.0);
			cr.Arc(x, y, radius, 0.0, 2 * Math.PI);
			cr.Fill();
		}

		public double Radius
		{
			get { return radius; }
			set { radius = value; }
		}
	}
}

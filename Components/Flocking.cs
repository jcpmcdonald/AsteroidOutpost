using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	class Flocking : Component
	{
		public Flocking(int entityID)
			: base(entityID)
		{
		}


		public int FlockID { get; set; }
		//public int? Target { get; set; }


		[XmlIgnore]
		[JsonIgnore]
		public Vector2 Cohesion { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		public Vector2 Separation { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		public Vector2 Alignment { get; set; }


		public float AccelerationFactor { get; set; }
		public float BoidVelocityTrim { get; set; }
		public float CohesionFactor { get; set; }
		public float SeparationFactor { get; set; }
		public float AlignmentFactor { get; set; }


		public float CohereNeighbourDistance { get; set; }
		public float SeparationDistance { get; set; }
		

		[XmlIgnore]
		[JsonIgnore]
		public Vector2 BoidsVelocity { get; set; }


		public bool DebugView { get; set; }
	}
}

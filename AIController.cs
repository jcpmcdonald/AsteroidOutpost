using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Units;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost
{
	class AIController : Controller
	{
		private IControllerIDProvider actorIDProvider;

		public AIController(AsteroidOutpostScreen theGame, IControllerIDProvider actorIDProvider, Force primaryForce)
			: base(theGame, ControllerRole.AI, primaryForce)
		{
			this.actorIDProvider = actorIDProvider;
		}

		public AIController(IControllerIDProvider actorIDProvider, BinaryReader br)
			: base(br)
		{
			this.actorIDProvider = actorIDProvider;
		}


		public override void Update(TimeSpan deltaTime)
		{
			// Do some AI logic here. This is very time consuming I'm sure
			// TODO: Do some real AI logic here, or better yet, script it
			var aiEntities = from ent in theGame.Entities where ent.OwningForce == PrimaryForce select ent;

			foreach(Entity aiEntity in aiEntities)
			{
				if(aiEntity is Ship)
				{
					Ship aiShip = (Ship)aiEntity;

					foreach (Entity entity in theGame.Entities)
					{

						// Only look at entities that are alive
						if (entity.HitPoints.Get() > 0)
						{
							// TODO: Fix possible NPE
							if ((aiShip.Target == null && isEnemy(entity)) ||
								(isEnemy(entity) && aiShip.Position.Distance(entity.Position) < aiShip.Position.Distance(aiShip.Target.Position)))
							{
								aiShip.SetTarget(entity);
							}
						}
					}
				}
			}

		}

		private bool isEnemy(Entity entity)
		{
			return (entity.OwningForce.Team == Team.Team1 || entity.OwningForce.Team == Team.Team2);
		}
	}
}

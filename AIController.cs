using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost
{
	class AIController : Controller
	{

		public AIController(World world, Force primaryForce)
			: base(world, ControllerRole.AI, primaryForce)
		{
		}

		public AIController(BinaryReader br)
			: base(br)
		{
		}


		public override void Update(TimeSpan deltaTime)
		{
			// Do some AI logic here. This is very time consuming I'm sure
			// TODO: Do some real AI logic here, or better yet, script it
			//var aiEntities = from ent in world.Entities where ent.OwningForce == PrimaryForce select ent;

			//foreach(Entity aiEntity in aiEntities)
			//{
				//if(aiEntity is Ship)
				//{
				//    Ship aiShip = (Ship)aiEntity;

				//    foreach (Entity entity in world.Entities)
				//    {

				//        // Only look at entities that are alive
				//        if (entity.HitPoints.Get() > 0)
				//        {
				//            // TODO: Fix possible NPE
				//            if ((aiShip.Target == null && isEnemy(entity)) ||
				//                (isEnemy(entity) && aiShip.Position.Distance(entity.Position) < aiShip.Position.Distance(aiShip.Target.Position)))
				//            {
				//                aiShip.SetTarget(entity);
				//            }
				//        }
				//    }
				//}
			//}

		}

		private bool IsEnemy(int entityID)
		{
			return (world.GetOwningForce(entityID).Team == Team.Team1 || world.GetOwningForce(entityID).Team == Team.Team2);
		}
	}
}

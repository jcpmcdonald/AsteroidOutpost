using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Networking
{
	class AOReflectiveIncomingMessage : AOIncomingMessage
	{
		// A reference to the object we wish to invoke a method on
		private readonly int targetObjectID;

		// Essentially a reference to the method we plan to invoke
		private String methodName;
		//private MethodInfo targetMethodInfo;

		// The list of parameters for the method
		private Type[] types;
		private Object[] parameters;


		/// <summary>
		/// Create a new incoming message that will target the given object and read the remaining information from the stream
		/// </summary>
		/// <param name="theTargetObjectID">The ID of the target of the message</param>
		/// <param name="stream">The stream to read the additional information from</param>
		public AOReflectiveIncomingMessage(int theTargetObjectID, Stream stream)
		{
			targetObjectID = theTargetObjectID;

			// Just make a binary reader if they didn't give me one
			deserialize(new BinaryReader(stream));
		}


		/// <summary>
		/// Create a new incoming message that will target the given object and read the remaining information from the binary reader
		/// </summary>
		/// <param name="theTargetObjectID">The ID of the target of the message</param>
		/// <param name="br">The binary reader to read the additional information from</param>
		public AOReflectiveIncomingMessage(int theTargetObjectID, BinaryReader br)
		{
			targetObjectID = theTargetObjectID;

			deserialize(br);
		}


		/// <summary>
		/// Deserialize additional information from a BinaryReader
		/// Note: This is done inside the Thread that read the data
		/// </summary>
		/// <param name="br">The binary reader to read additional information from</param>
		protected override void deserialize(BinaryReader br)
		{
			// Deserialize the method and parameters
			methodName = br.ReadString();
			int parameterCount = br.ReadInt32();

			types = new Type[parameterCount];
			parameters = new object[parameterCount];

			// Populate the parameters
			for (int i = 0; i < parameterCount; i++)
			{
				// Read the type of parameter that is coming through
				types[i] = Type.GetType(br.ReadString());

				// Read the parameter value based on the parameter type
				if (types[i] == typeof(bool))
				{
					parameters[i] = br.ReadBoolean();
				}
				else if (types[i] == typeof(string))
				{
					parameters[i] = br.ReadString();
				}
				else if (types[i] == typeof(int))
				{
					parameters[i] = br.ReadInt32();
				}
				else if (types[i] == typeof(float))
				{
					parameters[i] = br.ReadSingle();
				}
				else if (types[i] == typeof(double))
				{
					parameters[i] = br.ReadDouble();
				}
				else if (types[i] == typeof(Vector2))
				{
					parameters[i] = new Vector2(br.ReadSingle(), br.ReadSingle());
				}
				else if (types[i] == typeof(byte[]))
				{
					int count = br.ReadInt32();
					parameters[i] = br.ReadBytes(count);
				}
				else if (types[i].IsSubclassOf(typeof(Component)))
				{
					// Use reflection to make a new component of... whatever type was sent to us
					ConstructorInfo componentConstructor = types[i].GetConstructor(new Type[] { typeof(BinaryReader) });
					parameters[i] = componentConstructor.Invoke(new object[] { br });
				}
				else if (types[i] == typeof(Force) || types[i] == typeof(Actor) || types[i] == typeof(AIActor))
				{
					// Use reflection to make a new force
					ConstructorInfo entityConstructor = types[i].GetConstructor(new Type[] { typeof(BinaryReader) });
					parameters[i] = entityConstructor.Invoke(new object[] { br });
				}
				else
				{
					// Note to self: If this is encountered, it will leave some number of garbage bytes in the stream and cause
					// the rest of the stream to become corrupted
					Debug.Assert(false, GetType() + " needs an other handler for deserializing type: " + types[i]);
				}
			}
		}


		/// <summary>
		/// Execute this message. This should only be done from the Game Thread to avoid threading issues
		/// </summary>
		public override void Execute(AsteroidOutpostScreen theGame, AONetwork network, TimeSpan deltaTime)
		{
			//List<Type> reflectionTagets = Assembly.GetCallingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(IReflectionTarget))).ToList();

			// Look up the intended object
			Object targetObject;
			if (targetObjectID == AONetwork.SpecialTargetNetworkingClass)
			{
				// Reserved for the network
				targetObject = network;
			}
			else if (targetObjectID == AONetwork.SpecialTargetTheGame)
			{
				// Reserved for the game
				targetObject = theGame;
			}
			else
			{
				// For all other IDs, look up the corresponding Entity
				targetObject = theGame.GetComponent(targetObjectID);

				if (targetObject == null)
				{
					// Note: Further down the road, this assert might be ok because an entity could be deleted on
					//       the server at the same time that a client does something with it. But for now, it's an error
					Debugger.Break();
					Debug.Fail("We could not find the object being referenced. Maybe it has been deleted?");
					return;
				}
			}

			// Look up the method we want to invoke
			MethodInfo targetMethodInfo = targetObject.GetType().GetMethod(methodName, types);

			if (targetMethodInfo == null)
			{
				Console.WriteLine(GetType() + " couldn't find method: " + methodName);
				Debugger.Break();
				//throw new MissingMethodException("Couldn't find method: " + methodName);
			}


			foreach (Object param in parameters)
			{
				if(param is ISerializable)
				{
					((ISerializable)param).PostDeserializeLink(theGame);
				}
			}


			// Do it!
			targetMethodInfo.Invoke(targetObject, parameters);

			// TODO: Bring the target object up-to-date by calling Update();  Something like:
			/*
			if(targetObject is Entity)
			{
				Entity targetEntity = (Entity)targetObject;
				// We need to subtract the deltaTime because this object is just about to receive an Update(deltaTime)
				targetEntity.Update(aoGame.CurrentGameTime - packetGameTime - deltaTime);
			}
			//*/
		}
	}
}

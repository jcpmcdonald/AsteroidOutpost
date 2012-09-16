using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Networking
{
	class AOReflectiveOutgoingMessage : AOOutgoingMessage
	{
		// The AO Entity ID of the target
		private readonly int targetID;

		// The name of the target method to be invoked on the other end
		private readonly String targetMethodName;

		// The parameters for the target method
		private readonly Object[] parameters;


		/// <summary>
		/// Make a new AOMessage for serializing
		/// </summary>
		/// <param name="theTargetID">The game ID of the object you want to invoke</param>
		/// <param name="theTargetMethodName">The target method name</param>
		/// <param name="theParameters">The parameters for the target method</param>
		public AOReflectiveOutgoingMessage(int theTargetID, String theTargetMethodName, object[] theParameters)
		{
			targetID = theTargetID;

			targetMethodName = theTargetMethodName;
			parameters = theParameters;
		}

		/// <summary>
		/// Serialize this message
		/// </summary>
		/// <param name="bw">The BinaryWriter to use</param>
		public override void Serialize(BinaryWriter bw)
		{
			bw.Write(targetID);
			bw.Write(targetMethodName);
			bw.Write(parameters.Length);

			foreach(Object parameter in parameters)
			{
				Type parameterType = parameter.GetType();
				bw.Write(parameterType.AssemblyQualifiedName);

				// Write the various types to the stream
				if (parameterType == typeof(bool))
				{
					bw.Write((bool)parameter);
				}
				else if(parameterType == typeof(String))
				{
					bw.Write((String)parameter);
				}
				else if (parameterType == typeof(Int32) || parameterType == typeof(int))
				{
					bw.Write((int)parameter);
				}
				else if (parameterType == typeof(double))
				{
					bw.Write((double)parameter);
				}
				else if (parameterType == typeof(float))
				{
					bw.Write((float)parameter);
				}
				else if (parameterType == typeof(Vector2))
				{
					bw.Write((Vector2)parameter);
				}
				else if(parameterType == typeof(byte[]))
				{
					bw.Write(((byte[])parameter).Length);
					bw.Write((byte[])parameter);
				}
				else if (parameter is Component)
				{
					// TODO: Serialize this component
					Debugger.Break();
					//((Component)parameter).Serialize(bw);
				}
				else if (parameter is Entity)
				{
					((Entity)parameter).Serialize(bw);
				}
				else if (parameter is Force)
				{
					((Force)parameter).Serialize(bw);
				}
				else if (parameter is Controller)
				{
					((Controller)parameter).Serialize(bw);
				}
				else
				{
					Console.WriteLine(GetType() + " needs an other handler for serializing type: " + parameterType);
					Debugger.Break();
				}
			}
		}
	}
}

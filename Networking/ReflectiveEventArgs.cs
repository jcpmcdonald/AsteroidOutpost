using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AsteroidOutpost.Networking
{
	public delegate void ReflectiveEventHandler(ReflectiveEventArgs e);

	public class ReflectiveEventArgs : EventArgs
	{
		public ReflectiveEventArgs(IReflectionTarget target, String theRemoteMethodName, object[] theRemoteMethodParameters)
		{
			TargetID = target.EntityID;
			RemoteMethodName = theRemoteMethodName;
			RemoteMethodParameters = theRemoteMethodParameters;

#if DEBUG
			// Check that the method exists and Fail Fast
			Type[] types = new Type[theRemoteMethodParameters.Length];
			for (int i = 0; i < theRemoteMethodParameters.Length; i++)
			{
				types[i] = theRemoteMethodParameters[i].GetType();
			}
			MethodInfo targetMethodInfo = target.GetType().GetMethod(theRemoteMethodName, types);

			if(targetMethodInfo == null)
			{
				Console.WriteLine("Warning!! " + GetType() + " is referring to a method that can not be found!");
				//Debugger.Break();
			}
#endif
		}


		public ReflectiveEventArgs(int targetID, String theRemoteMethodName, object[] theRemoteMethodParameters)
		{
			TargetID = targetID;
			RemoteMethodName = theRemoteMethodName;
			RemoteMethodParameters = theRemoteMethodParameters;
		}




		public int TargetID { get; private set; }

		public string RemoteMethodName { get; private set; }

		public object[] RemoteMethodParameters { get; private set; }
	}
}

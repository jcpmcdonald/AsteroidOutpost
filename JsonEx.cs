﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	public static class JsonEx
	{
		public static void Extend(this JObject receiver, JObject donor)
		{
			foreach (var property in donor)
			{
				JObject receiverValue = receiver[property.Key] as JObject;
				JObject donorValue = property.Value as JObject;
				if (receiverValue != null && donorValue != null)
				{
					Extend(receiverValue, donorValue);
				}
				else
				{
					receiver[property.Key] = property.Value;
				}
			}
		}
	}
}

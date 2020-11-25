/*
 * Copyright 2007,2008 Davelopware Ltd
 * 
 * http://www.davelopware.com/txtmnu/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed
 * under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 *
 */
using System;
using System.Reflection;
using System.Collections;

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// Contains some basic functionality plus heuristics for displaying reflected object properties
	/// </summary>
	public class ReflectedObjectRenderer
	{
		public static string RenderObjectProperty(MenuSession session, PropertyInfo prop, object obj)
		{
			string result = "{unable to access value}";
			if (prop == null)
				return "";

			try
			{
				Type instanceType = prop.ReflectedType;

				MethodInfo propGet = prop.GetGetMethod(true);
				ParameterInfo[] parameters = propGet.GetParameters();
				if (parameters.Length == 0)
				{
					object propValue = prop.GetValue(obj,null);
					if (propValue is ICollection)
					{
						result = "{";
						bool first = true;
						foreach (object innerObj in (propValue as ICollection))
						{
							if ( first )
								first = false;
							else
								result += ",";
							if (innerObj == null)
								result += "null";
							else
								result += innerObj.ToString();
						}
						result += "}";
					}
					else
					{
						result = prop.GetValue(obj, null).ToString();
					}
				}
				else
				{
					string paramsDesc = string.Empty;
					foreach (ParameterInfo param in parameters)
					{
						if (paramsDesc != string.Empty)
							result += ",";
						paramsDesc += param.Name;
					}
					result += "[" + paramsDesc + "]";
				}
			}
			catch (Exception ex)
			{
				result = result + "[Exception:" + ex.Message + "]";
			}

			return result;
		}
	}
}

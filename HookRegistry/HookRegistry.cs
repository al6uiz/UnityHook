﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyTitle("HookRegistry")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
namespace Hooks
{
	public class HookRegistry
	{
		public delegate object Callback(string typeName,string methodName,params object[] args);

		List<Callback> callbacks = new List<Callback>();

		public static object OnCall(params object[] args)
		{
			return HookRegistry.Get().Internal_OnCall(args);
		}

		// Add a hook listener
		public static void Register(Callback cb)
		{
			HookRegistry.Get().callbacks.Add(cb);
		}

		// Remove a hook listener
		public static void Unregister(Callback cb)
		{
			HookRegistry.Get().callbacks.Remove(cb);
		}

		List<object> activeHooks = new List<object>();

		void LoadRuntimeHooks()
		{
			// Install hook listeners by classes that have the [RuntimeHook] attribute
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				var hooks = type.GetCustomAttributes(typeof(RuntimeHookAttribute), false);
				if (hooks != null && hooks.Length > 0)
				{
					activeHooks.Add(type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]));
				}
			}
		}

		object Internal_OnCall(params object[] args)
		{
			if (args.Length == 0)
			{
				// Installed hook is broken:
				throw new Exception("OnCall called with 0 args");
			}

			var rmh = (RuntimeMethodHandle)args[0];
			var method = MethodBase.GetMethodFromHandle(rmh);
			var typeName = method.DeclaringType.FullName;
			var methodName = method.Name;
			Debug.Log(String.Format("{0}.{1}(...)", typeName, methodName));
			foreach (var cb in callbacks)
			{
				var o = cb(typeName, methodName, args.Slice(1));
				if (o != null)
				{
					return o;
				}
			}
			return null;
		}

		static HookRegistry _instance;

		public static HookRegistry Get()
		{
			if (_instance == null)
			{
				_instance = new HookRegistry();
				_instance.LoadRuntimeHooks();
			}
			return _instance;
		}
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class RuntimeHookAttribute : Attribute
	{
	}
}
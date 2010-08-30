#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Lokad.Quality;

namespace Lokad.Cqrs
{
	[UsedImplicitly]
	public sealed class CloudEngineHost : ICloudEngineHost
	{
		readonly IContainer _container;
		readonly ILog _log;
		readonly IEnumerable<IEngineProcess> _serverProcesses;

		public CloudEngineHost(
			IContainer container,
			ILogProvider provider,
			IEnumerable<IEngineProcess> serverProcesses)
		{
			_container = container;
			_serverProcesses = serverProcesses;
			_log = provider.LogForName(this);
		}

		public Task Start(CancellationToken token)
		{
			_log.Info("Starting host");

			var tasks = _serverProcesses.ToArray(p => p.Start(token));

			return Task.Factory.ContinueWhenAll(tasks, t => _log.Info("Stopped host"));
		}

		public void Initialize()
		{
			_log.Info("Initializing host");

			foreach (var process in _serverProcesses)
			{
				process.Initialize();
			}
		}

		public TService Resolve<TService>()
		{
			try
			{
				return _container.Resolve<TService>();
			}
			catch (TargetInvocationException e)
			{
				throw Errors.Inner(e);
			}
		}

		public void Dispose()
		{
			_container.Dispose();
		}
	}
}
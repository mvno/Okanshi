using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Okanshi.Test
{
	public class MonitorApiTest : IDisposable
	{
		private readonly MonitorApi _monitorApi;
		private readonly string _monitorUrl = "http://localhost:13004/" + AppDomain.CurrentDomain.SetupInformation.ApplicationName + "/";

		public MonitorApiTest()
		{
			_monitorApi = new MonitorApi(new MonitorApiOptions { HttpPrefix = _monitorUrl });
			_monitorApi.Start();
			HealthChecks.Clear();
			CSharp.Monitor.ResetCounters();
		}

		public void Dispose()
		{
			_monitorApi.Stop();
		}

		public class AssemblyDependency
		{
			public string Name;
			public string Version;
		}

		[Fact]
		public void Asking_for_dependencies_gets_the_current_assembly()
		{
			var httpClient = new HttpClient();
			var result = httpClient.GetStringAsync(_monitorUrl + "dependencies").Result;

			var listOfAssemblies = JsonConvert.DeserializeObject<List<AssemblyDependency>>(result);

			listOfAssemblies.Single(depencency => depencency.Name == Assembly.GetExecutingAssembly().GetName().Name);
		}

		[Fact]
		public void Asking_for_healtchecks_runs_the_healthchecks()
		{
			var httpClient = new HttpClient();

			var result = httpClient.GetStringAsync(_monitorUrl + "healthchecks").Result;

			JsonConvert.DeserializeObject<Dictionary<string, bool>>(result).Should().BeEmpty();
		}

		[Fact]
		public void Asking_for_statistics_returns_the_statistics()
		{
			var httpClient = new HttpClient();

			var result = httpClient.GetStringAsync(_monitorUrl).Result;

			JsonConvert.DeserializeObject<Dictionary<string, bool>>(result).Should().BeEmpty();
		}
	}
}

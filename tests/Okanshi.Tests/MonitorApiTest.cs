using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Okanshi.Test
{
	public class MonitorApiTest : IDisposable
	{
		private readonly MonitorApi _monitorApi;
		private readonly string _monitorUrl = "http://localhost:13004/" + Guid.NewGuid() + "/";

		public MonitorApiTest()
		{
			_monitorApi = new MonitorApi(new MonitorApiOptions { HttpPrefix = _monitorUrl });
			_monitorApi.Start();
			Thread.Sleep(500);
			HealthChecks.Clear();
			CSharp.Monitor.ResetCounters();
		}

		public void Dispose()
		{
			_monitorApi.Stop();
		}

		public class Response<T>
		{
			public string Version;
			public T Data;
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

			var response = JsonConvert.DeserializeObject<Response<List<AssemblyDependency>>>(result);

			var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			response.Data.Should().Contain(depencency => depencency.Name == assemblyName);
		}

		[Fact]
		public void Asking_for_healtchecks_runs_the_healthchecks()
		{
			var httpClient = new HttpClient();

			var result = httpClient.GetStringAsync(_monitorUrl + "healthchecks").Result;

			JsonConvert.DeserializeObject<Response<Dictionary<string, bool>>>(result).Data.Should().BeEmpty();
		}

		[Fact]
		public void Asking_for_statistics_returns_the_statistics()
		{
			var httpClient = new HttpClient();

			var result = httpClient.GetStringAsync(_monitorUrl).Result;

			JsonConvert.DeserializeObject<Response<Dictionary<string, bool>>>(result).Data.Should().BeEmpty();
		}

		[Fact]
		public void Stopping_api_multiple_times_does_not_hang()
		{
			CSharp.Monitor.Stop();
			_monitorApi.Stop();
			_monitorApi.Stop();
			_monitorApi.Stop();
		}
	}
}

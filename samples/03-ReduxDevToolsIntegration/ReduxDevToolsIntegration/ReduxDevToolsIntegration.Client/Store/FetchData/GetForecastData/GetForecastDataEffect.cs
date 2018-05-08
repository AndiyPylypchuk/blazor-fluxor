﻿using Blazor.Fluxor;
using Microsoft.AspNetCore.Blazor;
using ReduxDevToolsIntegration.Shared;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReduxDevToolsIntegration.Client.Store.FetchData.GetForecastData
{
	public class GetForecastDataEffect : Effect<GetForecastDataAction>
	{
		private readonly HttpClient HttpClient;

		public GetForecastDataEffect(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public override async	Task<IAction[]> HandleAsync(GetForecastDataAction action)
		{
			try
			{
				WeatherForecast[] forecasts =
					await HttpClient.GetJsonAsync<WeatherForecast[]>("api/SampleData/WeatherForecasts");
				return new IAction[] { new GetForecastDataSuccessAction(forecasts) };
			}
			catch (Exception e)
			{
				return new IAction[] { new GetForecastDataFailedAction(errorMessage: e.Message) };
			}
		}
	}
}

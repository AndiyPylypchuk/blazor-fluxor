﻿using Blazor.Fluxor;
using System.Net.Http;
using WeatherForecastSample.Shared;
using Microsoft.AspNetCore.Blazor;
using System.Threading.Tasks;
using System;

namespace WeatherForecastSample.Client.Store.FetchData.GetForecastData
{
	public class GetForecastDataEffect : Effect<GetForecastDataAction>
	{
		private readonly HttpClient HttpClient;

		public GetForecastDataEffect(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public override async Task<IAction[]> HandleAsync(GetForecastDataAction action)
		{
			try
			{
				WeatherForecast[] forecasts = await HttpClient.GetJsonAsync<WeatherForecast[]>("/api/SampleData/WeatherForecasts");
				return new IAction[] { new GetForecastDataSuccessAction(forecasts) };
			}
			catch (Exception e)
			{
				return new IAction[] { new GetForecastDataFailedAction(errorMessage: e.Message) };
			}
		}
	}
}

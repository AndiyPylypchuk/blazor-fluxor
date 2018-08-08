﻿using Blazor.Fluxor;
using ReduxDevToolsIntegration.Shared;

namespace ReduxDevToolsIntegration.Client.Store.FetchData
{
	public class GetForecastDataSuccessAction : IAction
	{
		public WeatherForecast[] WeatherForecasts { get; private set; }

		public GetForecastDataSuccessAction(WeatherForecast[] weatherForecasts)
		{
			WeatherForecasts = weatherForecasts;
		}
	}
}
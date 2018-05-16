﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blazor.Fluxor.DependencyInjection.DependencyScanners
{
	internal static class FeaturesRegistration
	{
		internal static IEnumerable<DiscoveredFeatureInfo> DiscoverFeatures(IServiceCollection serviceCollection,
			IEnumerable<Type> allCandidateTypes, IEnumerable<DiscoveredReducerInfo> discoveredReducerInfos)
		{
			Dictionary<Type, IGrouping<Type, DiscoveredReducerInfo>> discoveredReducerInfosByStateType = discoveredReducerInfos
				.GroupBy(x => x.StateType)
				.ToDictionary(x => x.Key);

			IEnumerable<DiscoveredFeatureInfo> discoveredFeatureInfos = allCandidateTypes
				.Select(t => new
				{
					ImplementingType = t,
					GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IFeature<>))
				})
				.Where(x => x.GenericParameterTypes != null)
				.Select(x => new DiscoveredFeatureInfo(
					implementingType: x.ImplementingType,
					stateType: x.GenericParameterTypes[0]
					)
				)
				.ToList();

			foreach (DiscoveredFeatureInfo discoveredFeatureInfo in discoveredFeatureInfos)
			{
				discoveredReducerInfosByStateType.TryGetValue(
					discoveredFeatureInfo.StateType,
					out IGrouping<Type, DiscoveredReducerInfo> discoveredFeatureInfosForFeatureState);

				RegisterFeature(
					serviceCollection,
					discoveredFeatureInfo: discoveredFeatureInfo,
					discoveredReducerInfosForFeatureState: discoveredFeatureInfosForFeatureState);
			}

			return discoveredFeatureInfos;
		}

		private static void RegisterFeature(IServiceCollection serviceCollection,
			DiscoveredFeatureInfo discoveredFeatureInfo, IEnumerable<DiscoveredReducerInfo> discoveredReducerInfosForFeatureState)
		{
			string addReducerMethodName = nameof(IFeature<object>.AddReducer);
			string getIStateFuncName = nameof(IFeature<object>.GetStateProvider);

			// Register the implementing type so we can get an instance from the service provider
			serviceCollection.AddSingleton(discoveredFeatureInfo.ImplementingType);

			// Register the IGetState<T> state retriever
			Type genericGetStateFuncType = typeof(IStateProvider<>).MakeGenericType(discoveredFeatureInfo.StateType);
			serviceCollection.AddSingleton(genericGetStateFuncType, serviceProvider =>
			{
				IFeature featureInstance = (IFeature)serviceProvider.GetService(discoveredFeatureInfo.FeatureInterfaceGenericType);
				MethodInfo getIStateFuncMethodInfo = featureInstance.GetType().GetMethod(getIStateFuncName);
				object getIStateFunc = getIStateFuncMethodInfo.Invoke(featureInstance, null);
				return getIStateFunc;
			});

			// Register a factory for creating instance of this feature type when requested via the generic IFeature interface
			serviceCollection.AddSingleton(discoveredFeatureInfo.FeatureInterfaceGenericType, serviceProvider =>
			{
				// Create an instance of the implementing type
				IFeature featureInstance = (IFeature)serviceProvider.GetService(discoveredFeatureInfo.ImplementingType);
				if (discoveredReducerInfosForFeatureState != null)
				{
					foreach (DiscoveredReducerInfo reducerInfo in discoveredReducerInfosForFeatureState)
					{
						MethodInfo featureAddreducerMethod = discoveredFeatureInfo.ImplementingType
							.GetMethod(addReducerMethodName)
							.MakeGenericMethod(reducerInfo.ActionType);

						object reducerInstance = serviceProvider.GetService(reducerInfo.ReducerInterfaceGenericType);
						featureAddreducerMethod.Invoke(featureInstance, new object[] { reducerInstance });
					}
				}
				return featureInstance;
			});
		}

	}
}

using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteSegmentEventFactory : IRouteSegmentEventFactory
    {
        private readonly ApplicationSetting _applicationSettings;

        public RouteSegmentEventFactory(IOptions<ApplicationSetting> applicationSettings)
        {
            _applicationSettings = applicationSettings.Value;
        }

        public RouteSegmentRemoved CreateRemoved(RouteSegment routeSegment, IEnumerable<Guid> replacedBySegments, bool useApplicationName = false)
        {
            return new Events.RouteNetwork.RouteSegmentRemoved(
                nameof(Events.RouteNetwork.RouteSegmentRemoved),
                Guid.NewGuid(),
                DateTime.UtcNow,
                useApplicationName ? _applicationSettings.ApplicationName : routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment.Mrid,
                replacedBySegments.ToArray());
        }

        public RouteSegmentGeometryModified CreateGeometryModified(RouteSegment routeSegment, bool useApplicationName = false)
        {
            return new RouteSegmentGeometryModified(
                nameof(RouteSegmentGeometryModified),
                Guid.NewGuid(),
                DateTime.UtcNow,
                useApplicationName ? _applicationSettings.ApplicationName : routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment.Mrid,
                routeSegment.GetGeoJsonCoordinate());
        }

        public RouteSegmentMarkedForDeletion CreateMarkedForDeletion(RouteSegment routeSegment, bool useApplicationName = false)
        {
            return new RouteSegmentMarkedForDeletion(
                nameof(RouteSegmentMarkedForDeletion),
                Guid.NewGuid(),
                DateTime.UtcNow,
                useApplicationName ? _applicationSettings.ApplicationName : routeSegment?.ApplicationName,
                routeSegment.ApplicationInfo,
                routeSegment.Mrid);
        }

        public RouteSegmentAdded CreateAdded(RouteSegment routeSegment, RouteNode startRouteNode, RouteNode endRouteNode)
        {
            return new Events.RouteNetwork.RouteSegmentAdded(
                nameof(Events.RouteNetwork.RouteSegmentAdded),
                Guid.NewGuid(),
                DateTime.UtcNow,
                routeSegment?.ApplicationName,
                routeSegment?.ApplicationInfo,
                routeSegment?.NamingInfo,
                routeSegment?.LifeCycleInfo,
                routeSegment?.MappingInfo,
                routeSegment?.SafetyInfo,
                routeSegment.Mrid,
                startRouteNode.Mrid,
                endRouteNode.Mrid,
                routeSegment.GetGeoJsonCoordinate(),
                routeSegment?.RouteSegmentInfo);
        }
    }
}

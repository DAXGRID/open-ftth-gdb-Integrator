using System;
using System.Threading.Tasks;
using OpenFTTH.GDBIntegrator.RouteNetwork;
using OpenFTTH.GDBIntegrator.Integrator.Notifications;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.GeoDatabase;
using Microsoft.Extensions.Options;
using MediatR;

namespace OpenFTTH.GDBIntegrator.Integrator.Factories
{
    public class RouteNodeEventFactory : IRouteNodeEventFactory
    {
        private readonly ApplicationSetting _applicationSettings;
        private readonly IGeoDatabase _geoDatabase;

        public RouteNodeEventFactory(
            IOptions<ApplicationSetting> applicationSettings,
            IGeoDatabase geoDatabase)
        {
            _applicationSettings = applicationSettings.Value;
            _geoDatabase = geoDatabase;
        }

        public async Task<INotification> CreateUpdatedEvent(RouteNode before, RouteNode after)
        {
            if (before is null || after is null)
                throw new ArgumentNullException($"Parameter {nameof(before)} or {nameof(after)} cannot be null");

            var integratorRouteNode = await _geoDatabase.GetRouteNodeShadowTable(after.Mrid);

            if (AlreadyUpdated(after, integratorRouteNode))
                return new DoNothing($"{nameof(RouteNode)} with id: '{after.Mrid}' was already updated therefore do nothing.");

            await _geoDatabase.UpdateRouteNodeShadowTable(after);

            var intersectingRouteSegments = await _geoDatabase.GetIntersectingRouteSegments(after);

            if (intersectingRouteSegments.Count > 0)
                return new RollbackInvalidRouteNodeOperation(before);

            var eventId = Guid.NewGuid();
            if (after.MarkAsDeleted)
                return new RouteNodeDeleted { EventId = eventId, RouteNode = after };

            return new DoNothing($"{nameof(RouteNode)} with id: '{after.Mrid}' found not suitable action for {nameof(CreateUpdatedEvent)}.");
        }

        public async Task<INotification> CreateDigitizedEvent(RouteNode routeNode)
        {
            if (routeNode is null)
                throw new ArgumentNullException($"Parameter {nameof(routeNode)} cannot be null");

            if (IsCreatedByApplication(routeNode))
                return new DoNothing($"{nameof(RouteNode)} with id: '{routeNode.Mrid}' was created by nothing therefore do nothing.");

            // Update the 'shadow' table
            await _geoDatabase.InsertRouteNodeShadowTable(routeNode);

            var eventId = Guid.NewGuid();
            var intersectingRouteSegmentsTask = _geoDatabase.GetIntersectingRouteSegments(routeNode);
            var intersectingRouteNodesTask = _geoDatabase.GetIntersectingRouteNodes(routeNode);

            var intersectingRouteSegments = await intersectingRouteSegmentsTask;
            var intersectingRouteNodes = await intersectingRouteNodesTask;

            if (intersectingRouteNodes.Count > 0)
                return new InvalidRouteNodeOperation { RouteNode = routeNode, EventId = eventId };

            if (intersectingRouteSegments.Count == 0)
                return new RouteNodeAdded { EventId = eventId, RouteNode = routeNode };

            if (intersectingRouteSegments.Count == 1)
            {
                return new ExistingRouteSegmentSplitted
                {
                    RouteNode = routeNode,
                    EventId = eventId
                };
            }

            return new InvalidRouteNodeOperation { RouteNode = routeNode, EventId = eventId };
        }

        private async Task RollbackInvalidOperation(RouteNode rollbackToNode)
        {
            await _geoDatabase.UpdateRouteNode(rollbackToNode);
        }

        private bool AlreadyUpdated(RouteNode routeNode, RouteNode integratorRouteNode)
        {
            return routeNode.MarkAsDeleted == integratorRouteNode.MarkAsDeleted && routeNode.GetGeoJsonCoordinate() == integratorRouteNode.GetGeoJsonCoordinate();
        }

        private bool IsCreatedByApplication(RouteNode routeNode)
        {
            return routeNode.ApplicationName == _applicationSettings.ApplicationName;
        }
    }
}

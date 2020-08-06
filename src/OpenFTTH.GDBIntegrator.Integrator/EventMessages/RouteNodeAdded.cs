using System;

namespace OpenFTTH.GDBIntegrator.Integrator.EventMessages
{
    public class RouteNodeAdded
    {
        public readonly string EventType = nameof(RouteNodeAdded);
        public readonly string EventTs = DateTime.UtcNow.ToString("o");
        public readonly Guid EventId = Guid.NewGuid();
        public string CmdType { get; }
        public Guid CmdId { get; }
        public Guid NodeId { get; }
        public string Geometry { get; }
        public string ApplicationName { get; }
        public string ApplicationInfo { get; }
        public string NodeName { get; }
        public string NodeKind { get; }
        public string NodeFunction { get; }

        public RouteNodeAdded
        (
            Guid cmdId,
            Guid nodeId,
            string geometry,
            string cmdType,
            string applicationName,
            string applicationInfo,
            string nodeName,
            string nodeKind,
            string nodeFunction
        )
        {
            CmdId = cmdId;
            NodeId = nodeId;
            Geometry = geometry;
            CmdType = cmdType;
            ApplicationName = applicationName;
            ApplicationInfo = applicationInfo;
            NodeName = nodeName;
            NodeKind = nodeKind;
            NodeFunction = nodeFunction;
        }
    }
}

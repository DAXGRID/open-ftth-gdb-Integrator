using System;
using System.Text;
using Topos.Serialization;
using Newtonsoft.Json.Linq;
using OpenFTTH.GDBIntegrator.RouteNetwork;

namespace OpenFTTH.GDBIntegrator.Subscriber.Kafka.Serialize
{
    public class RouteSegmentSerializer : IMessageSerializer
    {
        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            if (message is null)
                throw new ArgumentNullException($"{nameof(ReceivedTransportMessage)} is null");

            if (message.Body is null || message.Body.Length == 0)
                return new ReceivedLogicalMessage(message.Headers, new RouteSegment(), message.Position);

            var messageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

            dynamic routeSegmentMessage = JObject.Parse(messageBody);
            var payload = routeSegmentMessage.payload;

            if (IsTombStoneMessage(payload))
                return new ReceivedLogicalMessage(message.Headers, new RouteSegment(), message.Position);

            var routeSegment = CreateRouteSegmentOnPayload(payload);

            return new ReceivedLogicalMessage(message.Headers, routeSegment, message.Position);
        }

        private bool IsTombStoneMessage(dynamic payload)
        {
            JToken afterPayload = payload["after"];
            return afterPayload.Type == JTokenType.Null;
        }

        private RouteSegment CreateRouteSegmentOnPayload(dynamic payload)
        {
            var payloadAfter = payload.after;

            return new RouteSegment
            {
                Mrid = new Guid(payloadAfter.mrid.ToString()),
                Coord = Convert.FromBase64String(payloadAfter.coord.wkb.ToString()),
                Username = payloadAfter.user_name.ToString(),
                WorkTaskMrid = payloadAfter.work_task_mrid.ToString() == string.Empty ? System.Guid.Empty : new Guid(payloadAfter.work_task_mrid.ToString()),
                ApplicationName = payloadAfter.application_name.ToString()
            };
        }

        public TransportMessage Serialize(LogicalMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

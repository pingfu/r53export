using System.Collections.Generic;
using Amazon.Route53;
using Amazon.Route53.Model;

// ReSharper disable once CheckNamespace
namespace Pingfu.Route53Export
{
    internal class Route53
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="route53Client"></param>
        /// <returns></returns>
        internal static IEnumerable<HostedZone> ListHostedZones(IAmazonRoute53 route53Client)
        {
            var allHostedZones = new List<HostedZone>();
            var response = route53Client.ListHostedZones();

            while (true)
            {
                allHostedZones.AddRange(response.HostedZones);

                if (response.NextMarker == null)
                {
                    return allHostedZones;
                }

                // make a request for the next chunk of data
                response = route53Client.ListHostedZones(new ListHostedZonesRequest { Marker = response.NextMarker });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route53Client"></param>
        /// <param name="hostedZoneId"></param>
        /// <returns></returns>
        internal static IEnumerable<ResourceRecordSet> ListResourceRecordSets(IAmazonRoute53 route53Client, string hostedZoneId)
        {
            var allResourceRecords = new List<ResourceRecordSet>();
            var response = route53Client.ListResourceRecordSets(new ListResourceRecordSetsRequest { HostedZoneId = hostedZoneId }); // , MaxItems = "1"

            while (true)
            {
                allResourceRecords.AddRange(response.ResourceRecordSets);

                if (response.IsTruncated == false)
                {
                    return allResourceRecords;
                }

                // make a request for the next chunk of data
                response = route53Client.ListResourceRecordSets(new ListResourceRecordSetsRequest { HostedZoneId = hostedZoneId, StartRecordName = response.NextRecordName });
            }
        }
    }
}

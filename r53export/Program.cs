using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Amazon;
using Amazon.Route53;
using Amazon.Route53.Model;
using Amazon.Util;
using Newtonsoft.Json;

namespace Pingfu.Route53Export
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Any())
            {
                StartNonInteractive();
            }
            else
            {
                Cmd.WriteLine("Route53 Zone Export Tool");

                StartInteractive();

                App.Exit();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void StartNonInteractive()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();

                switch (args[1])
                {
                    case "-l":
                        {
                            Cmd.WriteLine();
                            DisplayAccessKeys();
                            break;
                        }
                    case "-a":
                        {
                            Cmd.WriteLine();
                            AddProfile();
                            break;
                        }
                    case "-r":
                        {
                            // invalid number of args
                            if (args.Length != 3)
                            {
                                Cmd.WriteLine(ConsoleColor.Red, "Invalid profile name supplied.");
                                App.Exit(-1);
                            }

                            RemoveProfile(args[2]);

                            break;
                        }
                    case "-u":
                        {
                            // invalid number of args
                            if (args.Length != 3)
                            {
                                Cmd.WriteLine(ConsoleColor.Red, "Invalid profile name supplied.");
                                App.Exit(-1);
                            }

                            Cmd.WriteLine();
                            UpdateCredential(args[2]);

                            break;
                        }
                    case "-e":
                        {
                            // no profile name supplied
                            if (args.Length == 2)
                            {
                                ExportRoute53Data(ProfileManager.ListProfileNames().First());
                            }

                            // invalid number of args
                            if (args.Length != 3)
                            {
                                Cmd.WriteLine(ConsoleColor.Red, "Invalid profile name supplied.");
                                App.Exit(-1);
                            }

                            ExportRoute53Data(args[2]);

                            break;
                        }
                    default:
                        {
                            Cmd.WriteLine();
                            Cmd.WriteLine("Usage: r53export [-l] [-r name] [-a name] [-u name] [-d name] [-e name] | [-e]");
                            Cmd.WriteLine();
                            Cmd.WriteLine("Options:");
                            Cmd.WriteLine("    -a        Add a new AWS access key.");
                            Cmd.WriteLine("              Keys are stored in %LocalAppData%\\AWSToolkit\\RegisteredAccounts.json");
                            Cmd.WriteLine("              and encrypted by the Windows Data Protection API (DAPI).");
                            Cmd.WriteLine("    -l        List AWS access key profiles available to the user.");
                            Cmd.WriteLine("    -e        Export all zone data using the first available AWS access key profile.");
                            Cmd.WriteLine("    -e name   Export all zone data using a particular AWS access key profile.");
                            Cmd.WriteLine("    -u name   Update an AWS access key profile.");
                            Cmd.WriteLine("    -r name   Remove an AWS access key profile.");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Cmd.WriteLine(Cmd.Red, ex.Message);
                App.Exit(-1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void StartInteractive()
        {
            Cmd.WriteLine();

            while (true)
            {
                if (!ProfileManager.ListProfileNames().Any())
                {
                    Cmd.WriteLine("No AWS access keys profiles found.\n");

                    AddProfile();
                    continue;
                }

                SelectCredential();
                break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void SelectCredential()
        {
            Cmd.WriteLine(Cmd.White, "  == Select an AWS access key profile ==\n");

            Cmd.Write(Cmd.White, "   a. ");
            Cmd.WriteLine("Add a new AWS access key profile");
            Cmd.Write(Cmd.White, "   d. ");
            Cmd.WriteLine("Decrypt and display stored AWS access key profiles");
            Cmd.Write(Cmd.White, "   q. ");
            Cmd.WriteLine("Quit\n");

            foreach (var profile in ProfileManager.ListProfileNames().Select((name, item) => new { Name = name, Index = item }))
            {
                Cmd.Write(Cmd.Yellow, "  {0,2}. ", profile.Index);
                Cmd.WriteLine(profile.Name);
            }

            Cmd.WriteLine();

            while (true)
            {
                Cmd.Write(Cmd.White, "Enter option : ");

                var input = Console.ReadLine();
                if (input == null) return;

                switch (input.ToLower())
                {
                    case "a":
                        {
                            Cmd.WriteLine();
                            AddProfile();
                            StartInteractive();
                            break;
                        }
                    case "d":
                        {
                            Cmd.WriteLine();
                            DisplayAccessKeys();
                            return;
                        }
                    case "q":
                        {
                            App.Exit();
                            return; // keep compiler happy
                        }
                    default:
                        {
                            int index;
                            var validInput = Int32.TryParse(input, out index);

                            // bounds check input
                            if (validInput && (index >= 0 && index <= ProfileManager.ListProfileNames().Count() - 1))
                            {
                                Cmd.WriteLine();
                                SelectAction(ProfileManager.ListProfileNames().ElementAt(index));
                                return;
                            }

                            Cmd.WriteLine("Invalid selection\n");
                            break;
                        }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        static void SelectAction(string profileName)
        {
            Cmd.WriteLine(Cmd.White, "  == Select an option ({0}) ==\n", profileName);

            Cmd.Write(Cmd.White, "   e. ");
            Cmd.WriteLine("Export all zone data in Route53 using this access key profile");
            Cmd.Write(Cmd.White, "   u. ");
            Cmd.WriteLine("Update the credentials in this access key profile");
            Cmd.Write(Cmd.White, "   r. ");
            Cmd.WriteLine("Remove this access key profile");
            Cmd.Write(Cmd.White, "   q. ");
            Cmd.WriteLine("Quit\n");

            while (true)
            {
                Cmd.Write(Cmd.White, "Enter option : ");

                var input = Console.ReadLine();
                if (input == null) return;

                switch (input.ToLower())
                {
                    case "r":
                        {
                            RemoveProfile(profileName);
                            StartInteractive();
                            break;
                        }
                    case "u":
                        {
                            Cmd.WriteLine();
                            UpdateCredential(profileName);
                            StartInteractive();
                            break;
                        }
                    case "e":
                        {
                            if (ExportRoute53Data(profileName))
                            {
                                Cmd.WriteLineFixed("  Export complete on {0} at {1}.", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                                Cmd.WriteLine();
                                App.Exit();
                            }

                            App.Exit(-1);
                            return;// keep compiler happy
                        }
                    case "q":
                        {
                            App.Exit();
                            return;// keep compiler happy
                        }
                    default:
                        {
                            Cmd.WriteLine("Invalid selection\n");
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void AddProfile()
        {
            Cmd.WriteLine(Cmd.Yellow, "  r53export stores credentials using the Amazon SDK.\n");
            Cmd.WriteLine("  The AWSAccessKey and AWSSecretKey are encrypted using the");
            Cmd.WriteLine("  Windows DAPI, and stored in your user account directory.\n");
            Cmd.WriteLine(Cmd.White, "  > {0}\n\n", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AWSToolkit\\RegisteredAccounts.json"));
            Cmd.WriteLine("  == Create a new credential set ==\n");

            Cmd.Write(Cmd.White, "  AWS Access Key : ");
            var awsAccessKey = Console.ReadLine();

            Cmd.Write(Cmd.White, "  AWS Secret Key : ");
            var awsSecretKey = Console.ReadLine();

            Cmd.Write(Cmd.White, "  Profile Name   : ");
            var profileName = Console.ReadLine();

            ProfileManager.RegisterProfile(profileName, awsAccessKey, awsSecretKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        static void RemoveProfile(string profileName)
        {
            if (ProfileManager.IsProfileKnown(profileName))
            {
                ProfileManager.UnregisterProfile(profileName);   
            }
            else
            {
                throw new Exception("Invalid profile name supplied");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        static void UpdateCredential(string profileName)
        {
            Cmd.Write(Cmd.White, "  AWS Access Key: ");
            var awsAccessKey = Console.ReadLine();

            Cmd.Write(Cmd.White, "  AWS Secret Key: ");
            var awsSecretKey = Console.ReadLine();

            ProfileManager.RegisterProfile(profileName, awsAccessKey, awsSecretKey);
        }

        /// <summary>
        /// 
        /// </summary>
        static void DisplayAccessKeys()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var accountsFile = Path.Combine(directory, "AWSToolkit\\RegisteredAccounts.json");

            Cmd.WriteLine(Cmd.White, "  == Attempting to decrypt {0} for {1}\\{2} ==\n", accountsFile, Environment.UserDomainName.ToUpper(), Environment.UserName);

            if (File.Exists(accountsFile))
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(accountsFile));

                    Cmd.WriteLine(Cmd.White, "  {0,-30}{1,-30}{2,-40}", "Profile Name", "AWSAccessKey", "AWSSecretKey");
                    Cmd.WriteLine("  {0,-30}{1,-30}{2,-40}", "------------", "------------", "------------");

                    foreach (var item in json)
                    {
                        var accessKey = "";
                        var secretKey = "";
                        var displayName = ""; // item.Key;

                        foreach (var setting in item.Value)
                        {
                            if (setting.Key == "DisplayName")
                            {
                                displayName = setting.Value;
                            }

                            if (setting.Key == "AWSAccessKey" || setting.Key == "AWSSecretKey")
                            {
                                var hexData = setting.Value.HexToByteArray();
                                var decryptedBytes = ProtectedData.Unprotect(hexData, null, DataProtectionScope.CurrentUser);
                                var originalValue = Encoding.Unicode.GetString(decryptedBytes);

                                if (setting.Key == "AWSAccessKey") accessKey = originalValue;
                                if (setting.Key == "AWSSecretKey") secretKey = originalValue;
                            }
                        }

                        Cmd.WriteLine("  {0,-30}{1,-30}{2,-40}", displayName, accessKey, secretKey);
                    }
                }
                catch (Exception ex)
                {
                    Cmd.WriteLine(Cmd.Red, "  {0}.", ex.Message);
                }
            }
            else
            {
                Cmd.WriteLine(Cmd.Red, "  File does not exist or is not accessible.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        static bool ExportRoute53Data(string profileName)
        {
            try
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                var currentTime = DateTime.Now.ToString("yyyyMMdd.HHmmss");
                var fileName = string.Format("{0}-{1}.txt", assemblyName, currentTime);
                var exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                // debug
                Cmd.WriteLine("\n  Exporting to {0} ...", exportPath);

                // create r53 endpoint
                var route53Client = new AmazonRoute53Client(ProfileManager.GetAWSCredentials(profileName), RegionEndpoint.EUWest1);

                using (var writer = File.AppendText(exportPath))
                {
                    try
                    {
                        // itterate zones
                        foreach (var zone in ListHostedZones(route53Client).OrderBy(x => x.Name))
                        {
                            Log.Write(writer, "{0}", zone.Name);

                            // enum all records in zone
                            var records = ListResourceRecordSets(route53Client, zone.Id).OrderBy(x => x.Name);
                            foreach (var recordSet in records)
                            {
                                Cmd.WriteLineFixed("  Zone: {0,-30} ({1} records)", zone.Name, records.Count());

                                // todo unused fields ...
                                // recordSet.Weight, 
                                // recordSet.AliasTarget, 
                                // recordSet.Failover, 
                                // recordSet.GeoLocation, 
                                // recordSet.HealthCheckId, 
                                // recordSet.Region, 
                                // recordSet.SetIdentifier

                                Log.Write(writer, "  {0,-70} TTL={1,-6} Type={2,-8} {3}",
                                    recordSet.Name.EscapeSpecialCharacters(),
                                    recordSet.TTL,
                                    recordSet.Type,
                                    string.Join(" ", recordSet.ResourceRecords.Select(n => n.Value).ToArray()));
                            }

                            Log.Write(writer, "\n");
                        }

                        return true;
                    }
                    catch (AmazonRoute53Exception arex)
                    {
                        Cmd.WriteLine(Cmd.Red, "  {0}", arex.Message);
                        Log.Write(writer, arex.ToString());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Cmd.WriteLine(Cmd.Red, "  {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route53Client"></param>
        /// <returns></returns>
        static IEnumerable<HostedZone> ListHostedZones(IAmazonRoute53 route53Client)
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
        static IEnumerable<ResourceRecordSet> ListResourceRecordSets(IAmazonRoute53 route53Client, string hostedZoneId)
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
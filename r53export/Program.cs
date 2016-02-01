using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Amazon;
using Amazon.Route53;
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
        static void Main()
        {
            try
            {
                // collapses quoted strings separated by spaces into a single array element.
                var args = Environment.GetCommandLineArgs();

                if (args.Count() > 1)
                {
                    switch (args[1])
                    {
                        case "-l":
                            {
                                Cmd.WriteLine();
                                DisplayAwsAccessKeys();
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
                                UpdateProfileCredentials(args[2]);

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
                                DisplayHelp();
                            break;
                        }
                    }
                }
                else
                {
                    DisplayHelp();
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

            ProfileManager.RegisterProfile(
                profileName.StripUnwantedCharacters(), 
                awsAccessKey.StripUnwantedCharacters(), 
                awsSecretKey.StripUnwantedCharacters());
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
                Cmd.WriteLine("Profile \"{0}\" removed.", profileName);
            }
            else
            {
                throw new Exception(string.Format("Profile \"{0}\" not found.", profileName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        static void UpdateProfileCredentials(string profileName)
        {
            if (ProfileManager.IsProfileKnown(profileName))
            {
                Cmd.WriteLine("  Update AWS access key credentials for profile \"{0}\" ... \n", profileName);

                Cmd.Write(Cmd.White, "  Enter new AWS Access Key: ");
                var awsAccessKey = Console.ReadLine();

                Cmd.Write(Cmd.White, "  Enter new AWS Secret Key: ");
                var awsSecretKey = Console.ReadLine();

                ProfileManager.RegisterProfile(
                    profileName.StripUnwantedCharacters(),
                    awsAccessKey.StripUnwantedCharacters(),
                    awsSecretKey.StripUnwantedCharacters());
            }
            else
            {
                throw new Exception(string.Format("Profile \"{0}\" not found.", profileName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void DisplayAwsAccessKeys()
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
                    Cmd.WriteLine(Cmd.Red, "  {0}.", ex);
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
        static void DisplayHelp()
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        static void ExportRoute53Data(string profileName)
        {
            try
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                var currentTime = DateTime.Now.ToString("yyyyMMdd.HHmmss");
                var fileName = string.Format("{0}-{1}.txt", assemblyName, currentTime);
                var exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                // create Eoute53 client
                var route53Client = new AmazonRoute53Client(ProfileManager.GetAWSCredentials(profileName), RegionEndpoint.EUWest1);

                using (var writer = File.AppendText(exportPath))
                {
                    try
                    {
                        var zones = Route53.ListHostedZones(route53Client).OrderBy(x => x.Name);

                        // update user
                        Cmd.WriteLine("Exporting {0} zones from \"{1}\" to {2}\n", zones.Count(), profileName, exportPath);

                        // itterate zones
                        foreach (var zone in zones)
                        {
                            // enum all records in zone
                            var records = Route53.ListResourceRecordSets(route53Client, zone.Id).OrderBy(x => x.Name);

                            // export to file
                            Log.Write(writer, "{0}", zone.Name);
                            
                            foreach (var recordSet in records)
                            {
                                // update user
                                Cmd.WriteInPlace("Exporting zone: {0,-64} ({1} records ... {2})", zone.Name, records.Count(), recordSet.Name.EscapeSpecialCharacters());

                                // todo unused fields ...
                                // recordSet.Weight, 
                                // recordSet.AliasTarget, 
                                // recordSet.Failover, 
                                // recordSet.GeoLocation, 
                                // recordSet.HealthCheckId, 
                                // recordSet.Region, 
                                // recordSet.SetIdentifier

                                // export to file
                                Log.Write(writer, "  {0,-70} TTL={1,-6} Type={2,-8} {3}",
                                    recordSet.Name.EscapeSpecialCharacters(),
                                    recordSet.TTL,
                                    recordSet.Type,
                                    string.Join(" ", recordSet.ResourceRecords.Select(n => n.Value).ToArray()));
                            }

                            // update the user
                            Cmd.WriteInPlace("Exporting zone: {0,-64} ({1} records)", zone.Name, records.Count());
                            Cmd.WriteLine();

                            // export to file
                            Log.Write(writer, "\n");
                        }
                    }
                    catch (AmazonRoute53Exception arex)
                    {
                        Cmd.WriteLine(Cmd.Red, "{0}", arex);
                        Log.Write(writer, arex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Cmd.WriteLine(Cmd.Red, "{0}", ex);
            }
        }
    }
}
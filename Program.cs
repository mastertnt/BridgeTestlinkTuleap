using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TestLinkApi;
using XTuleap;

namespace BridgeTestlinkTuleap
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings lAppSettings = null;
            // If the setting is null, create default one.
            if (File.Exists(@".\settings.json"))
            {
                try
                {
                    string lJsonValue = File.ReadAllText(@".\settings.json");
                    lAppSettings = JsonConvert.DeserializeObject<Settings>(lJsonValue);
                }
                catch
                    (Exception lEx)
                {
                    Console.WriteLine("Cannot load settings.json" + lEx);
                    // ignored
                }
            }
            else
            {
                Console.WriteLine("Cannot find settings.json");
            }

            if (lAppSettings == null)
            {
                lAppSettings = new Settings()
                {
                    TuleapUrl = "YOUR_TULEAP_URL",
                    TuleapApiKey = "YOUR_TULEAP_KEY",
                    TuleapTrackerId = 1,
                    TuleapTimeout = 300000,
                    TestlinkUrl = "YOUR_TESTLINK_URL",
                    TestlinkApiKey = "YOUR_TESTLINK_KEY",
                    TestlinkProjecName = "MonProjet",
                };

                Formatting lIndented = Formatting.Indented;
                var lSerialized = JsonConvert.SerializeObject(lAppSettings, lIndented);
                File.WriteAllText(@".\settings.json", lSerialized);

                return;
            }

            List<ArtifactTest> lAllTests = new List<ArtifactTest>();

            // First of all, read all test cases for the given project.
            TestLink lTestlinkConnection = new TestLink(lAppSettings.TestlinkApiKey, lAppSettings.TestlinkUrl);
            var lProject = lTestlinkConnection.GetProject(lAppSettings.TestlinkProjecName);
            if (lProject != null)
            {
                Console.WriteLine($"Project found {lAppSettings.TestlinkProjecName}");
                var lTestSuite = lTestlinkConnection.GetFirstLevelTestSuitesForTestProject(lProject.id).FirstOrDefault();
                var lTestPlan = lTestlinkConnection.GetProjectTestPlans(lProject.id).FirstOrDefault();
                if (lTestSuite != null && lTestPlan != null)
                {
                    Console.WriteLine($"TestSuite found {lTestSuite.name}");
                    List<Build> lBuilds = lTestlinkConnection.GetBuildsForTestPlan(lTestPlan.id);
                    var lTestCases = lTestlinkConnection.GetTestCasesForTestSuite(lTestSuite.id, false);
                    foreach (var lTestCase in lTestCases)
                    {
                        ArtifactTest lTest = new ArtifactTest { Status = TestStatus.NotExecuted };
                        lTest.TestlinkId = lProject.prefix + "-" + lTestCase.external_id;
                        lTest.Title = lTestCase.name;
                        var lLastResult = lTestlinkConnection.GetLastExecutionResult(lTestPlan.id, lTestCase.id);
                        if (lLastResult != null)
                        {
                            Build lBuild = lBuilds.FirstOrDefault(pBuild => pBuild.id == lLastResult.build_id);
                            lTest.Build = lBuild.name;
                            Console.WriteLine($"Result {lLastResult.status} for test {lTestCase.name} : Build {lBuild.name}");
                            if (lLastResult.status == "p")
                            {
                                lTest.Status = TestStatus.Succeed;
                            }
                            else if (lLastResult.status == "b")
                            {
                                lTest.Status = TestStatus.Blocked;
                            }
                            else if (lLastResult.status == "f")
                            {
                                lTest.Status = TestStatus.Failed;
                            }
                        }
                        lAllTests.Add(lTest);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Cannot found the project {lAppSettings.TestlinkProjecName}");
            }

            Connection lConnection = new Connection(lAppSettings.TuleapUrl, lAppSettings.TuleapApiKey);
            TrackerStructure lTargetStructure = lConnection.AddTrackerStructure(lAppSettings.TuleapTrackerId);
            Tracker<Artifact> lTargetTracker = new Tracker<Artifact>(lTargetStructure);
            lTargetTracker.PreviewRequest(lConnection);
            lTargetTracker.Request(lConnection);

            foreach (var lTest in lAllTests)
            {
                Artifact lArtifact = lTargetTracker.Artifacts.FirstOrDefault(pTest => pTest.GetFieldValue<string>("id_testlink") == lTest.TestlinkId);
                if (lArtifact == null)
                {
                    Artifact lNewArtifact = new Artifact();
                    Dictionary<string, object> lValues = new Dictionary<string, object>
                    {
                        {"title", lTest.Title}, {"id_testlink", lTest.TestlinkId}, {"build", lTest.Build}
                    };
                    switch (lTest.Status)
                    {
                        case TestStatus.Blocked:
                            {
                                lValues.Add("status", "Bloqué");
                            }
                            break;
                        case TestStatus.Failed:
                            {
                                lValues.Add("status", "KO");
                            }
                            break;
                        case TestStatus.NotExecuted:
                            {
                                lValues.Add("status", "Non executé");
                            }
                            break;
                        case TestStatus.Succeed:
                            {
                                lValues.Add("status", "OK");
                            }
                            break;

                    }
                    lNewArtifact.Create(lConnection, lTargetStructure, lValues);
                    Console.WriteLine("Must create an artefact");
                }
                else
                {
                    lArtifact.CommitValue(lConnection, "title", lTest.Title);
                    lArtifact.CommitValue(lConnection, "build", lTest.Build);

                    switch (lTest.Status)
                    {
                        case TestStatus.Blocked:
                        {
                            lArtifact.CommitValue(lConnection, "status", "Bloqué");
                        }
                            break;
                        case TestStatus.Failed:
                        {
                            lArtifact.CommitValue(lConnection, "status", "KO");
                        }
                            break;
                        case TestStatus.NotExecuted:
                        {
                            lArtifact.CommitValue(lConnection, "status", "Non executé");
                        }
                            break;
                        case TestStatus.Succeed:
                        {
                            lArtifact.CommitValue(lConnection, "status", "OK");
                        }
                            break;

                    }
                }
            }
        }
    }
}

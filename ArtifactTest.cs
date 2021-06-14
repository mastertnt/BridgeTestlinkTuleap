using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeTestlinkTuleap
{
    public enum TestStatus
    {
        NotExecuted,
        Blocked,
        Succeed,
        Failed
    }

    class ArtifactTest
    {
        public TestStatus Status
        {
            get;
            set;
        }

        public string Build
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string TestlinkId
        {
            get;
            set;
        }

        public ArtifactTest()
        {
            this.Build = "";
            this.Title = "";
        }
    }
}

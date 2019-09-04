using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleServiceBusClient
{
    public class MessageDto
    {
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
        public int site { get; set; }

        public int SurveyGroup { get; set; }

        public int[] Surveys { get; set; }
    }
}

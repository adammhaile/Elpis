using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Util;

namespace PandoraSharp
{
    public struct JSONFault
    {
        public ErrorCodes Error;
        public string FaultString;
    }

    public class JSONResult : JObject
    {
        public JSONResult(string data)
        {
            JObject r = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(data);
            foreach (KeyValuePair<string, JToken> kvp in r)
            {
                this.Add(kvp.Key, kvp.Value);
            }
        }

        public JToken Result
        {
            get
            {
                return this["result"];
            }
        }

        public bool Fault
        {
            get
            {
                JToken v;
                if (!this.TryGetValue("stat", out v)) return false;
                return v.ToString() == "fail";
            }
        }

        public ErrorCodes FaultCode
        {
            get
            {
                if (!Fault)
                    return ErrorCodes.SUCCESS;
                int c = (this["code"].ToObject<int>());

                try
                {
                    return (ErrorCodes)c;
                }
                catch
                {
                    Log.O("Unknown error code: " + c);
                    return ErrorCodes.UNKNOWN_ERROR;
                }
            }
        }

        public string FaultString
        {
            get
            {
                if (!Fault) return "Operation completed successfully.";

                int c = (this["code"].ToObject<int>());

                string err = "[ERROR CODE " + c + "]";

                try
                {
                    err = Enum.GetName(typeof(Errors), c).ToString();
                }
                catch { }

                return err + " - " + Errors.GetErrorMessage(FaultCode, this["message"].ToString());
            }
        }

        public JSONFault FaultObject
        {
            get
            {
                return new JSONFault() { Error = FaultCode, FaultString = FaultString };
            }
        }

        const string UPDATE_REQUIRED = "Your client requires an update to continue listening to Pandora.";
    }
}

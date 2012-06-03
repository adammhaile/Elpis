using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Util;

namespace PandoraSharp
{
    public enum Errors
    { // _ prefix means not a specific definition
        UNKNOWN_ERROR = -2000,
        SYSTEM_ERROR = -1999,
        SUCCESS = -1,
        INTERNAL = 0,
        MAINTENANCE_MODE = 1,
        URL_PARAM_MISSING_METHOD = 2,
        URL_PARAM_MISSING_AUTH_TOKEN = 3,
        URL_PARAM_MISSING_PARTNER_ID = 4,
        URL_PARAM_MISSING_USER_ID = 5,
        SECURE_PROTOCOL_REQUIRED = 6,
        CERTIFICATE_REQUIRED = 7,
        PARAMETER_TYPE_MISMATCH = 8,
        PARAMETER_MISSING = 9,
        PARAMETER_VALUE_INVALID = 10,
        API_VERSION_NOT_SUPPORTED = 11,
        _INVALID_COUNTRY = 12,
        INSUFFICIENT_CONNECTIVITY = 13,
        _INVALID_METHOD = 14,
        _SECURE_REQUIRED = 15,
        READ_ONLY_MODE = 1000,
        INVALID_AUTH_TOKEN = 1001,
        INVALID_PARTNER_LOGIN = 1002,
        LISTENER_NOT_AUTHORIZED = 1003,
        USER_NOT_AUTHORIZED = 1004,
        _END_OF_PLAYLIST = 1005,
        STATION_DOES_NOT_EXIST = 1006,
        COMPLIMENTARY_PERIOD_ALREADY_IN_USE = 1007,
        CALL_NOT_ALLOWED = 1008,
        DEVICE_NOT_FOUND = 1009,
        PARTNER_NOT_AUTHORIZED = 1010,
        INVALID_USERNAME = 1011,
        INVALID_PASSWORD = 1012,
        USERNAME_ALREADY_EXISTS = 1013,
        DEVICE_ALREADY_ASSOCIATED_TO_ACCOUNT = 1014,
        UPGRADE_DEVICE_MODEL_INVALID = 1015,
        EXPLICIT_PIN_INCORRECT = 1018,
        EXPLICIT_PIN_MALFORMED = 1020,
        DEVICE_MODEL_INVALID = 1023,
        ZIP_CODE_INVALID = 1024,
        BIRTH_YEAR_INVALID = 1025,
        BIRTH_YEAR_TOO_YOUNG = 1026,
        INVALID_COUNTRY_CODE = 1027,
        INVALID_GENDER = 1027,
        DEVICE_DISABLED = 1034,
        DAILY_TRIAL_LIMIT_REACHED = 1035,
        INVALID_SPONSOR = 1036,
        USER_ALREADY_USED_TRIAL = 1037
    };

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

        public Errors FaultCode
        {
            get
            {
                if (!Fault)
                    return Errors.SUCCESS;
                int c = (this["code"].ToObject<int>());

                try
                {
                    return (Errors)c;
                }
                catch
                {
                    Log.O("Unknown error code: " + c);
                    return Errors.UNKNOWN_ERROR;
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

                return err + " - " + GetNiceMessage(FaultCode, this["message"].ToString());
            }
        }

        const string UPDATE_REQUIRED = "Your client requires an update to continue listening to Pandora.";

        private string GetNiceMessage(Errors FaultCode, string msg)
        {
            switch (FaultCode)
            {
                case Errors.UNKNOWN_ERROR: return "An unknown error occured.";
                case Errors.SYSTEM_ERROR: return "An internal client error occured.";
                case Errors.SUCCESS: return "Operation completed successfully.";
                case Errors.INTERNAL: return "An internal error occured.";
                case Errors.MAINTENANCE_MODE: return "Pandora is currently conducting maintenance. Please try again later.";
                case Errors.URL_PARAM_MISSING_METHOD: return UPDATE_REQUIRED;
                case Errors.URL_PARAM_MISSING_AUTH_TOKEN: return UPDATE_REQUIRED;
                case Errors.URL_PARAM_MISSING_PARTNER_ID: return UPDATE_REQUIRED;
                case Errors.URL_PARAM_MISSING_USER_ID: return UPDATE_REQUIRED;
                case Errors.SECURE_PROTOCOL_REQUIRED: return UPDATE_REQUIRED;
                case Errors.CERTIFICATE_REQUIRED: return UPDATE_REQUIRED;
                case Errors.PARAMETER_TYPE_MISMATCH: return UPDATE_REQUIRED;
                case Errors.PARAMETER_MISSING: return UPDATE_REQUIRED;
                case Errors.PARAMETER_VALUE_INVALID: return UPDATE_REQUIRED;
                case Errors.API_VERSION_NOT_SUPPORTED: return UPDATE_REQUIRED;
                case Errors._INVALID_COUNTRY: return "The country you are connecting from is not allowed to access Pandora.";
                case Errors.INSUFFICIENT_CONNECTIVITY: return "INSUFFICIENT_CONNECTIVITY. Possibly invalid sync time. Try logging in again, and check for client updates.";
                case Errors._INVALID_METHOD: return "An attempt was made to use an invalid method call. " + UPDATE_REQUIRED;
                case Errors._SECURE_REQUIRED: return "SSL required for last RPC. " + UPDATE_REQUIRED;
                case Errors.READ_ONLY_MODE: return "Pandora is currently conducting maintenance. Please try again later."; ;
                case Errors.INVALID_AUTH_TOKEN: return "Auth token is invalid/expired.";
                case Errors.INVALID_PARTNER_LOGIN: return "Partner or user login is invalid.";
                case Errors.LISTENER_NOT_AUTHORIZED: return "Your subscription has lapsed. Please visit www.pandora.com to confirm account status.";
                case Errors.USER_NOT_AUTHORIZED: return "This user may not perform that action.";
                case Errors._END_OF_PLAYLIST: return "End of playlist detected. Skip limit exceeded.";
                case Errors.STATION_DOES_NOT_EXIST: return "Station does not exist.";
                case Errors.COMPLIMENTARY_PERIOD_ALREADY_IN_USE: return "Pandora One Trial is currently active.";
                case Errors.CALL_NOT_ALLOWED: return "Permission denied to use this call.";
                case Errors.DEVICE_NOT_FOUND: return "Device not found.";
                case Errors.PARTNER_NOT_AUTHORIZED: return "partnerLogin is invalid. " + UPDATE_REQUIRED;
                case Errors.INVALID_USERNAME: return "Specified username is not valid.";
                case Errors.INVALID_PASSWORD: return "Specified password is not valid.";
                case Errors.USERNAME_ALREADY_EXISTS: return "This username is already in use.";
                case Errors.DEVICE_ALREADY_ASSOCIATED_TO_ACCOUNT: return "Device already associated.";
                case Errors.UPGRADE_DEVICE_MODEL_INVALID: return "This client is out of date.";
                case Errors.EXPLICIT_PIN_INCORRECT: return "Explicit PIN is incorrect.";
                case Errors.EXPLICIT_PIN_MALFORMED: return "Explicit PIN is invalid.";
                case Errors.DEVICE_MODEL_INVALID: return "Device model is not valid. Client out of date.";
                case Errors.ZIP_CODE_INVALID: return "ZIP code is not valid.";
                case Errors.BIRTH_YEAR_INVALID: return "Birth year is not valid.";
                case Errors.BIRTH_YEAR_TOO_YOUNG: return "You must be 13 or older to use the Pandora service.";
                //case Errors.INVALID_COUNTRY_CODE: return "Country code invalid.";
                case Errors.INVALID_GENDER: return "Invalid gender: 'male' or 'female' expected.";
                case Errors.DEVICE_DISABLED: return "Device disabled.";
                case Errors.DAILY_TRIAL_LIMIT_REACHED: return "You may not activate any more trials.";
                case Errors.INVALID_SPONSOR: return "Invalid sponsor.";
                case Errors.USER_ALREADY_USED_TRIAL: return "You have already used your Pandora One trial.";
                default: return msg;
            }
        }
    }
}

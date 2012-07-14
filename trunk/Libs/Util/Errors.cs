using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
    public enum ErrorCodes
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
        USER_ALREADY_USED_TRIAL = 1037,

        NO_AUDIO_URLS = 5000,

        //Elpis Specific
        ERROR_RPC = 6000,
        CONFIG_LOAD_ERROR = 6001,
        LOG_SETUP_ERROR = 6002,
        ENGINE_INIT_ERROR = 6003,
        STREAM_ERROR = 6004,

        //LastFM Errors
        ERROR_GETTING_SESSION = 7000,
        ERROR_GETTING_TOKEN = 7001,
    };

    public class Errors
    {
        static readonly string UPDATE_REQUIRED = "Elpis requires an update.\r\nPlease check http://adamhaile.net for details.";

        public static string GetErrorMessage(ErrorCodes FaultCode, string msg = "Unknown Error")
        {
            switch (FaultCode)
            {
                case ErrorCodes.UNKNOWN_ERROR: return "An unknown error occured.";
                case ErrorCodes.SYSTEM_ERROR: return "An internal client error occured.";
                case ErrorCodes.SUCCESS: return "Operation completed successfully.";
                case ErrorCodes.INTERNAL: return "An internal error occured.";
                case ErrorCodes.MAINTENANCE_MODE: return "Pandora is currently conducting maintenance. Please try again later.";
                case ErrorCodes.URL_PARAM_MISSING_METHOD: return UPDATE_REQUIRED;
                case ErrorCodes.URL_PARAM_MISSING_AUTH_TOKEN: return UPDATE_REQUIRED;
                case ErrorCodes.URL_PARAM_MISSING_PARTNER_ID: return UPDATE_REQUIRED;
                case ErrorCodes.URL_PARAM_MISSING_USER_ID: return UPDATE_REQUIRED;
                case ErrorCodes.SECURE_PROTOCOL_REQUIRED: return UPDATE_REQUIRED;
                case ErrorCodes.CERTIFICATE_REQUIRED: return UPDATE_REQUIRED;
                case ErrorCodes.PARAMETER_TYPE_MISMATCH: return UPDATE_REQUIRED;
                case ErrorCodes.PARAMETER_MISSING: return UPDATE_REQUIRED;
                case ErrorCodes.PARAMETER_VALUE_INVALID: return UPDATE_REQUIRED;
                case ErrorCodes.API_VERSION_NOT_SUPPORTED: return UPDATE_REQUIRED;
                case ErrorCodes._INVALID_COUNTRY: return "The country you are connecting from is not allowed to access Pandora.";
                case ErrorCodes.INSUFFICIENT_CONNECTIVITY: return "INSUFFICIENT_CONNECTIVITY. Possibly invalid sync time. Try logging in again, and check for client updates.";
                case ErrorCodes._INVALID_METHOD: return "Incorrect HTTP/S method used for last RPC. " + UPDATE_REQUIRED;
                case ErrorCodes._SECURE_REQUIRED: return "SSL required for last RPC. " + UPDATE_REQUIRED;
                case ErrorCodes.READ_ONLY_MODE: return "Pandora is currently conducting maintenance. Please try again later."; ;
                case ErrorCodes.INVALID_AUTH_TOKEN: return "Auth token is invalid/expired.";
                case ErrorCodes.INVALID_PARTNER_LOGIN: return "Partner or user login is invalid.";
                case ErrorCodes.LISTENER_NOT_AUTHORIZED: return "Your subscription has lapsed. Please visit www.pandora.com to confirm account status.";
                case ErrorCodes.USER_NOT_AUTHORIZED: return "This user may not perform that action.";
                case ErrorCodes._END_OF_PLAYLIST: return "End of playlist detected. Skip limit exceeded.";
                case ErrorCodes.STATION_DOES_NOT_EXIST: return "Station does not exist.";
                case ErrorCodes.COMPLIMENTARY_PERIOD_ALREADY_IN_USE: return "Pandora One Trial is currently active.";
                case ErrorCodes.CALL_NOT_ALLOWED: return "Permission denied to use this call.";
                case ErrorCodes.DEVICE_NOT_FOUND: return "Device not found.";
                case ErrorCodes.PARTNER_NOT_AUTHORIZED: return "partnerLogin is invalid. " + UPDATE_REQUIRED;
                case ErrorCodes.INVALID_USERNAME: return "Specified username is not valid.";
                case ErrorCodes.INVALID_PASSWORD: return "Specified password is not valid.";
                case ErrorCodes.USERNAME_ALREADY_EXISTS: return "This username is already in use.";
                case ErrorCodes.DEVICE_ALREADY_ASSOCIATED_TO_ACCOUNT: return "Device already associated.";
                case ErrorCodes.UPGRADE_DEVICE_MODEL_INVALID: return "This client is out of date.";
                case ErrorCodes.EXPLICIT_PIN_INCORRECT: return "Explicit PIN is incorrect.";
                case ErrorCodes.EXPLICIT_PIN_MALFORMED: return "Explicit PIN is invalid.";
                case ErrorCodes.DEVICE_MODEL_INVALID: return "Device model is not valid. Client out of date.";
                case ErrorCodes.ZIP_CODE_INVALID: return "ZIP code is not valid.";
                case ErrorCodes.BIRTH_YEAR_INVALID: return "Birth year is not valid.";
                case ErrorCodes.BIRTH_YEAR_TOO_YOUNG: return "You must be 13 or older to use the Pandora service.";
                //case ErrorCodes.INVALID_COUNTRY_CODE: return "Country code invalid.";
                case ErrorCodes.INVALID_GENDER: return "Invalid gender: 'male' or 'female' expected.";
                case ErrorCodes.DEVICE_DISABLED: return "Device disabled.";
                case ErrorCodes.DAILY_TRIAL_LIMIT_REACHED: return "You may not activate any more trials.";
                case ErrorCodes.INVALID_SPONSOR: return "Invalid sponsor.";
                case ErrorCodes.USER_ALREADY_USED_TRIAL: return "You have already used your Pandora One trial.";

                case ErrorCodes.NO_AUDIO_URLS: return "No Audio URLs returned for this track.";

                case ErrorCodes.ERROR_RPC: return "Error communicating with the server. \r\nTry again, check your connection or try restarting.";
                case ErrorCodes.CONFIG_LOAD_ERROR: return @"Error loading Elpis configuration. Try navigating to %AppData%\Elpis\ and deleting ""elpis.config""";
                case ErrorCodes.LOG_SETUP_ERROR: return "Error setting up logging.";
                case ErrorCodes.ENGINE_INIT_ERROR: return "Error initializing the player engine, Elpis must close. Try restarting the application.";
                case ErrorCodes.STREAM_ERROR: return "Failed to load song more than once.\r\nCheck connection and try again.";

                case ErrorCodes.ERROR_GETTING_SESSION: return "Error retrieving Last.FM Session.";
                case ErrorCodes.ERROR_GETTING_TOKEN: return "Error retrieving Last.FM Auth Token.";

                default: return msg;
            }
        }
    
        public static bool IsHardFail(ErrorCodes FaultCode)
        {
            switch(FaultCode)
            {
                case ErrorCodes.URL_PARAM_MISSING_METHOD:
                case ErrorCodes.URL_PARAM_MISSING_AUTH_TOKEN:
                case ErrorCodes.URL_PARAM_MISSING_PARTNER_ID:
                case ErrorCodes.URL_PARAM_MISSING_USER_ID:
                case ErrorCodes.SECURE_PROTOCOL_REQUIRED:
                case ErrorCodes.CERTIFICATE_REQUIRED:
                case ErrorCodes.PARAMETER_TYPE_MISMATCH:
                case ErrorCodes.PARAMETER_MISSING:
                case ErrorCodes.PARAMETER_VALUE_INVALID:
                case ErrorCodes.API_VERSION_NOT_SUPPORTED:
                case ErrorCodes.PARTNER_NOT_AUTHORIZED:
                case ErrorCodes.CONFIG_LOAD_ERROR:
                case ErrorCodes.LOG_SETUP_ERROR:
                case ErrorCodes.ENGINE_INIT_ERROR:
                    return true;
                default:
                    return false;
            }
        }
    }
}

/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of Elpis.
 * Elpis is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * Elpis is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with Elpis. If not, see http://www.gnu.org/licenses/.
*/

using System.Collections.Generic;

namespace Elpis
{
    public class ErrorCode
    {
        public string Description { get; set; }
        public bool HardFail { get; set; }
    }

    public class Errors
    {
        public static ErrorCode UnknownError = new ErrorCode { Description = "Looks like there was a problem talking to Pandora.\r\nTry again or restart Elpis.", HardFail = false };

        public static Dictionary<string, ErrorCode> ErrorCodes =
            new Dictionary<string, ErrorCode>
                {
                    {
                        "INCOMPATIBLE_VERSION",
                        new ErrorCode
                            {
                                Description =
                                    "Oh no! Looks like Pandora changed the API. Check the website for updates.",
                                HardFail = true
                            }
                        },
                    {
                        "AUTH_INVALID_USERNAME_PASSWORD",
                        new ErrorCode
                            {
                                Description = "Invalid Email or Password!",
                                HardFail = false
                            }
                        },
                    {
                        "OUT_OF_SYNC",
                        new ErrorCode
                            {
                                Description =
                                    "Unable to sync with Pandora. Please make sure your system time is correct.",
                                HardFail = false
                            }
                        },
                    {
                        "ERROR_RPC",
                        new ErrorCode
                            {
                                Description =
                                    "Error communicating with the server. The issue may be temporary. \r\nTry again, check your connection or try restarting.",
                                HardFail = false
                            }
                        },
                    {
                        "LISTENER_NOT_AUTHORIZED",
                        new ErrorCode
                            {
                                Description =
                                    "User action is not authorized. You are probably trying to modify a station you do not own.",
                                HardFail = false
                            }
                        },
                    {
                        "READONLY_MODE",
                        new ErrorCode
                            {
                                Description = "Request cannot be completed now. Please try again later.",
                                HardFail = false
                            }
                        },
                    {
                        "STATION_CODE_INVALID",
                        new ErrorCode
                            {
                                Description = "Station ID is invalid.",
                                HardFail = false
                            }
                        },
                    {
                        "STATION_DOES_NOT_EXIST",
                        new ErrorCode
                            {
                                Description = "Station does not exist.",
                                HardFail = false
                            }
                        },
                    {
                        "PLAYLIST_END",
                        new ErrorCode
                            {
                                Description = "You've listened to all the music in this station. Please try another.",
                                HardFail = false
                            }
                        },
                    {
                        "PLAYLIST_EMPTY",
                        new ErrorCode
                            {
                                Description = "Unable to load more songs.",
                                HardFail = false
                            }
                        },
                    {
                        "STREAM_ERROR",
                        new ErrorCode
                            {
                                Description = "Failed to load song more than once.\r\nCheck connection and try again.",
                                HardFail = false
                            }
                        },
                    {
                        "QUICKMIX_NOT_PLAYABLE",
                        new ErrorCode
                            {
                                Description = "Quickmix not playable.",
                                HardFail = false
                            }
                        },
                    {
                        "REMOVING_TOO_MANY_SEEDS",
                        new ErrorCode
                            {
                                Description = "Last seed cannot be removed.",
                                HardFail = false
                            }
                        },
                    {
                        "CONFIG_LOAD_ERROR",
                        new ErrorCode
                            {
                                Description =
                                    @"Error loading Elpis configuration. Try navigating to %AppData%\Elpis\ and deleting ""elpis.config""",
                                HardFail = true
                            }
                        },
                    {
                        "LOG_SETUP_ERROR",
                        new ErrorCode
                            {
                                Description = "Error setting up logging.",
                                HardFail = true
                            }
                        },
                    {
                        "ENGINE_INIT_ERROR",
                        new ErrorCode
                            {
                                Description = "Error initializing the player engine, Elpis must close. Try restarting the application.",
                                HardFail = true
                            }
                        },
                };

        public static ErrorCode GetError(string code)
        {
            if (ErrorCodes.ContainsKey(code))
                return ErrorCodes[code];

            return UnknownError;
        }
    }
}
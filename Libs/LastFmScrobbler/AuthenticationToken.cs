using System;

namespace Lpfm.LastFmScrobbler
{
    /// <summary>
    /// A Last.fm Authentication Token DTO
    /// </summary>
    public class AuthenticationToken
    {
        /// <summary>
        /// The number of seconds a token is valid for
        /// </summary>
        public const int ValidForMinutes = 60;

        /// <summary>
        /// Instantiates an <see cref="AuthenticationToken"/>
        /// </summary>
        /// <param name="value"></param>
        public AuthenticationToken(string value)
        {
            Created = DateTime.Now;
            Value = value;
        }

        /// <summary>
        /// The token string value 
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// When the token was created
        /// </summary>
        public DateTime Created { get; protected set; }

        /// <summary>
        /// True when this token is still valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return Created > DateTime.Now.AddMinutes(-ValidForMinutes);
        }
    }
}
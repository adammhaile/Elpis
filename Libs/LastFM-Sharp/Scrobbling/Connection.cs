//  
//  Copyright (C) 2009 Amr Hassan
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;

namespace Lastfm.Scrobbling
{
	/// <summary>
	/// A connection to the Last.fm scrobbling service. Can be used individually for scrobbling
	/// or through a <see cref="ScrobbleManager"/> object.
	/// </summary>
	public class Connection
	{
		public string ClientID {get; private set;}
		
		private string SessionID {get; set;}
		private Uri SubmissionURL {get; set;}
		private Uri NowplayingURL {get; set;}
		
		private RequestParameters handshakeParameters;
		private bool firstHandshakeDone {get; set;}
		
		public Connection(string clientID, string clientVersion, string username, 
		                  Session authenticatedSession)
		{
			RequestParameters p = new RequestParameters();
			p["hs"] = "true";
			p["p"] = "1.2.1";
			p["c"] = clientID;
			p["v"] = clientVersion;
			p["u"] = username;
			string timestamp = Utilities.DateTimeToUTCTimestamp(DateTime.Now).ToString();			
			p["t"] = timestamp;
			p["api_key"] = authenticatedSession.APIKey;
			p["sk"] = authenticatedSession.SessionKey;
			p["a"] = Utilities.MD5(authenticatedSession.APISecret + timestamp);
			
			this.handshakeParameters = p;
			
			ClientID = clientID;
		}
		
		public Connection(string clientID, string clientVersion, string username, string md5Password)
		{
			RequestParameters p = new RequestParameters();
			p["hs"] = "true";
			p["p"] = "1.2.1";
			p["c"] = clientID;
			p["v"] = clientVersion;
			p["u"] = username;
			string timestamp = Utilities.DateTimeToUTCTimestamp(DateTime.Now).ToString();
			p["t"] = timestamp;
			p["a"] = Utilities.MD5(md5Password + timestamp);
			
			this.handshakeParameters = p;
			
			ClientID = clientID;
		}

		/// <summary>
		/// A handshake is performed at the beginning to retrieve the 
		/// SessionID, SubmissionURL and NowplayingURL and also everytime the server
		/// reports that the SessionID is invalid.
		/// </summary>
		private void doHandshake()
		{
			Request request = new Request(new Uri("http://post.audioscrobbler.com:80/"), handshakeParameters);
			
			string[] response = request.execute().Split('\n');
			
			SessionID = response[1];
			NowplayingURL = new Uri(response[2]);
			SubmissionURL = new Uri(response[3]);
		}
		
		/// <summary>
		/// Performs the initial handshake. Can be ignored, a handshake
		/// will be performed whenever necessary.
		/// </summary>
		public void Initialize()
		{
			if (!firstHandshakeDone)
			{
				doHandshake();
				firstHandshakeDone = true;
			}
		}
		
		/// <summary>
		/// Send the now playing notification.
		/// </summary>
		/// <param name="track">
		/// A <see cref="NowplayingTrack"/>
		/// </param>
		public void ReportNowplaying(NowplayingTrack track)
		{
			Initialize();
			
			RequestParameters p = new RequestParameters();
			p["s"] = SessionID;
			p["a"] = track.Artist;
			p["t"] = track.Title;
			p["b"] = track.Album;

			if (track.Duration.TotalSeconds == 0)
				p["l"] = "";
			else
				p["l"] = track.Duration.TotalSeconds.ToString();
			
			if (track.Number == 0)
				p["n"] = "";
			else
				p["n"] = track.Number.ToString();
			
			p["m"] = track.MBID;

			Request request = new Request(this.NowplayingURL, p);

			// A BadSessionException occurs when another client has made a handshake
			// with this user's credentials, should redo a handshake and pass this 
			// exception quietly.
			try
			{
				request.execute();
			} catch (BadSessionException) {
				this.doHandshake();
				this.ReportNowplaying(track);
			}
		}
		
		/// <summary>
		/// Public scrobble function. Scrobbles a PlayedTrack object.
		/// </summary>
		/// <param name="track">
		/// A <see cref="PlayedTrack"/>
		/// </param>
		public void Scrobble(Entry track)
		{
			RequestParameters np = new RequestParameters();
			RequestParameters p = track.getParameters();
			
			foreach(string key in p.Keys)
				np[key + "[0]"] = p[key];
			
			// This scrobbles the collection of parameters no matter what they belong to.
			this.Scrobble(np);
		}
		
		/// <summary>
		/// The internal scrobble function, scrobbles pure request parameters.
		/// Could be for more than one track, as specified by Last.fm, but they recommend that
		/// only one track should be submitted at a time.
		/// </summary>
		/// <param name="parameters">
		/// A <see cref="RequestParameters"/>
		/// </param>
		internal void Scrobble(RequestParameters parameters)
		{
			Initialize();
			
			parameters["s"] = SessionID;			
			Request request = new Request(this.SubmissionURL, parameters);

			// A BadSessionException occurs when another client has made a handshake
			// with this user's credentials, should redo a handshake and pass this 
			// exception quietly.
			try
			{
				request.execute();
			} catch (BadSessionException) {
				this.doHandshake();
				this.Scrobble(parameters);
			}
		}			
	}
}

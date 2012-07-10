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
using System.Xml;
using System.Collections.Generic;

namespace Lastfm.Radio
{
	
	/// <summary>
	/// A Last.fm radio station.
	/// </summary>
	public class Station : Services.Base
	{
		//// <value>
		/// The unique station path.
		/// </value>
		public StationURI URI {get; private set;}
		
		//// <value>
		/// Station title. Like "Cher Similar Artists".
		/// </value>
		public string Title {get; private set;}
		
		private bool tunedIn;
		
		public Station(StationURI uri, Session session)
			:base(session)
		{
			URI = uri;
		}
		
		public Station(string uri, Session session)
			:base(session)
		{
			URI = new StationURI(uri);
		}
		
		private void tuneIn()
		{
			RequestParameters p = new RequestParameters();
			p["station"] = URI.ToString();
			
			XmlDocument doc = (new Services.Request("radio.tune", Session, p)).execute();
			Title = extract(doc, "name");
			
			this.tunedIn = true;
		}
		
		/// <summary>
		/// Fetches new radio content periodically.
		/// </summary>
		/// <param name="discoveryMode">
		/// A <see cref="System.Boolean"/>
		/// Whether to request last.fm content with discovery mode switched on.
		/// </param>
		/// <param name="isScrobbling">
		/// A <see cref="System.Boolean"/>
		/// Whether the user is scrobbling or not during this radio session (helps content generation).
		/// </param>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] FetchTracks(bool discoveryMode, bool isScrobbling)
		{
			// tuneIn if necessary
			if (!tunedIn)
				tuneIn();
			
			// Fetch tracks
			RequestParameters p = new RequestParameters();
			if (discoveryMode)
				p["discovery"] = "true";
			if (isScrobbling)
				p["rtp"] = "true";
			
			XmlDocument doc = (new Services.Request("radio.getPlaylist", Session, p)).execute();
			
			List<Track> list = new List<Track>();
			foreach(XmlNode node in doc.GetElementsByTagName("track"))
			{
				Track track = new Track(extract(node, "creator"),
				                        extract(node, "title"),
				                        extract(node, "album"),
				                        extract(node, "location"),
				                        extract(node, "identifier"),
				                        extract(node, "image"),
				                        new TimeSpan(0, 0, 0, 0, Int32.Parse(extract(node, "duration"))));
				
				list.Add(track);
			}
			
			return list.ToArray();
		}
		
		internal override RequestParameters getParams ()
		{
			throw new System.NotImplementedException ();
		}

	}
}

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

namespace Lastfm.Services
{
	/// <summary>
	/// A Last.fm XSPF playlist.
	/// </summary>
	public class XSPF : Base
	{
		/// <value>
		/// The Last.fm playlist url.
		/// </value>
		/// <summary>Playlist URL</summary>
		
		public string PlaylistUrl {get; private set; }
		
		public XSPF(string playlistUrl, Session session)
			:base(session)
		{
			PlaylistUrl = playlistUrl;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["playlistURL"] = PlaylistUrl;
			
			return p;
		}
		
		/// <summary>
		/// Returns the tracks on this XSPF playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] GetTracks()
		{
			XmlDocument doc = request("playlist.fetch");
			
			List<Track> list = new List<Track>();
			foreach(XmlNode n in doc.GetElementsByTagName("track"))
				list.Add(new Track(extract(n, "creator"), extract(n, "title"), Session));
			
			return list.ToArray();
		}
	}
}

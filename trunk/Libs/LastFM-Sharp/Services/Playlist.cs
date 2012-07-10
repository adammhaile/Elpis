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
using System.Collections.Generic;
using System.Xml;

namespace Lastfm.Services
{
	/// <summary>
	/// A Last.fm user playlist.
	/// </summary>
	public class Playlist : Base, IHasImage, System.IEquatable<Playlist>, IHasURL
	{	
		/// <summary>
		/// The playlist ID. A Unique identifier.
		/// </summary>
		public int ID {get; private set;}
		
		/// <summary>
		/// The user who owns the playlist. 
		/// </summary>
		public User User {get; private set;}
		
		public Playlist(string username, int id, Session session)
			:base(session)
		{
			ID = id;
			User = new User(username, session);
		}
		
		public Playlist(User user, int id, Session session)
			:base(session)
		{
			ID = id;
			User = user;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["id"] = ID.ToString();
			
			return p;
		}
		
		/// <summary>
		/// Returns the tracks in this playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] GetTracks()
		{
			string url = "lastfm://playlist/" + ID.ToString();
			
			return (new XSPF(url, Session)).GetTracks();
		}
		
		/// <summary>
		/// Adds a track to this playlist.
		/// </summary>
		/// <param name="track">
		/// A <see cref="Track"/>
		/// </param>
		public void AddTrack(Track track)
		{
			requireAuthentication();
			
			RequestParameters p = getParams();
			p["track"] = track.Title;
			p["artist"] = track.Artist.Name;
			
			request("playlist.addTrack", p);
		}
		
		private XmlNode getNode()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["user"] = User.Name;
			
			XmlDocument doc = request("user.getPlaylists", p);
			foreach(XmlNode node in doc.GetElementsByTagName("playlist"))
			{
				if (Int32.Parse(extract(node, "id")) == ID)
					return node;
			}
			
			return null;			
		}
		
		/// <summary>
		/// Returns the title of this playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetTitle()
		{
			return extract(getNode(), "title");
		}
		
		/// <summary>
		/// Returns the description of this playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetDescription()
		{
			return extract(getNode(), "description");
		}
		
		/// <summary>
		/// Returns the date of creation of this playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="DateTime"/>
		/// </returns>
		public DateTime GetCreationDate()
		{
			return DateTime.Parse(extract(getNode(), "date"));
		}
		
		/// <summary>
		/// Returns the number of tracks on this playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetSize()
		{
			return Int32.Parse(extract(getNode(), "size"));
		}
		
		/// <summary>
		/// Returns the total duration of the playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="TimeSpan"/>
		/// </returns>
		public TimeSpan GetDuration()
		{
			int duration = Int32.Parse(extract(getNode(), "duration"));
			
			// duration is in seconds, i guess..
			return new TimeSpan(0, 0, duration);
		}
		
		/// <summary>
		/// Returns the url to the image of the playlist.
		/// </summary>
		/// <param name="size">
		/// A <see cref="ImageSize"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL(ImageSize size)
		{
			return extractAll(getNode(), "image")[(int)size];
		}
		
		/// <summary>
		/// Returns the url to the image of the playlist.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL()
		{
			return GetImageURL(ImageSize.Large);
		}
		
		public bool Equals(Playlist playlist)
		{
			return (this.ID == playlist.ID);
		}
		
		/// <summary>
		/// Creates a new playlist for the authenticated user.
		/// </summary>
		/// <param name="title">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="description">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="Playlist"/>
		/// </returns>
		public static Playlist CreateNew(string title, string description, Session session)
		{
			//manually test session for authentication.
			if(!session.Authenticated)
				throw new AuthenticationRequiredException();
			
			
			RequestParameters p = new Lastfm.RequestParameters();
			p["title"] = title;
			p["description"] = description;
			
			XmlDocument doc = (new Request("playlist.create", session, p)).execute();
			int id = Int32.Parse(doc.GetElementsByTagName("id")[0].InnerText);
			
			return new Playlist(AuthenticatedUser.GetUser(session), id, session);
		}
		
		/// <summary>
		/// Returns the Last.fm page of this object.
		/// </summary>
		/// <param name="language">
		/// A <see cref="SiteLanguage"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetURL(SiteLanguage language)
		{
			string url = ((XmlElement)getNode()).GetElementsByTagName("url")[0].InnerText;
			
			string domain = getSiteDomain(language);
			
			url = url.Substring(url.IndexOf("user/"));
			
			return "http://" + domain + "/" + url;
		}

		/// <summary>
		/// The object's Last.fm page url.
		/// </summary>
		public string URL
		{ get { return GetURL(SiteLanguage.English); } }
	}
}

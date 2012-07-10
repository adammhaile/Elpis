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
	/// Compare Last.fm users and others.
	/// </summary>
	public class Tasteometer : Base
	{
		private string firstType {get; set;}
		private string firstValue {get; set;}
		private string secondType {get; set;}
		private string secondValue {get; set;}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["type1"] = firstType;
			p["type2"] = secondType;
			p["value1"] = firstValue;
			p["value2"] = secondValue;
			
			return p;
		}

		/// <summary>
		/// Compare a user with another user.
		/// </summary>
		/// <param name="firstUser">
		/// A <see cref="User"/>
		/// </param>
		/// <param name="secondUser">
		/// A <see cref="User"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Tasteometer(User firstUser, User secondUser, Session session)
			:base(session)
		{
			firstType = "user";
			secondType = "user";
			
			firstValue = firstUser.Name;
			secondValue = secondUser.Name;			
		}
		
		/// <summary>
		/// Compare a list of artists with another list of artists.
		/// </summary>
		/// <param name="firstArtists">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		/// <param name="secondArtists">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Tasteometer(IEnumerable<Artist> firstArtists, IEnumerable<Artist> secondArtists, Session session)
			:base(session)
		{
			firstType = "artists";
			secondType = "artists";
			
			foreach(Artist artist in firstArtists)
				firstValue += "," + artist.Name;
			foreach(Artist artist in secondArtists)
				secondValue += "," + artist.Name;
		}
		
		/// <summary>
		/// Compare a myspace profile with another.
		/// </summary>
		/// <param name="firstMyspaceURL">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="secondMyspaceURL">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Tasteometer(string firstMyspaceURL, string secondMyspaceURL, Session session)
			:base(session)
		{
			firstType = "myspace";
			secondType = "myspace";
			
			firstValue = firstMyspaceURL;
			secondValue = secondMyspaceURL;
		}
		
		/// <summary>
		/// Compare a user with a list of artists.
		/// </summary>
		/// <param name="user">
		/// A <see cref="User"/>
		/// </param>
		/// <param name="artists">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Tasteometer(User user, IEnumerable<Artist> artists, Session session)
			:base(session)
		{
			firstType = "user";
			secondType = "artists";
			
			firstValue = user.Name;
			foreach(Artist artist in artists)
				secondValue += "," + artist.Name;
		}
		
		/// <summary>
		/// Compare a user with a myspace profile.
		/// </summary>
		/// <param name="user">
		/// A <see cref="User"/>
		/// </param>
		/// <param name="myspaceURL">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Tasteometer(User user, string myspaceURL, Session session)
			:base(session)
		{
			firstType = "user";
			secondType = "myspace";
			
			firstValue = user.Name;
			secondValue = myspaceURL;
		}
		
		/// <summary>
		/// Compare a list of artists with a myspace profile.
		/// </summary>
		/// <param name="artists">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		/// <param name="myspaceURL">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Tasteometer(IEnumerable<Artist> artists, string myspaceURL, Session session)
			:base(session)
		{
			firstType = "artists";
			secondType = "myspace";
			
			foreach(Artist artist in artists)
				firstValue += "," + artist.Name;
			secondValue = myspaceURL;
		}
		
		/// <summary>
		/// Returns the comparison percentage.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public float GetScore()
		{
			XmlDocument doc = request("tasteometer.compare");
			
			return float.Parse(extract(doc, "score"));
		}
		
		/// <summary>
		/// Returns the shared artits.
		/// </summary>
		/// <returns>
		/// A <see cref="Artist"/>
		/// </returns>
		public Artist[] GetSharedArtists()
		{
			// default limit value
			return GetSharedArtists(5);
		}
		
		/// <summary>
		/// Returns the shared artists.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="Artist"/>
		/// </returns>
		public Artist[] GetSharedArtists(int limit)
		{
			RequestParameters p = getParams();
			p["limit"] = limit.ToString();
			
			XmlDocument doc = request("tasteometer.compare", p);
			
			List<Artist> list = new List<Artist>();
			
			foreach(XmlNode node in doc.GetElementsByTagName("artist"))
				list.Add(new Artist(extract(node, "name"), Session));
			
			return list.ToArray();
		}
	}
}

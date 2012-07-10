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
	/// The authenticated user.
	/// </summary>
	/// <remarks>
	/// To create an an object of this class, you'd have to call
	/// <see cref="AuthenticatedUser.GetUser"/>.
	/// </remarks>
	public class AuthenticatedUser : User, IHasImage
	{
		private AuthenticatedUser(string username, Session session)
			:base(username, session)
		{
		}
		
		/// <summary>
		/// Returns the authenticated user of this session.
		/// </summary>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="AuthenticatedUser"/>
		/// </returns>
		public static AuthenticatedUser GetUser(Session session)
		{
			//check for authentication manually
			if(!session.Authenticated)
				throw new AuthenticationRequiredException();
			
			XmlDocument doc = (new Request("user.getInfo", session, new RequestParameters())).execute();
			
			string name = doc.GetElementsByTagName("name")[0].InnerText;
			
			return new AuthenticatedUser(name, session);
		}
		
		/// <summary>
		/// Returns the url to the authenticated user's avatar.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL()
		{
			XmlDocument doc = request("user.getInfo");
			
			return extract(doc, "image");
		}
		
		/// <summary>
		/// Returns the ISO 639 alpha-2 language code of this user.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetLanguageCode()
		{
			XmlDocument doc = request("user.getInfo");
			
			return extract(doc, "lang");
		}
		
		/// <summary>
		/// Returns the user's country.
		/// </summary>
		/// <returns>
		/// A <see cref="Country"/>
		/// </returns>
		public Country GetCountry()
		{
			XmlDocument doc = request("user.getInfo");
			
			return new Country(extract(doc, "country"), Session);
		}
		
		/// <summary>
		/// Returns the authenticated user's age.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetAge()
		{
			XmlDocument doc = request("user.getInfo");
			
			return Int32.Parse(extract(doc, "age"));
		}
		
		/// <summary>
		/// Returns the authenticated user's gender.
		/// </summary>
		/// <returns>
		/// A <see cref="Gender"/>
		/// </returns>
		public Gender GetGender()
		{
			XmlDocument doc = request("user.getInfo");
			
			string g = extract(doc, "gender");
			
			if (g=="m")
				return Gender.Male;
			else if (g=="f")
				return Gender.Female;
			else
				return Gender.Unspecified;
		}
		
		/// <summary>
		/// Returns true if the user is a subscriber, false if not.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsSubscriber()
		{
			XmlDocument doc = request("user.getInfo");
			
			return (extract(doc, "subscriber") == "1");
		}
		
		/// <summary>
		/// Returns the user's playcount.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetPlaycount()
		{
			XmlDocument doc = request("user.getInfo");
			
			return Int32.Parse(extract(doc, "playcount"));
		}
		
		/// <summary>
		/// Returns the recommended events by Last.fm for this user.
		/// </summary>
		public RecommendedEvents RecommendedEvents
		{ get { return new RecommendedEvents(this, Session); } }
		
		/// <summary>
		/// The recommended artists for this user.
		/// </summary>
		public RecommendedArtists RecommendedArtists
		{ get { return new RecommendedArtists(this, Session); } }
	}
}

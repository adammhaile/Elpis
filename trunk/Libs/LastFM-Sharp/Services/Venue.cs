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
	/// A Last.fm venue.
	/// </summary>
	public class Venue : Base
	{
		/// <value>
		/// The venue ID.
		/// </value>
		public int ID {get; private set;}
		
		public Venue(int id, Session session)
			:base(session)
		{
			ID = id;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["venue"] = ID.ToString();
			
			return p;
		}

		/// <summary>
		/// Returns the upcoming <see cref="Event"/>s that are being held in this venue.
		/// </summary>
		/// <returns>
		/// A <see cref="Event"/>
		/// </returns>
		public Event[] GetUpcomingEvents()
		{
			XmlDocument doc = request("venue.getEvents");
			
			List<Event> list = new List<Event>();
			foreach(XmlNode n in doc.GetElementsByTagName("event"))
				list.Add(new Event(int.Parse(extract(n, "id")), Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// The past events held in this venue.
		/// </summary>
		public VenuePastEvents PastEvents
		{ get { return new VenuePastEvents(this, Session); } }

		/// <summary>
		/// Returns the venue's name.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetName()
		{
			// TODO: Replace this call when venue.getInfo comes out.
			
			XmlDocument doc = request("venue.getEvents");
			return doc.DocumentElement.GetElementsByTagName("events")[0].Attributes.GetNamedItem("venue").InnerText;
		}
	}
}

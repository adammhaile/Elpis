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
	/// The past events of a venue.
	/// </summary>
	public class VenuePastEvents : Pages<Event>
	{
		/// <value>
		/// The venue.
		/// </value>
		public Venue Venue {get; private set;}
		
		public VenuePastEvents(Venue venue, Session session)
			:base("venue.getPastEvents", session)
		{
			Venue = venue;
		}
		
		internal override RequestParameters getParams ()
		{
			return Venue.getParams();
		}
		
		public override Event[] GetPage (int page)
		{
			if(page < 1)
				throw new InvalidPageException(page, 1);
			
			RequestParameters p = getParams();
			p["page"] = page.ToString();
			
			List<Event> list = new List<Event>();
			XmlDocument doc = request("venue.getPastEvents", p);
			foreach(XmlNode n in doc.GetElementsByTagName("event"))
				list.Add(new Event(int.Parse(extract(n, "id")), Session));
			
			return list.ToArray();
		}
	}
}

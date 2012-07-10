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
	
	
	public class VenueSearch : Search<Venue>
	{
		
		public VenueSearch(string name, Session session)
			:base("venue", session)
		{
			this.searchTerms["venue"] = name;
		}
		
		public VenueSearch(string name, string countryAlphaTwoCode, Session session)
			:base("venue", session)
		{
			this.searchTerms["venue"] = name;
			this.searchTerms["country"] = countryAlphaTwoCode;
		}
		
		public override Venue[] GetPage (int page)
		{
			if (page < 1)
				throw new InvalidPageException(page, 1);
			
			RequestParameters p = getParams();
			p["page"] = page.ToString();
			
			XmlDocument doc = request(prefix + ".search", p);
			
			List<Venue> list = new List<Venue>();
			foreach(XmlNode n in doc.GetElementsByTagName("venue"))
				list.Add(new Venue(int.Parse(extract(n, "id")), Session));
			
			return list.ToArray();
		}

	}
}

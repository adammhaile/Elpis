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
	/// Encapsulates the album searching functions.
	/// </summary>
	public class AlbumSearch : Search<Album>
	{		
		public AlbumSearch(string albumName, Session session)
			:base("album", session)
		{
			searchTerms["album"] = albumName;
		}
			
		/// <summary>
		/// Returns a page of results.
		/// </summary>
		/// <param name="page">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="Album"/>
		/// </returns>
		public override Album[] GetPage(int page)
		{
			if (page < 1)
				throw new InvalidPageException(page, 1);
			
			RequestParameters p = getParams();
			p["page"] = page.ToString();
			
			XmlDocument doc = request(prefix + ".search", p);
			
			List<Album> list = new List<Album>();			
			foreach(XmlNode n in doc.GetElementsByTagName("album"))
				list.Add(new Album(extract(n, "artist"), extract(n, "name"), Session));
			
			return list.ToArray();
		}
	}
}

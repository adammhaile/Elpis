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
	/// Encapsulates the tag searching functions.
	/// </summary>
	/// <remarks>
	/// To create an object of this class use <see cref="Search.ForTags"/>.
	/// </remarks>
	public class TagSearch : Search<Tag>
	{	
		
		public TagSearch(string name, Session session)
			:base("tag", session)
		{
			this.searchTerms["tag"] = name;
		}
		
		/// <summary>
		/// Returns a page of results.
		/// </summary>
		/// <param name="page">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="Tag"/>
		/// </returns>
		public override Tag[] GetPage(int page)
		{
			if (page < 1)
				throw new InvalidPageException(page, 1);
			
			RequestParameters p = getParams();
			p["page"] = page.ToString();
			
			XmlDocument doc = request(prefix + ".search", p);
			
			List<Tag> list = new List<Tag>();			
			foreach(XmlNode n in doc.GetElementsByTagName("tag"))
				list.Add(new Tag(extract(n, "name"), Session));
			
			return list.ToArray();
		}
	}
}

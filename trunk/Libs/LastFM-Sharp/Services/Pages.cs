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
	/// An abstract class inherited by data objects that come in pages.
	/// </summary>
	public abstract class Pages<T> : Base
	{
		protected internal string methodName {get; set;}
		
		public Pages(string methodName, Session session)
			:base(session)
		{
			this.methodName = methodName;
		}
		
		public int GetPageCount()
		{
			XmlDocument doc = request(methodName);
			
			return int.Parse(doc.DocumentElement.ChildNodes[0].Attributes.GetNamedItem("totalPages").InnerText);
		}
		
		public int GetItemsPerPage()
		{
			XmlDocument doc = request(methodName);
			
			return int.Parse(doc.DocumentElement.ChildNodes[0].Attributes.GetNamedItem("perPage").InnerText);
		}
		
		public abstract T[] GetPage(int page);
	}
}

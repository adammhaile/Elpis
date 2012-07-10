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
	/// An abstract searching provider.
	/// </summary>
	public abstract class Search<T> : Base
	{
		protected internal string prefix {get; set;}
		protected internal Dictionary<string, string> searchTerms {get; set;}
		
		internal Search(string prefix, Session session)
			:base(session)
		{
			this.prefix = prefix;
			this.searchTerms = new Dictionary<string,string>();
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			
			foreach(string key in searchTerms.Keys)
				p[key] = searchTerms[key];
			
			return p;
		}
		
		/// <summary>
		/// Returne the total number of results.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetResultCount()
		{
			return Int32.Parse(extract(request(prefix + ".search"), "opensearch:totalResults"));
		}
		
		/// <summary>
		/// Returns the number of items per page.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetItemsPerPage()
		{
			return int.Parse(extract(request(prefix + ".search"), "opensearch:itemsPerPage"));
		}
		
		/// <summary>
		/// Returns a page of reuslts.
		/// </summary>
		/// <param name="page">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="T"/>
		/// </returns>
		public abstract T[] GetPage(int page);

		/// <summary>
		/// Specify how many items are returned in a page.
		/// </summary>
		/// <param name="itemsPerPage">
		/// A <see cref="System.Int32"/>
		/// </param>
		public void SpecifyItemsPerPage(int itemsPerPage)
		{
			this.searchTerms["limit"] = itemsPerPage.ToString();
		}
		
		/// <summary>
		/// I'm Feeling Lucky.
		/// </summary>
		/// <returns>
		/// A <see cref="T"/>
		/// </returns>
		public T GetFirstMatch()
		{
			return GetPage(1)[0];
		}
	}
}

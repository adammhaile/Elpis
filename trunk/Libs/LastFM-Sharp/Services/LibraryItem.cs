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
	/// An item in a user's library.
	/// </summary>
	public class LibraryItem<T>
	{
		protected internal T item {get; private set;}
		
		/// <value>
		/// How many times the user have played it.
		/// </value>
		public int Playcount {get; private set;}
		
		/// <value>
		/// How many tags have the user set to it.
		/// </value>
		public int Tagcount {get; private set;}
		
		public LibraryItem(T item, int playcount, int tagcount)
		{
			this.item = item;
			Playcount = playcount;
			Tagcount = tagcount;
		}
	}
}

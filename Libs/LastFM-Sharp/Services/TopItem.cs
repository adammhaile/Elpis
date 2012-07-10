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

namespace Lastfm.Services
{
	/// <summary>
	/// A top item in a list of top items.
	/// </summary>
	public class TopItem<T>
	{
		/// <summary>
		/// The concerned item.
		/// </summary>
		public T Item {get; private set;}
		
		/// <summary>
		/// The weight of this item in the list. A playcount, tagcount or a percentage.
		/// </summary>
		public int Weight {get; private set;}
		
		public TopItem(T item, int weight)
		{
			Item = item;
			Weight = weight; 
		}
	}
}

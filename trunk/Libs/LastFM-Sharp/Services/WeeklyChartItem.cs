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
	/// An abstract Weekly Chart Item class.
	/// </summary>
	public abstract class WeeklyChartItem
	{
		/// <summary>
		/// The rank of this item in the chart.
		/// </summary>
		public int Rank {get; private set;}
		
		/// <value>
		/// The number of playcounts during the timespan of the chart.
		/// </value>
		/// <summary> Playcounts during.</summary>
		public int Playcount {get; private set;}
		
		/// <summary>Time Span</summary>
		/// <value>
		/// The time span of this chart list.
		/// </value>
		public WeeklyChartTimeSpan Span {get; private set;}
		
		internal WeeklyChartItem(int rank, int playcount, WeeklyChartTimeSpan span)
		{
			Rank = rank;
			Playcount = playcount;
			Span = span;
		}
	}
}

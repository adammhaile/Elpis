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
using System.Text.RegularExpressions;

namespace Lastfm.Radio
{
	/// <summary>
	/// A unique radio station URI.
	/// They are in the form of lastfm://<stationtype>/<resourcename>/<station-subtype>
	/// </summary>
	public class StationURI
	{
		public string StationType {get; set;}
		public string ResourceName {get; set;}
		public string SubType {get; set;}
		
		public StationURI(string uri)
		{
			// TODO
		}
		
		public StationURI(string stationType, string resourceName, string subType)
		{
			StationType = stationType;
			ResourceName = resourceName;
			SubType = subType;
		}
		
		public override string ToString ()
		{
			return String.Format("lastfm://{0}/{1}/{2}", StationType, ResourceName, SubType);
		}
		
		public static StationURI GetGlobalTag(string tagName)
		{
			return new StationURI("globaltags", tagName, "");
		}
		
		public static StationURI GetLoved(string username)
		{
			return new StationURI("user", username, "loved");
		}
		
		public static StationURI GetNeighbours(string username)
		{
			return new StationURI("user", username, "neighbours");
		}
		
		public static StationURI GetRecommended(string username)
		{
			return new StationURI("user", username, "recommended");
		}
		
		public static StationURI GetArtist(string artistName)
		{
			return new StationURI("artist", artistName, "similarartists");
		}
		
		public static StationURI GetUserLibrary(string username)
		{
			return new StationURI("user", username, "library");
		}
		
		public static StationURI GetTopFans(string artistName)
		{
			return new StationURI("artist", artistName, "fans");
		}
		
	}
}

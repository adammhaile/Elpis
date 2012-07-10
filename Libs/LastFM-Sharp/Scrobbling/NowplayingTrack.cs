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

namespace Lastfm.Scrobbling
{
	public class NowplayingTrack
	{
		public string Artist {get; set;}
		public string Title {get; set;}
		public string Album {get; set;}
		public TimeSpan Duration {get; set;}
		public string MBID {get; set;}
		public int Number {get; set;}
		
		private void init()
		{
			Duration = new TimeSpan();
		}
		
		public NowplayingTrack(string artist, string title)
		{
			// Set initial values
			init();
			
			Artist = artist;
			Title = title;
		}
		
		public NowplayingTrack(string artist, string title, TimeSpan duration)
		{
			// Set initial values
			init();
			
			Artist = artist;
			Title = title;
			Duration = duration;
		}
	}
}

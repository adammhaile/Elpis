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

namespace Lastfm.Radio
{
	
	
	public class Track
	{
		public string StreamPath {get; set;}
		public string Title {get; set;}
		public string Artist {get; set;}
		public string Identifier {get; set;}
		public string AlbumTitle {get; set;}
		public TimeSpan Duration {get; set;}
		public string ImageLocation {get; set;}
		
		public Track(string artist, string title, string album, string streamPath,
		             string identifier, string imageLocation, TimeSpan duration)
		{
			Artist = artist;
			Title = title;
			AlbumTitle = album;
			StreamPath = streamPath;
			Identifier = identifier;
			ImageLocation = imageLocation;
			Duration = duration;
		}
	}
}

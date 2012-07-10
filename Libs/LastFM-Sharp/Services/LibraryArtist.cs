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
	/// An <see cref="Artist"/> in a <see cref="Library"/>.
	/// </summary>
	public class LibraryArtist : LibraryItem<Artist>
	{
		public LibraryArtist(Artist artist, int playcount, int tagcount)
			:base(artist, playcount, tagcount)
		{
		}
		
		/// <summary>
		/// The artist.
		/// </summary>
		public Artist Artist { get { return this.item; } }
	}
}

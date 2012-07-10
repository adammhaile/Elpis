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
	/// An <see cref="Album"/> in a <see cref="Library"/>.
	/// </summary>
	public class LibraryAlbum : LibraryItem<Album>
	{
		public LibraryAlbum(Album album, int playcount, int tagcount)
			:base(album, playcount, tagcount)
		{}

		/// <summary>
		/// The album.
		/// </summary>
		public Album Album { get { return this.item; } }
	}
}

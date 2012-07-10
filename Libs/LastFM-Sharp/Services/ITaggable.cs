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
	/// Objects that implement this can be tagged on Last.fm.
	/// </summary>
	public interface ITaggable
	{
		void AddTags(params Tag[] tags);
		void AddTags(params String[] tags);
		void AddTags(TagCollection tags);
		Tag[] GetTags();
		TopTag[] GetTopTags();
		TopTag[] GetTopTags(int limit);
		void RemoveTags(params string[] tags);
		void RemoveTags(params Tag[] tags);
		void RemoveTags(TagCollection tags);
		void SetTags(Tag[] tags);
		void SetTags(string[] tags);
		void SetTags(TagCollection tags);
		void ClearTags();
	}
}

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
using System.Net;

namespace Lastfm
{
	/// <summary>
	/// Shared properties.
	/// </summary>
	public static class Lib
	{
		/// <summary>
		/// A <see cref="IWebProxy"/>.
		/// </summary>
		/// <value>
		/// A web proxy to be used in making all the calls to Last.fm.
		/// </value>
		/// <remarks>
		/// To enable using a proxy server, set this value to a <see cref="IWebProxy"/>, like <see cref="WebProxy"/>.
		/// To disable using a proxy server, set it to null.
		/// 
		/// Default value is null.
		/// </remarks>
		public static IWebProxy Proxy {get; set;}
		
		/// <summary>
		/// Returns the version of this assembly.
		/// </summary>
		public static Version Version
		{
			get{ return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
		}
	}
}

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
using System.Collections.Generic;

namespace Lastfm.Services
{
	/// <summary>
	/// A collection of recipients.
	/// </summary>
	public class Recipients : List<string>
	{
		public Recipients()
			:base()
		{
		}
		
		/// <summary>
		/// Add a Last.fm username.
		/// </summary>
		/// <param name="username">
		/// A <see cref="System.String"/>
		/// </param>
		public new void Add(string username)
		{
			base.Add(username);
		}
		
		/// <summary>
		/// Add a <see cref="User"/>.
		/// </summary>
		/// <param name="user">
		/// A <see cref="User"/>
		/// </param>
		public void Add(User user)
		{
			base.Add(user.Name);
		}
		
		/// <summary>
		/// Add an email.
		/// </summary>
		/// <param name="email">
		/// A <see cref="System.String"/>
		/// </param>
		public void AddEmail(string email)
		{
			base.Add(email);
		}
	}
}

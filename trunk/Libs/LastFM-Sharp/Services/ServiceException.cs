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
	public enum ServiceExceptionType
	{
		InvalidService = 2,
		InvalidMethod = 3,
		AuthenticationFailed = 4,
		InvalidFormat = 5,
		InvalidParameters = 6,
		InvalidResource = 7,
		TokenError = 8,
		InvalidSessionKey = 9,
		InvalidAPIKey = 10,
		ServiceOffline = 11,
		SubscribersOnly = 12,
		InvalidSignature = 13,
		UnauthorizedToken = 14,
		ExpiredToken = 15,
		FreeRadioExpired = 18,
		NotEnoughContent = 20,
		NotEnoughMembers = 21,
		NotEnoughFans = 22,
		NotEnoughNeighbours = 23
	}
	
	/// <summary>
	/// A Last.fm web service exception
	/// </summary>
	public class ServiceException : Exception
	{
		/// <summary>
		/// The exception type.
		/// </value>
		public ServiceExceptionType Type {get; private set;}
		
		/// <summary>
		/// The description of the exception.
		/// </summary>
		public string Description {get; private set;}
		
		public ServiceException(ServiceExceptionType type, string description) : base()
		{
			this.Type = type;
			this.Description = description;
		}
		
		public override string Message
		{
			get
			{				
				return this.Type.ToString() + ": " + this.Description;
			}
		}
	}
}

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
using System.Web;
using System.Text;

namespace Lastfm
{
	internal class RequestParameters : SortedDictionary<string, string>
	{
		public override string ToString()
		{
			string values = "";
			foreach(string key in this.Keys)
				values += HttpUtility.UrlEncode(key) + "=" +
					HttpUtility.UrlEncode(this[key]) + "&";
			values = values.Substring(0, values.Length - 1);
			
			return values;
		}
		
		internal byte[] ToBytes()
		{	
			return Encoding.ASCII.GetBytes(ToString());
		}
		
		internal string serialize()
		{
			string line = "";
			
			foreach (string key in Keys)
				line += key + "\t" + this[key] + "\t";
			
			return line;
		}
		
		internal RequestParameters(string serialization)
			:base()
		{
			string[] values = serialization.Split('\t');
			
			for(int i = 0; i < values.Length - 1; i++)
			{
				if ( (i%2) == 0 )
					this[values[i]] = values[i+1];
			}
		}
		
		public RequestParameters()
			:base()
		{
		}
	}
}

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
using System.Web;
using System.Net;
using System.IO;
using System.Text;

namespace Lastfm.Scrobbling
{
	internal class Request
	{
		RequestParameters Parameters;
		Uri URI {get; set;}
		
		internal Request(Uri uri, RequestParameters parameters)
		{
			URI = uri;
			Parameters = parameters;
		}
		
		internal string execute()
		{
			
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URI);
			byte[] data = Parameters.ToBytes();
			
			request.ContentLength = data.Length;
			request.UserAgent = Utilities.UserAgent;
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
      request.Headers["Accept-Charset"] = "utf-8";
			
			if (Lib.Proxy != null)
				request.Proxy = Lib.Proxy;
			
			Stream writeStream = request.GetRequestStream();
			writeStream.Write(data, 0, data.Length);
			writeStream.Close();
			
			HttpWebResponse webresponse;
			try
			{
				webresponse = (HttpWebResponse)request.GetResponse();
			}catch (WebException e){
				webresponse = (HttpWebResponse)e.Response;
			}
			
			StreamReader reader = new StreamReader(webresponse.GetResponseStream());
			
			string output = reader.ReadToEnd();
			
			checkForErrors(output);
			
			return output;
		}
		
		private void checkForErrors(string output)
		{
			string line = output.Split('\n')[0];
			
			if(line.StartsWith("BANNED"))
				throw new BannedClientException();
			else if(line.StartsWith("BADAUTH"))
				throw new AuthenticationFailureException();
			else if(line.StartsWith("BADTIME"))
				throw new WrongTimeException();
			else if(line.StartsWith("FAILED"))
				throw new ScrobblingException(output.Substring(output.IndexOf(' ') + 1));
		}
	}
}

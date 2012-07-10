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
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;

namespace Lastfm.Services
{
	internal class Request
	{
		const string ROOT = "http://ws.audioscrobbler.com/2.0/";
		
		public string MethodName {get; private set;}
		public Session Session {get; private set;}
		
		public RequestParameters Parameters {get; private set;}
		
		internal static DateTime? lastCallTime {get; set;}
		
		public Request(string methodName, Session session, RequestParameters parameters)
		{
			this.MethodName = methodName;
			this.Session = session;
			this.Parameters = parameters;
			
			this.Parameters["method"] = this.MethodName;
			this.Parameters["api_key"] = this.Session.APIKey;
			if (Session.Authenticated)
			{
				this.Parameters["sk"] = this.Session.SessionKey;
				signIt();
			}
		}
		
		internal void signIt()
		{
			// because auth.getSession requires a signature without session key. 
			this.Parameters["api_sig"] = this.getSignature();
		}
		
		private string getSignature()
		{
			string str = "";
			foreach(string key in this.Parameters.Keys)
				str += key + this.Parameters[key];
			
			str += this.Session.APISecret;
			
			return Utilities.MD5(str);
			
		}
		
		private void delay()
		{
			// If the last call was less than one second ago, it would delay execution for a second.
			
			if (Request.lastCallTime == null)
				Request.lastCallTime = new Nullable<DateTime>(DateTime.Now);
			
			if (DateTime.Now.Subtract(Request.lastCallTime.Value) > new TimeSpan(0, 0, 1))
				Thread.Sleep(1000);
		}

		public XmlDocument execute()
		{
			// delays the execution if necessary.
			this.delay();
			
			// Go on normally from here.
			byte[] data = Parameters.ToBytes();
			
			/* RANT 
			 * The most annoying thing i've ever encountered in my life.
			 * The freakin "Expect: 100-continue" kept being added to my headers collection
			 * making the post call always fail. I've waisted a whole day hunting down this
			 * bug, and finally i fired up wireshark sniffing my calls and found out about it.
			 * After googling the problem now that i know what it is exactly, I found
			 * the solution there: http://haacked.com/archive/2004/05/15/http-web-request-expect-100-continue.aspx
			 * so, thank you for saving my sanity. This works now. time to move on...
			*/
			System.Net.ServicePointManager.Expect100Continue = false;
			
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ROOT);
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
			
			XmlDocument doc = new XmlDocument();
      doc.Load(webresponse.GetResponseStream());

			checkForErrors(doc);
			
			return doc;
		}
		
		private void checkForErrors(XmlDocument document)
		{
			XmlNode n = document.GetElementsByTagName("lfm")[0];
			
			string status = n.Attributes[0].InnerText;
			
			if (status == "failed")
			{
				XmlNode err = document.GetElementsByTagName("error")[0];
				ServiceExceptionType type = (ServiceExceptionType)Convert.ToInt32(err.Attributes[0].InnerText);
				string description = err.InnerText;
				
				throw new ServiceException(type, description);
			}
		}
	}
}

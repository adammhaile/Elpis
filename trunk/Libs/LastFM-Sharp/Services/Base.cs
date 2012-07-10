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
using System.Xml;
using System.Web;

namespace Lastfm.Services
{
	/// <summary>
	/// A base class for most of the objects.
	/// </summary>
	public abstract class Base
	{
		protected Session Session {get; set;}
		
		public Base(Session session)
		{
			Session = session;
		}
		
		internal abstract RequestParameters getParams();
    
		internal XmlDocument request(string methodName, RequestParameters parameters)
		{
			return (new Request(methodName, Session, parameters)).execute();
		}
    
		internal XmlDocument request(string methodName)
		{
			return (new Request(methodName, Session, getParams())).execute();
		}
		
		internal string extract(XmlNode node, string name, int index)
		{
			return ((XmlElement)node).GetElementsByTagName(name)[index].InnerText;
		}
		
		internal string extract(XmlNode node, string name)
		{
			return extract((XmlElement)node, name, 0);
		}
		
		internal string extract(XmlDocument document, string name)
		{
			return extract(document.DocumentElement, name);
		}
		
		internal string extract(XmlDocument document, string name, int index)
		{
			return extract(document.DocumentElement, name, index);
		}
		
		internal string[] extractAll(XmlNode node, string name, int limitCount)
		{
			string[] s = extractAll(node, name);
			List<string> l = new List<string>();
			
			for(int i = 0; i < limitCount; i++)
				l.Add(s[i]);
			
			return l.ToArray();
		}
    
		internal string[] extractAll(XmlNode node, string name)
		{
			List<string> list = new List<string>();
			
			for(int i = 0; i < ((XmlElement)node).GetElementsByTagName(name).Count; i++)
				list.Add(extract(node, name, i));
			
			return list.ToArray();
		}
		
		internal string[] extractAll(XmlDocument document, string name)
		{
			return extractAll(document.DocumentElement, name);
		}
		
		internal string[] extractAll(XmlDocument document, string name, int limitCount)
		{
			return extractAll(document.DocumentElement, name, limitCount);
		}
		
		internal void requireAuthentication()
		{
			if(!this.Session.Authenticated)
				throw new AuthenticationRequiredException();
		}
		
		internal T[] sublist<T> (T[] original, int length)
		{
			List<T> list = new List<T>();
			
			for(int i=0; i<length; i++)
				list.Add(original[i]);
			
			return list.ToArray();
		}
		
		internal string urlSafe(string text)
		{
			return HttpUtility.UrlEncode(HttpUtility.UrlEncode(text));
		}
		
		internal string getPeriod(Period period)
		{
			string[] values = new string[] {"overall", "3month", "6month", "12month"};
			
			return values[(int)period];
		}
		
		internal string getSiteDomain(SiteLanguage language)
		{
			Dictionary<SiteLanguage, string> domains = new Dictionary<SiteLanguage,string>();
			
			domains.Add(SiteLanguage.English, "www.last.fm");
			domains.Add(SiteLanguage.German, "www.lastfm.de");
			domains.Add(SiteLanguage.Spanish, "www.lastfm.es");
			domains.Add(SiteLanguage.French, "www.lastfm.fr");
			domains.Add(SiteLanguage.Italian, "www.lastfm.it");
			domains.Add(SiteLanguage.Polish, "www.lastfm.pl");
			domains.Add(SiteLanguage.Portuguese, "www.lastfm.com.br");
			domains.Add(SiteLanguage.Swedish, "www.lastfm.se");
			domains.Add(SiteLanguage.Turkish, "www.lastfm.com.tr");
			domains.Add(SiteLanguage.Russian, "www.lastfm.ru");
			domains.Add(SiteLanguage.Japanese, "www.lastfm.jp");
			domains.Add(SiteLanguage.Chinese, "cn.last.fm");
			
			return domains[language];
		}
	}
}

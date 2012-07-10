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
using System.Xml;
using System.Collections.Generic;

namespace Lastfm.Services
{	
	/// <summary>
	/// A Last.fm event.
	/// </summary>
	public class Event : Base, IEquatable<Event>, IShareable, IHasImage, IHasURL
	{
		/// <summary>
		/// The event ID.
		/// </summary>
		public int ID {get; private set;}
		
		public Event(int id, Session session)
			:base(session)
		{
			ID = id;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["event"] = ID.ToString();
			
			return p;
		}
		
		/// <summary>
		/// Set the authenticated user's status for this event. 
		/// </summary>
		/// <param name="attendance">
		/// A <see cref="EventAttendance"/>
		/// </param>
		public void SetAttendance(EventAttendance attendance)
		{
			requireAuthentication();
			
			RequestParameters p = getParams();
			int i = (int)attendance;
			p["status"] = i.ToString();
			request("event.attend", p);
		}
		
		/// <summary>
		/// Returns the title of the event.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetTitle()
		{
			XmlDocument doc = request("event.getInfo");
			
			return extract(doc, "title");
		}
		
		/// <summary>
		/// Returns the participating artists in this event.
		/// </summary>
		/// <returns>
		/// A <see cref="Artist"/>
		/// </returns>
		public Artist[] GetArtists()
		{
			XmlDocument doc = request("event.getInfo");
			
			List<Artist> list = new List<Artist>();
			foreach(string name in extractAll(doc, "artist"))
				list.Add(new Artist(name, Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the headliner artist.
		/// </summary>
		/// <returns>
		/// A <see cref="Artist"/>
		/// </returns>
		public Artist GetHeadliner()
		{
			XmlDocument doc = request("event.getInfo");
			
			return new Artist(extract(doc, "headliner"), Session);
		}
		
		/// <summary>
		/// Returns the start time of the event.
		/// </summary>
		/// <returns>
		/// A <see cref="DateTime"/>
		/// </returns>
		public DateTime GetStartDate()
		{
			XmlDocument doc = request("event.getInfo");
			
			return DateTime.Parse(extract(doc, "startDate"));
		}
		
		/// <summary>
		/// Returns the description of the evnet.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetDescription()
		{
			XmlDocument doc = request("event.getInfo");
			
			return extract(doc, "description");
		}
		
		/// <summary>
		/// Returns the url to the image of this event.
		/// </summary>
		/// <param name="size">
		/// A <see cref="ImageSize"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL(ImageSize size)
		{
			XmlDocument doc = request("event.getInfo");
			
			return extractAll(doc, "image")[(int)size];
		}
		
		/// <summary>
		/// Returns the url to the image of this event.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL()
		{
			return GetImageURL(ImageSize.Large);
		}
		
		/// <summary>
		/// Returns the number of attendees.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetAttendantCount()
		{
			XmlDocument doc = request("event.getInfo");
			
			return Int32.Parse(extract(doc, "attendance"));
		}
		
		/// <summary>
		/// Returns the number of reviews for this event.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetReviewCount()
		{
			XmlDocument doc = request("event.getInfo");
			
			return Int32.Parse(extract(doc, "reviews"));
		}

		/// <summary>
		/// Returns the flickr tag.
		/// </summary>
		/// <remarks>
		/// You can tag your image on flickr with this tag and they would be imported into
		/// the last.fm page for this event. 
		/// </remarks>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetFlickrTag()
		{
			XmlDocument doc = request("event.getInfo");
			
			return extract(doc, "tag");
		}
		
		/// <summary>
		/// Check to see if this object equals another.
		/// </summary>
		/// <param name="eventObject">
		/// A <see cref="Event"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Equals(Event eventObject)
		{
			if(eventObject.ToString() == this.ToString())
				return true;
			else
				return false;
		}
		
		/// <summary>
		/// Share this event with others.
		/// </summary>
		/// <param name="recipients">
		/// A <see cref="Recipients"/>
		/// </param>
		/// <param name="message">
		/// A <see cref="System.String"/>
		/// </param>
		public void Share(Recipients recipients, string message)
		{
			if (recipients.Count > 1)
			{
				foreach(string recipient in recipients)
				{
					Recipients r = new Recipients();
					r.Add(recipient);
					Share(r, message);
				}
				
				return;
			}
			
			requireAuthentication();
			
			RequestParameters p = getParams();
			p["recipient"] = recipients[0];
			p["message"] = message;
			
			request("event.Share", p);
		}
		
		/// <summary>
		/// Share this event with others.
		/// </summary>
		/// <param name="recipients">
		/// A <see cref="Recipients"/>
		/// </param>
		public void Share(Recipients recipients)
		{
			if (recipients.Count > 1)
			{
				foreach(string recipient in recipients)
				{
					Recipients r = new Recipients();
					r.Add(recipient);
					Share(r);
				}
				
				return;
			}
			
			requireAuthentication();
			
			RequestParameters p = getParams();
			p["recipient"] = recipients[0];
			
			request("event.Share", p);
		}
		
		/// <summary>
		/// Returns the Last.fm page of this object.
		/// </summary>
		/// <param name="language">
		/// A <see cref="SiteLanguage"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetURL(SiteLanguage language)
		{
			string domain = getSiteDomain(language);
			
			return "http://" + domain + "/event/" + ID.ToString();
		}

		/// <summary>
		/// The object's Last.fm page url.
		/// </summary>
		public string URL
		{ get { return GetURL(SiteLanguage.English); } }

		/// <value>
		/// The venue where the event is being held.
		/// </value>
		public Venue Venue
		{
			get
			{
				XmlDocument doc = request("event.getInfo");
								
				string url = ((XmlElement)doc.GetElementsByTagName("venue")[0]).GetElementsByTagName("url")[0].InnerText;
				int id = int.Parse(url.Substring(url.LastIndexOf('/') + 1));						
				
				return new Venue(id, Session);
			}
		}
	}
}

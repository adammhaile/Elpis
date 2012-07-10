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

namespace Lastfm.Services
{
	/// <summary>
	/// A Last.fm Group.
	/// </summary>
	public class Group : Base, IEquatable<Group>, IHasWeeklyAlbumCharts, IHasWeeklyArtistCharts,
	IHasWeeklyTrackCharts, IHasURL
	{
		/// <summary>
		/// Name of the group.
		/// </summary>
		public string Name {get; private set;}
		
		public Group(string groupName, Session session)
			:base(session)
		{
			Name = groupName;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["group"] = Name;
			
			return p;
		}
		
		/// <summary>
		/// String representation of the object.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString()
		{
			return Name;
		}
		
		/// <summary>
		/// Returns the latest weekly track chart for this group.
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyTrackChart"/>
		/// </returns>
		public WeeklyTrackChart GetWeeklyTrackChart()
		{
			XmlDocument doc = request("group.getWeeklyTrackChart");
			
			XmlNode n = doc.GetElementsByTagName("weeklytrackchart")[0];
			
			DateTime nfrom = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[1].InnerText), DateTimeKind.Utc);
			DateTime nto = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[2].InnerText), DateTimeKind.Utc);
			
			WeeklyTrackChart chart = new WeeklyTrackChart(new WeeklyChartTimeSpan(nfrom, nto));
			
			foreach(XmlNode node in doc.GetElementsByTagName("track"))
			{
				int rank = Int32.Parse(node.Attributes[0].InnerText);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				WeeklyTrackChartItem item = 
					new WeeklyTrackChartItem(new Track(extract(node, "artist"), extract(node, "name"), Session),
					                         rank, playcount, new WeeklyChartTimeSpan(nfrom, nto));
				
				chart.Add(item);
			}
			
			return chart;
		}
		
		/// <summary>
		/// Returns the weekly track chart for this group in the given <see cref="Lastfm.Services.WeeklyChartTimeSpan"/>.
		/// </summary>
		/// <param name="span">
		/// A <see cref="WeeklyChartTimeSpan"/>
		/// </param>
		/// <returns>
		/// A <see cref="WeeklyTrackChart"/>
		/// </returns>
		public WeeklyTrackChart GetWeeklyTrackChart(WeeklyChartTimeSpan span)
		{
			RequestParameters p = getParams();
			
			p["from"] = Utilities.DateTimeToUTCTimestamp(span.From).ToString();
			p["to"] = Utilities.DateTimeToUTCTimestamp(span.To).ToString();
			
			XmlDocument doc = request("group.getWeeklyTrackChart", p);
			
			XmlNode n = doc.GetElementsByTagName("weeklytrackchart")[0];
			
			DateTime nfrom = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[1].InnerText), DateTimeKind.Utc);
			DateTime nto = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[2].InnerText), DateTimeKind.Utc);
			
			WeeklyTrackChart chart = new WeeklyTrackChart(new WeeklyChartTimeSpan(nfrom, nto));
			
			foreach(XmlNode node in doc.GetElementsByTagName("track"))
			{
				int rank = Int32.Parse(node.Attributes[0].InnerText);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				WeeklyTrackChartItem item = 
					new WeeklyTrackChartItem(new Track(extract(node, "artist"), extract(node, "name"), Session),
					                         rank, playcount, new WeeklyChartTimeSpan(nfrom, nto));
				
				chart.Add(item);
			}
			
			return chart;
		}
		
		/// <summary>
		/// Returns the latest weekly artist chart for this group.
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyArtistChart"/>
		/// </returns>
		public WeeklyArtistChart GetWeeklyArtistChart()
		{
			XmlDocument doc = request("group.getWeeklyArtistChart");
			
			XmlNode n = doc.GetElementsByTagName("weeklyartistchart")[0];
			
			DateTime nfrom = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[1].InnerText), DateTimeKind.Utc);
			DateTime nto = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[2].InnerText), DateTimeKind.Utc);
			
			WeeklyArtistChart chart = new WeeklyArtistChart(new WeeklyChartTimeSpan(nfrom, nto));
			
			foreach(XmlNode node in doc.GetElementsByTagName("artist"))
			{
				int rank = Int32.Parse(node.Attributes[0].InnerText);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				WeeklyArtistChartItem item = 
					new WeeklyArtistChartItem(new Artist(extract(node, "name"), Session),
					                         rank, playcount, new WeeklyChartTimeSpan(nfrom, nto));
				
				chart.Add(item);
			}
			
			return chart;
		}
		
		/// <summary>
		/// Returns the weekly artist chart for this group in the given 
		/// <see cref="Lastfm.Services.WeeklyChartTimeSpan"/>.
		/// </summary>
		/// <param name="span">
		/// A <see cref="WeeklyChartTimeSpan"/>
		/// </param>
		/// <returns>
		/// A <see cref="WeeklyArtistChart"/>
		/// </returns>
		public WeeklyArtistChart GetWeeklyArtistChart(WeeklyChartTimeSpan span)
		{
			RequestParameters p = getParams();
			
			p["from"] = Utilities.DateTimeToUTCTimestamp(span.From).ToString();
			p["to"] = Utilities.DateTimeToUTCTimestamp(span.To).ToString();
			
			XmlDocument doc = request("group.getWeeklyArtistChart", p);
			
			XmlNode n = doc.GetElementsByTagName("weeklyartistchart")[0];
			
			DateTime nfrom = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[1].InnerText), DateTimeKind.Utc);
			DateTime nto = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[2].InnerText), DateTimeKind.Utc);
			
			WeeklyArtistChart chart = new WeeklyArtistChart(new WeeklyChartTimeSpan(nfrom, nto));
			
			foreach(XmlNode node in doc.GetElementsByTagName("artist"))
			{
				int rank = Int32.Parse(node.Attributes[0].InnerText);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				WeeklyArtistChartItem item = 
					new WeeklyArtistChartItem(new Artist(extract(node, "name"), Session),
					                         rank, playcount, new WeeklyChartTimeSpan(nfrom, nto));
				
				chart.Add(item);
			}
			
			return chart;
		}
		
		/// <summary>
		/// Returns the latest weekly album chart for this group.
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyAlbumChart"/>
		/// </returns>
		public WeeklyAlbumChart GetWeeklyAlbumChart()
		{
			XmlDocument doc = request("group.getWeeklyAlbumChart");
			
			XmlNode n = doc.GetElementsByTagName("weeklyalbumchart")[0];
			
			DateTime nfrom = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[1].InnerText), DateTimeKind.Utc);
			DateTime nto = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[2].InnerText), DateTimeKind.Utc);
			
			WeeklyAlbumChart chart = new WeeklyAlbumChart(new WeeklyChartTimeSpan(nfrom, nto));
			
			foreach(XmlNode node in doc.GetElementsByTagName("album"))
			{
				int rank = Int32.Parse(node.Attributes[0].InnerText);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				WeeklyAlbumChartItem item = 
					new WeeklyAlbumChartItem(new Album(extract(node, "artist"), extract(node, "name"), Session),
					                         rank, playcount, new WeeklyChartTimeSpan(nfrom, nto));
				
				chart.Add(item);
			}
			
			return chart;
		}
		
		/// <summary>
		/// Returns the weekly album chart for this group in the given <see cref="Lastfm.Services.WeeklyChartTimeSpan"/>.
		/// </summary>
		/// <param name="span">
		/// A <see cref="WeeklyChartTimeSpan"/>
		/// </param>
		/// <returns>
		/// A <see cref="WeeklyAlbumChart"/>
		/// </returns>
		public WeeklyAlbumChart GetWeeklyAlbumChart(WeeklyChartTimeSpan span)
		{
			RequestParameters p = getParams();
			
			p["from"] = Utilities.DateTimeToUTCTimestamp(span.From).ToString();
			p["to"] = Utilities.DateTimeToUTCTimestamp(span.To).ToString();
			
			XmlDocument doc = request("group.getWeeklyAlbumChart", p);
			
			XmlNode n = doc.GetElementsByTagName("weeklyalbumchart")[0];
			
			DateTime nfrom = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[1].InnerText), DateTimeKind.Utc);
			DateTime nto = Utilities.TimestampToDateTime(Int64.Parse(n.Attributes[2].InnerText), DateTimeKind.Utc);
			
			WeeklyAlbumChart chart = new WeeklyAlbumChart(new WeeklyChartTimeSpan(nfrom, nto));
			
			foreach(XmlNode node in doc.GetElementsByTagName("album"))
			{
				int rank = Int32.Parse(node.Attributes[0].InnerText);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				WeeklyAlbumChartItem item = 
					new WeeklyAlbumChartItem(new Album(extract(node, "artist"), extract(node, "name"), Session),
					                         rank, playcount, new WeeklyChartTimeSpan(nfrom, nto));
				
				chart.Add(item);
			}
			
			return chart;
		}
		
		/// <summary>
		/// Returns the available timespans for charts available for this group.
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyChartTimeSpan"/>
		/// </returns>
		public WeeklyChartTimeSpan[] GetWeeklyChartTimeSpans()
		{
			XmlDocument doc = request("group.getWeeklyChartList");
			
			List<WeeklyChartTimeSpan> list = new List<WeeklyChartTimeSpan>();
			foreach(XmlNode node in doc.GetElementsByTagName("chart"))
			{
				long lfrom = long.Parse(node.Attributes[0].InnerText);
				long lto = long.Parse(node.Attributes[1].InnerText);
				
				DateTime from = Utilities.TimestampToDateTime(lfrom, DateTimeKind.Utc);
				DateTime to = Utilities.TimestampToDateTime(lto, DateTimeKind.Utc);
				
				list.Add(new WeeklyChartTimeSpan(from, to));
			}
			
			return list.ToArray();
		}
		
		public bool Equals(Group group)
		{
			return (group.Name == this.Name);
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
			
			return "http://" + domain + "/group/" + urlSafe(Name);
		}
		
		/// <summary>
		/// The object's Last.fm page url.
		/// </summary>
		public string URL
		{ get { return GetURL(SiteLanguage.English); } }
		
		/// <value>
		/// The members in this group.
		/// </value>
		public GroupMembers Members
		{ get { return new GroupMembers(this, Session); } }
	}
}

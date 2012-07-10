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
	/// A Last.fm user.
	/// </summary>
	public class User : Base, IEquatable<User>, IHasWeeklyTrackCharts, IHasWeeklyAlbumCharts,
	IHasWeeklyArtistCharts, IHasURL
	{
		/// <summary>
		/// The user's name.
		/// </summary>
		public string Name {get; private set;}
		
		/// <summary>
		/// Returns the user's library.
		/// </summary>
		public Library Library
		{
			get { return new Library(this, Session); }
		}
		
		public User(string name, Session session)
			:base(session)
		{
			Name = name;
		}
		
		/// <summary>
		/// String representation of the object.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString ()
		{
			return Name;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["user"] = Name;
			
			return p;
		}
		
		/// <summary>
		/// Check to see if this object equals another.
		/// </summary>
		/// <param name="user">
		/// A <see cref="User"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Equals(User user)
		{
			return (user.Name == this.Name);
		}
		
		/// <summary>
		/// Returns the latest weekly track chart.
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyTrackChart"/>
		/// </returns>
		public WeeklyTrackChart GetWeeklyTrackChart()
		{
			XmlDocument doc = request("user.getWeeklyTrackChart");
			
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
		/// Returns a weekly chart specified by a certain week.
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
			
			XmlDocument doc = request("user.getWeeklyTrackChart", p);
			
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
		/// Returns the latest weekly artist chart.
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyArtistChart"/>
		/// </returns>
		public WeeklyArtistChart GetWeeklyArtistChart()
		{
			XmlDocument doc = request("user.getWeeklyArtistChart");
			
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
		/// Returns a weekly artist chart of a specified week.
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
			
			XmlDocument doc = request("user.getWeeklyArtistChart", p);
			
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
		/// Returns the latest weekly album chart.
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyAlbumChart"/>
		/// </returns>
		public WeeklyAlbumChart GetWeeklyAlbumChart()
		{
			XmlDocument doc = request("user.getWeeklyAlbumChart");
			
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
		/// Returns a weekly album chart of a specified week.
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
			
			XmlDocument doc = request("user.getWeeklyAlbumChart", p);
			
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
		/// Returns all the vailable weeks (as an array of <see cref="WeeklyChartTimeSpan"/>)
		/// </summary>
		/// <returns>
		/// A <see cref="WeeklyChartTimeSpan"/>
		/// </returns>
		public WeeklyChartTimeSpan[] GetWeeklyChartTimeSpans()
		{
			XmlDocument doc = request("user.getWeeklyChartList");
			
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
		
		/// <summary>
		/// Returns the top tracks listened to by this user in a specified period.
		/// </summary>
		/// <param name="period">
		/// A <see cref="Period"/>
		/// </param>
		/// <returns>
		/// A <see cref="TopTrack"/>
		/// </returns>
		public TopTrack[] GetTopTracks(Period period)
		{
			RequestParameters p = getParams();
			p["period"] = getPeriod(period);
			
			XmlDocument doc = request("user.getTopTracks", p);
			
			List<TopTrack> list = new List<TopTrack>();
			foreach(XmlNode node in doc.GetElementsByTagName("track"))
			{
				Track track = new Track(extract(node, "name", 1), extract(node, "name"), Session);
				int count = Int32.Parse(extract(node, "playcount"));
				
				list.Add(new TopTrack(track, count));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the overall most listned-to tracks by this user.
		/// </summary>
		/// <returns>
		/// A <see cref="TopTrack"/>
		/// </returns>
		public TopTrack[] GetTopTracks()
		{
			return GetTopTracks(Period.Overall);
		}
		
		/// <summary>
		/// Returns the top tags used by this user.
		/// </summary>
		/// <returns>
		/// A <see cref="TopTag"/>
		/// </returns>
		public TopTag[] GetTopTags()
		{
			XmlDocument doc = request("user.getTopTags");
			
			List<TopTag> list = new List<TopTag>();
			foreach(XmlNode node in doc.GetElementsByTagName("tag"))
				list.Add(new TopTag(new Tag(extract(node, "name"), Session), Int32.Parse(extract(node, "count"))));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the top tags used by this user.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="TopTag"/>
		/// </returns>
		public TopTag[] GetTopTags(int limit)
		{
			return sublist<TopTag>(GetTopTags(), limit);
		}
		
		/// <summary>
		/// Returns the top artists listened-to by this user in a specified period.
		/// </summary>
		/// <param name="period">
		/// A <see cref="Period"/>
		/// </param>
		/// <returns>
		/// A <see cref="TopArtist"/>
		/// </returns>
		public TopArtist[] GetTopArtists(Period period)
		{
			RequestParameters p = getParams();
			p["period"] = getPeriod(period);
			
			XmlDocument doc = request("user.getTopArtists", p);
			List<TopArtist> list = new List<TopArtist>();
			
			foreach(XmlNode node in doc.GetElementsByTagName("artist"))
			{
				Artist artist = new Artist(extract(node, "name"), Session);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				list.Add(new TopArtist(artist, playcount));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns teh overall most listened-to artists by this user.
		/// </summary>
		/// <returns>
		/// A <see cref="TopArtist"/>
		/// </returns>
		public TopArtist[] GetTopArtists()
		{
			return GetTopArtists(Period.Overall);
		}
		
		/// <summary>
		/// Returns the most listened-to albums by this user in a specified period.
		/// </summary>
		/// <param name="period">
		/// A <see cref="Period"/>
		/// </param>
		/// <returns>
		/// A <see cref="TopAlbum"/>
		/// </returns>
		public TopAlbum[] GetTopAlbums(Period period)
		{
			RequestParameters p = getParams();
			p["period"] = getPeriod(period);
			
			XmlDocument doc = request("user.getTopAlbums", p);
			List<TopAlbum> list = new List<TopAlbum>();
			
			foreach(XmlNode node in doc.GetElementsByTagName("album"))
			{
				Album album = new Album(extract(node, "name", 1), extract(node, "name"), Session);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				list.Add(new TopAlbum(album, playcount));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returne the overall most listened-to albums by this user.
		/// </summary>
		/// <returns>
		/// A <see cref="TopAlbum"/>
		/// </returns>
		public TopAlbum[] GetTopAlbums()
		{
			return GetTopAlbums(Period.Overall);
		}
		
		/// <summary>
		/// Returns the most recent played tracks for this user.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] GetRecentTracks(int limit)
		{
			RequestParameters p = getParams();
			p["limit"] = limit.ToString();
			
			XmlDocument doc = request("user.getRecentTracks", p);			
			List<Track> list = new List<Track>();
			
			foreach(XmlNode node in doc.GetElementsByTagName("track"))
			{
				// skip the track that is now playing.
				if (node.Attributes.Count > 0)					
					continue;
				
				list.Add(new Track(extract(node, "artist"), extract(node, "name"), Session));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the most recent played tracks for this user.
		/// </summary>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] GetRecentTracks()
		{
			// default value is 10.
			return GetRecentTracks(10);
		}
		
		/// <summary>
		/// Returns the track that the user's currently listening to.
		/// </summary>
		/// <returns>
		/// A <see cref="Track"/> if the user is listening to a track, or null if they're not.
		/// </returns>
		public Track GetNowPlaying()
		{
			// Would return null if no track is now playing.
			
			RequestParameters p = getParams();
			p["limit"] = "1";
			
			XmlDocument doc = request("user.getRecentTracks", p);
			XmlNode node = doc.GetElementsByTagName("track")[0];
			
			if (node.Attributes.Count > 0)
				return new Track(extract(node, "artist"), extract(node, "name"), Session);
			else
				return null;
		}
		
		/// <summary>
		/// Returns true if the user's listening right now.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsNowListening()
		{
			return (GetNowPlaying() != null);
		}
		
		/// <summary>
		/// Returns this user's playlists.
		/// </summary>
		/// <returns>
		/// A <see cref="Playlist"/>
		/// </returns>
		public Playlist[] GetPlaylists()
		{
			XmlDocument doc = request("user.getPlaylists");
			
			List<Playlist> list = new List<Playlist>();
			foreach(string id in extractAll(doc, "id"))
				list.Add(new Playlist(Name, Int32.Parse(id), Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the past events attended by this user.
		/// </summary>
		public PastEvents PastEvents
		{ get { return new PastEvents(this, Session); } }
		
		/// <summary>
		/// Returns the neighbours of this user.
		/// </summary>
		/// <returns>
		/// A <see cref="User"/>
		/// </returns>
		public User[] GetNeighbours()
		{
			XmlDocument doc = request("user.getNeighbours");
			
			List<User> list = new List<User>();
			foreach(string name in extractAll(doc, "name"))
				list.Add(new User(name, Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the neighbours of this user.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="User"/>
		/// </returns>
		public User[] GetNeighbours(int limit)
		{
			return sublist<User>(GetNeighbours(), limit);
		}
		
		/// <summary>
		/// Returns the most recent 50 loved tracks by this user.
		/// </summary>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] GetLovedTracks()
		{
			XmlDocument doc = request("user.getLovedTracks");
			
			List<Track> list = new List<Track>();
			foreach(XmlNode node in doc.GetElementsByTagName("track"))
				list.Add(new Track(extract(node, "name", 1), extract(node, "name"), Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the user's friends.
		/// </summary>
		/// <returns>
		/// A <see cref="User"/>
		/// </returns>
		public User[] GetFriends()
		{
			XmlDocument doc = request("user.getFriends");
			
			List<User> list = new List<User>();
			foreach(string name in extractAll(doc, "name"))
				list.Add(new User(name, Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the user's friends.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="User"/>
		/// </returns>
		public User[] GetFriends(int limit)
		{
			return sublist<User>(GetFriends(), limit);
		}
		
		/// <summary>
		/// Returns the upcoming events for this user.
		/// </summary>
		/// <returns>
		/// A <see cref="Event"/>
		/// </returns>
		public Event[] GetUpcomingEvents()
		{
			XmlDocument doc = request("user.getEvents");
			
			List<Event> list = new List<Event>();
			foreach(string id in extractAll(doc, "id"))
				list.Add(new Event(Int32.Parse(id), Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Compare this user with another.
		/// </summary>
		/// <param name="anotherUser">
		/// A <see cref="User"/>
		/// </param>
		/// <returns>
		/// A <see cref="Tasteometer"/>
		/// </returns>
		public Tasteometer Compare(User anotherUser)
		{
			return new Tasteometer(this, anotherUser, Session);
		}
		
		/// <summary>
		/// Compare this user with a list of artists.
		/// </summary>
		/// <param name="artists">
		/// A <see cref="IEnumerable`1"/>
		/// </param>
		/// <returns>
		/// A <see cref="Tasteometer"/>
		/// </returns>
		public Tasteometer Compare(IEnumerable<Artist> artists)
		{
			return new Tasteometer(this, artists, Session);
		}
		
		/// <summary>
		/// Compare this user with a myspace profile.
		/// </summary>
		/// <param name="myspaceURL">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="Tasteometer"/>
		/// </returns>
		public Tasteometer Compare(string myspaceURL)
		{
			return new Tasteometer(this, myspaceURL, Session);
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
			
			return "http://" + domain + "/user/" + urlSafe(Name);
		}
		
		/// <summary>
		/// The object's Last.fm page url.
		/// </summary>
		public string URL
		{ get { return GetURL(SiteLanguage.English); } }
	}
}

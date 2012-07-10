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
	/// A Last.fm track.
	/// </summary>
	public class Track : Base, IEquatable<Track>, IShareable, ITaggable, IHasURL
	{
		/// <summary>
		/// The track title.
		/// </summary>
		public string Title {get; private set;}
		
		/// <summary>
		/// The artist.
		/// </summary>
		public Artist Artist {get; private set;}
		
		/// <summary>
		/// The track wiki on Last.fm.
		/// </summary>
		public Wiki Wiki
		{ get { return new TrackWiki(this, Session); } }
    
		public Track(string artistName, string title, Session session)
			:base(session)
		{
			Title = title;
			Artist = new Artist(artistName, Session);
		}
		
		public Track(Artist artist, string title, Session session)
			:base(session)
		{
			Title = title;
			Artist = artist;
		}
		
		/// <summary>
		/// String representation of the object.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString ()
		{
			return this.Artist + " - " + this.Title;
		}
    
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["artist"] = Artist.Name;
			p["track"] = Title;
			
			return p;
		}
		
		/// <summary>
		/// A unique Last.fm ID.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetID()
		{
			XmlDocument doc = request("track.getInfo");
			
			return Int32.Parse(extract(doc, "id"));
		}
		
		/// <summary>
		/// Returns the duration.
		/// </summary>
		/// <returns>
		/// A <see cref="TimeSpan"/>
		/// </returns>
		public TimeSpan GetDuration()
		{
			XmlDocument doc = request("track.getInfo");
			
			// Duration is returned in milliseconds.
			return new TimeSpan(0, 0, 0, 0, Int32.Parse(extract(doc, "duration")));
		}
		
		/// <summary>
		/// Returns true if the track is available for streaming.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsStreamable()
		{
			XmlDocument doc = request("track.getInfo");
			
			int value = Int32.Parse(extract(doc, "streamable"));
			
			if (value == 1)
				return true;
			else
				return false;
		}
		
		/// <summary>
		/// Returns the album of this track.
		/// </summary>
		/// <returns>
		/// A <see cref="Album"/>
		/// </returns>
		public Album GetAlbum()
		{
			XmlDocument doc = request("track.getInfo");
			
			if (doc.GetElementsByTagName("album").Count > 0)
			{
				XmlNode n = doc.GetElementsByTagName("album")[0];
				
				string artist = extract(n, "artist");
				string title = extract(n, "title");
				
				return new Album(artist, title, Session);
			}else{
				return null;
			}
		}
		
		/// <summary>
		/// Ban this track.
		/// </summary>
		public void Ban()
		{
			//This method requires authentication
			requireAuthentication();
			
			request("track.ban");
		}
		
		/// <summary>
		/// Return similar tracks.
		/// </summary>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] GetSimilar()
		{
			XmlDocument doc = request("track.getSimilar");
			
			List<Track> list = new List<Track>();
			
			foreach(XmlNode n in doc.GetElementsByTagName("track"))
			{
				list.Add(new Track(extract(n, "name", 1), extract(n, "name"), Session));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Love this track.
		/// </summary>
		public void Love()
		{
			//This method requires authentication
			requireAuthentication();
			
			request("track.love");
		}
		
		/// <summary>
		/// Check to see if this object equals another.
		/// </summary>
		/// <param name="track">
		/// A <see cref="Track"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Equals(Track track)
		{
			return(track.Title == this.Title && track.Artist.Name == this.Artist.Name);
		}
		
		/// <summary>
		/// Share this track with others.
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
			
			request("track.Share", p);
		}
		
		/// <summary>
		/// Share this track with others.
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
			
			request("track.Share", p);
		}
		
		/// <summary>
		/// Search for tracks on Last.fm.
		/// </summary>
		/// <param name="artist">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="TrackSearch"/>
		/// </returns>
		public static TrackSearch Search(string artist, string title, Session session)
		{
			return new TrackSearch(artist, title, session);
		}
		
		/// <summary>
		/// Search for tracks on Last.fm.
		/// </summary>
		/// <param name="title">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="TrackSearch"/>
		/// </returns>
		public static TrackSearch Search(string title, Session session)
		{
			return new TrackSearch(title, session);
		}
		
		/// <summary>
		/// Add tags to this track.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="Tag"/>
		/// </param>
		public void AddTags(params Tag[] tags)
		{
			//This method requires authentication
			requireAuthentication();
			
			foreach(Tag tag in tags)
			{
				RequestParameters p = getParams();
				p["tags"] = tag.Name;
				
				request("track.addTags", p);
			}
		}
		
		/// <summary>
		/// Add tags to this track.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="System.String"/>
		/// </param>
		public void AddTags(params string[] tags)
		{
			foreach(string tag in tags)
				AddTags(new Tag(tag, Session));
		}
		
		/// <summary>
		/// Add tags to this track.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="TagCollection"/>
		/// </param>
		public void AddTags(TagCollection tags)
		{
			foreach(Tag tag in tags)
				AddTags(tag);
		}
		
		/// <summary>
		/// Returns the tags set by the authenticated user to this track.
		/// </summary>
		/// <returns>
		/// A <see cref="Tag"/>
		/// </returns>
		public Tag[] GetTags()
		{
			//This method requires authentication
			requireAuthentication();
			
			XmlDocument doc = request("track.getTags");
			
			TagCollection collection = new TagCollection(Session);
			
			foreach(string name in this.extractAll(doc, "name"))
				collection.Add(name);
			
			return collection.ToArray();
		}
		
		/// <summary>
		/// Return the top tags.
		/// </summary>
		/// <returns>
		/// A <see cref="TopTag"/>
		/// </returns>
		public TopTag[] GetTopTags()
		{
			XmlDocument doc = request("track.getTopTags");
			
			List<TopTag> list = new List<TopTag>();
			foreach(XmlNode n in doc.GetElementsByTagName("tag"))
				list.Add(new TopTag(new Tag(extract(n, "name"), Session), Int32.Parse(extract(n, "count"))));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the top tags.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="TopTag"/>
		/// </returns>
		public TopTag[] GetTopTags(int limit)
		{
			return this.sublist<TopTag>(GetTopTags(), limit);
		}
		
		/// <summary>
		/// Remove a bunch of tags from this track.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="Tag"/>
		/// </param>
		public void RemoveTags(params Tag[] tags)
		{
			//This method requires authentication
			requireAuthentication();
			
			foreach(Tag tag in tags)
			{
				RequestParameters p = getParams();
				p["tag"] = tag.Name;
				
				request("track.removeTag", p);
			}
		}
		
		/// <summary>
		/// Remove a bunch of tags from this track.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="System.String"/>
		/// </param>
		public void RemoveTags(params string[] tags)
		{
			//This method requires authentication
			requireAuthentication();
			
			foreach(string tag in tags)
				RemoveTags(new Tag(tag, Session));
		}
		
		/// <summary>
		/// Remove a bunch of tags from this track.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="TagCollection"/>
		/// </param>
		public void RemoveTags(TagCollection tags)
		{
			foreach(Tag tag in tags)
				RemoveTags(tag);
		}
		
		/// <summary>
		/// Set the tags of this tracks to only those tags.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="System.String"/>
		/// </param>
		public void SetTags(string[] tags)
		{
			List<Tag> list = new List<Tag>();
			foreach(string name in tags)
				list.Add(new Tag(name, Session));
			
			SetTags(list.ToArray());
		}
		
		/// <summary>
		/// Set the tags of this tracks to only those tags.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="Tag"/>
		/// </param>
		public void SetTags(Tag[] tags)
		{
			List<Tag> newSet = new List<Tag>(tags);
			List<Tag> current = new List<Tag>(GetTags());
			List<Tag> toAdd = new List<Tag>();
			List<Tag> toRemove = new List<Tag>();
			
			foreach(Tag tag in newSet)
				if(!current.Contains(tag))
					toAdd.Add(tag);
			
			foreach(Tag tag in current)
				if(!newSet.Contains(tag))
					toRemove.Add(tag);
			
			if (toAdd.Count > 0)
				AddTags(toAdd.ToArray());
			if (toRemove.Count > 0)
				RemoveTags(toRemove.ToArray());
		}
		
		/// <summary>
		/// Set the tags of this tracks to only those tags.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="TagCollection"/>
		/// </param>
		public void SetTags(TagCollection tags)
		{
			SetTags(tags.ToArray());
		}
		
		/// <summary>
		/// Clear all the tags from this track.
		/// </summary>
		public void ClearTags()
		{
			foreach(Tag tag in GetTags())
				RemoveTags(tag);
		}
		
		/// <summary>
		/// Returns the top fans.
		/// </summary>
		/// <returns>
		/// A <see cref="TopFan"/>
		/// </returns>
		public TopFan[] GetTopFans()
		{
			XmlDocument doc = request("track.getTopFans");
			
			List<TopFan> list = new List<TopFan>();
			foreach(XmlNode node in doc.GetElementsByTagName("user"))
				list.Add(new TopFan(new User(extract(node, "name"), Session), Int32.Parse(extract(node, "weight"))));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the top fans.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="TopFan"/>
		/// </returns>
		public TopFan[] GetTopFans(int limit)
		{
			return sublist<TopFan>(GetTopFans(), limit);
		}
		
		/// <summary>
		/// Returns the track's MusicBrainz id.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetMBID()
		{
			XmlDocument doc = request("track.getInfo");
			
			return doc.GetElementsByTagName("mbid")[0].InnerText;
		}
		
		/// <summary>
		/// Returns a track by its MusicBrainz id.
		/// </summary>
		/// <param name="mbid">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public static Track GetByMBID(string mbid, Session session)
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["mbid"] = mbid;
			
			XmlDocument doc = (new Request("track.getInfo", session, p)).execute();
			
			string title = doc.GetElementsByTagName("name")[0].InnerText;
			string artist = doc.GetElementsByTagName("name")[1].InnerText;
			
			return new Track(artist, title, session);
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
			
			return "http://" + domain + "/music/" + urlSafe(Artist.Name) + "/_/" + urlSafe(Title);
		}

		/// <summary>
		/// The object's Last.fm page url.
		/// </summary>
		public string URL
		{ get { return GetURL(SiteLanguage.English); } }
		
		/// <summary>
		/// Add this track to a <see cref="Playlist"/>
		/// </summary>
		/// <param name="playlist">
		/// A <see cref="Playlist"/>
		/// </param>
		public void AddToPlaylist(Playlist playlist)
		{
			playlist.AddTrack(this);
		}
	}
}
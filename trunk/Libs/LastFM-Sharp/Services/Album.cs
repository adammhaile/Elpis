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
	/// A Last.fm album.
	/// </summary>
	public class Album : Base, IEquatable<Album>, IHasImage, IHasURL
	{
		/// <summary>
		/// The album title.
		/// </summary>
		public string Title {get; private set;}
		
		/// <summary>
		/// The album title/name.
		/// </summary>
		public string Name {get { return Title; } }
		
		/// <summary>
		/// The album's artist.
		/// </summary>
		public Artist Artist {get; private set; }
		
		/// <summary>
		/// The album's wiki on Last.fm.
		/// </summary>
		public AlbumWiki Wiki {get { return new AlbumWiki(this, Session); } }
		
		/// <summary>
		/// Createa an album object.
		/// </summary>
		/// <param name="artistName">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Album(string artistName, string title, Session session)
			:base(session)
		{
			Artist = new Artist(artistName, Session);
			Title = title;
		}
		
		/// <summary>
		/// Create an album.
		/// </summary>
		/// <param name="artist">
		/// A <see cref="Artist"/>
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Album(Artist artist, string title, Session session)
			:base(session)
		{
			Artist = artist;
			Title = title;
		}
		
		/// <summary>
		/// String representation of the object.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString ()
		{
			return Artist.Name + " - " + Title;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["artist"] = Artist.Name;
			p["album"] = Title;
			
			return p;
		}
		
		/// <summary>
		/// Returns the album's MusicBrainz ID if available.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetMBID()
		{
			XmlDocument doc = request("album.getInfo");
			
			return extract(doc, "mbid");
		}
		
		/// <summary>
		/// Returns the album ID on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetID()
		{
			XmlDocument doc = request("album.getInfo");
			
			return extract(doc, "id");
		}
		
		/// <summary>
		/// Returns the album's release date.
		/// </summary>
		/// <returns>
		/// A <see cref="DateTime"/>
		/// </returns>
		public DateTime GetReleaseDate()
		{
			XmlDocument doc = request("album.getInfo");
			
			return DateTime.Parse(extract(doc, "releasedate"));
		}
		
		/// <summary>
		/// Returns the url to the album cover if available.
		/// </summary>
		/// <param name="size">
		/// A <see cref="AlbumImageSize"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL(AlbumImageSize size)
		{
			XmlDocument doc = request("album.getInfo");
			
			return extractAll(doc, "image", 4)[(int)size];
		}
		
		/// <summary>
		/// Returns the url to the album cover if available.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL()
		{
			return GetImageURL(AlbumImageSize.ExtraLarge);
		}
		
		/// <summary>
		/// Returns the number of listeners on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetListenerCount()
		{
			XmlDocument doc = request("album.getInfo");
			
			return Int32.Parse(extract(doc, "listeners"));
		}
		
		/// <summary>
		/// Returns the play count on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetPlaycount()
		{
			XmlDocument doc = request("album.getInfo");
			
			return Int32.Parse(extract(doc, "playcount"));
		}
		
		/// <summary>
		/// Returns an array of the tracks on this album.
		/// </summary>
		/// <returns>
		/// A <see cref="Track"/>
		/// </returns>
		public Track[] GetTracks()
		{
			string url = "lastfm://playlist/album/" + this.GetID();
			
			return (new XSPF(url, Session)).GetTracks();
		}
		
		/// <summary>
		/// Returns the top tags for this album on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="TopTag"/>
		/// </returns>
		public TopTag[] GetTopTags()
		{
			XmlDocument doc = request("album.getInfo");
			XmlNode node = doc.GetElementsByTagName("toptags")[0];
			
			List<TopTag> list = new List<TopTag>();
			foreach(XmlNode n in ((XmlElement)node).GetElementsByTagName("tag"))
			{
				Tag tag = new Tag(extract(n, "name"), Session);
				int count = Int32.Parse(extract(n, "count"));
				
				list.Add(new TopTag(tag, count));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the top tags for this album on Last.fm.
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
		/// Check to see if this object equals another.
		/// </summary>
		/// <param name="album">
		/// A <see cref="Album"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Equals(Album album)
		{
			if(album.Title == this.Title && album.Artist.Name == this.Artist.Name)
				return true;
			else
				return false;
		}
		
		/// <summary>
		/// Search for an album on Last.fm.
		/// </summary>
		/// <param name="albumName">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="AlbumSearch"/>
		/// </returns>
		public static AlbumSearch Search(string albumName, Session session)
		{
			return new AlbumSearch(albumName, session);
		}
		
		/// <summary>
		/// Add tags to this album.
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
				
				request("album.addTags", p);
			}
		}
		
		/// <summary>
		/// Add tags to this album.
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
		/// Add tags to this album.
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
		/// Returns the tags set by the authenticated user to this album.
		/// </summary>
		/// <returns>
		/// A <see cref="Tag"/>
		/// </returns>
		public Tag[] GetTags()
		{
			//This method requires authentication
			requireAuthentication();
			
			XmlDocument doc = request("album.getTags");
			
			TagCollection collection = new TagCollection(Session);
			
			foreach(string name in this.extractAll(doc, "name"))
				collection.Add(name);
			
			return collection.ToArray();
		}
		
		/// <summary>
		/// Remove from your tags on this album.
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
				
				request("album.removeTag", p);
			}
		}
		
		/// <summary>
		/// Remove from your tags on this album.
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
		/// Remove from the authenticated user's tags on this album.
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
		/// Set the authenticated user's tags to only those tags.
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
		/// Set the authenticated user's tags to only those tags.
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
		/// Set the authenticated user's tags to only those tags.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="TagCollection"/>
		/// </param>
		public void SetTags(TagCollection tags)
		{
			SetTags(tags.ToArray());
		}
		
		/// <summary>
		/// Clears all the tags that the authenticated user has set to this album.
		/// </summary>
		public void ClearTags()
		{
			foreach(Tag tag in GetTags())
				RemoveTags(tag);
		}
		
		/// <summary>
		/// Returns an album by it's MusicBrainz id.
		/// </summary>
		/// <param name="mbid">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="Album"/>
		/// </returns>
		public static Album GetByMBID(string mbid, Session session)
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["mbid"] = mbid;
			
			XmlDocument doc = (new Request("album.getInfo", session, p)).execute();
			
			string title = doc.GetElementsByTagName("name")[0].InnerText;
			string artist = doc.GetElementsByTagName("artist")[0].InnerText;
			
			return new Album(artist, title, session);
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
			
			return "http://" + domain + "/music/" + urlSafe(Artist.Name) + "/" + urlSafe(Name);
		}
		
		/// <value>
		/// The Last.fm page of this object.
		/// </value>
		public string URL
		{ get { return GetURL(SiteLanguage.English); } }
	}
}

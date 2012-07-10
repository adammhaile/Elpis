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
	/// An artist on Last.fm
	/// </summary>
	public class Artist : Base, ITaggable, IEquatable<Artist>, IShareable, IHasImage, IHasURL
	{
		/// <summary>
		/// The name of the artist.
		/// </summary>
		public string Name {get; private set;}
		
		/// <summary>
		/// The current wiki version.
		/// </summary>
		public ArtistBio Bio
		{ get { return new ArtistBio(this, Session); } }
		
		public Artist(string name, Session session)
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
			return this.Name;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["artist"] = this.Name;
			
			return p;
		}
		
		/// <summary>
		/// Returns the similar artists to this artist ordered by similarity from the
		/// most similar to the least similar.
		/// </summary>
		/// <param name="limit">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="Artist"/>
		/// </returns>
		public Artist[] GetSimilar(int limit)
		{
			RequestParameters p = getParams();
			if (limit > -1)
				p["limit"] = limit.ToString();
			
			XmlDocument doc = request("artist.getSimilar", p);
      
			string[] names = extractAll(doc, "name");
      
			List<Artist> list = new List<Artist>();
      
			foreach(string name in names)
				list.Add(new Artist(name, Session));
			
			return list.ToArray();
		}

		/// <summary>
		/// Returns the similar artists to this artist ordered by similarity from the
		/// most similar to the least similar.
		/// </summary>
		/// <returns>
		/// A <see cref="Artist"/>
		/// </returns>
		public Artist[] GetSimilar()
		{
			return GetSimilar(-1);
		}
		
		/// <summary>
		/// Returns the total number of listeners on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetListenerCount()
		{
			XmlDocument doc = request("artist.getInfo");
			
			return Convert.ToInt32(extract(doc, "listeners"));
		}
		
		/// <summary>
		/// Returns the total number of playcounts on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int GetPlaycount()
		{
			XmlDocument doc = request("artist.getInfo");
			
			return Convert.ToInt32(extract(doc, "playcount"));
		}
		
		/// <summary>
		/// Returns the url of the artist's image on Last.fm.
		/// </summary>
		/// <param name="size">
		/// A <see cref="ImageSize"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL(ImageSize size)
		{
			XmlDocument doc = request("artist.getInfo");
			
			string[] sizes = extractAll(doc, "image", 3);
			
			return sizes[(int)size];
		}
		
		/// <summary>
		/// Returns the url of the artist's image on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetImageURL()
		{
			return GetImageURL(ImageSize.Large);
		}
		
		/// <summary>
		/// Returns the top most popular tracks by this artist.
		/// </summary>
		/// <returns>
		/// A <see cref="TopTrack"/>
		/// </returns>
		public TopTrack[] GetTopTracks()
		{
			XmlDocument doc = request("artist.getTopTracks");
			
			List<TopTrack> list = new List<TopTrack>();
			
			foreach(XmlNode n in doc.GetElementsByTagName("track"))
			{
				Track track = new Track(extract(n, "name", 1), extract(n, "name"), Session);
				int weight = int.Parse(extract(n, "playcount"));
				
				list.Add(new TopTrack(track, weight));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns a list of upcoming events for this artist.
		/// </summary>
		/// <returns>
		/// A <see cref="Event"/>
		/// </returns>
		public Event[] GetEvents()
		{
			XmlDocument doc = request("artist.getEvents");
			
			List<Event> list = new List<Event>();
			foreach(string id in extractAll(doc, "id"))
				list.Add(new Event(Int32.Parse(id), Session));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the most popular albums by this artist.
		/// </summary>
		/// <returns>
		/// A <see cref="TopAlbum"/>
		/// </returns>
		public TopAlbum[] GetTopAlbums()
		{
			XmlDocument doc = request("artist.getTopAlbums");
			
			List<TopAlbum> list = new List<TopAlbum>();
			foreach(XmlNode n in doc.GetElementsByTagName("album"))
			{
				Album album = new Album(extract(n, "name", 1), extract(n, "name"), Session);
				int weight = int.Parse(extract(n, "playcount"));
				
				list.Add(new TopAlbum(album, weight));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the the top fans for this artist.
		/// </summary>
		/// <returns>
		/// A <see cref="TopFan"/>
		/// </returns>
		public TopFan[] GetTopFans()
		{
			XmlDocument doc = request("artist.getTopFans");
			
			List<TopFan> list = new List<TopFan>();
			foreach(XmlNode node in doc.GetElementsByTagName("user"))
			{
				User user = new User(extract(node, "name"), Session);
				int weight = int.Parse(extract(node, "weight"));
				
				list.Add(new TopFan(user, weight));
			}
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Check to see if this object equals another.
		/// </summary>
		/// <param name="artist">
		/// A <see cref="Artist"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Equals(Artist artist)
		{
			return (artist.Name == this.Name);
		}
		
		/// <summary>
		/// Share this artist with others.
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
			
			request("artist.Share", p);
		}
		
		/// <summary>
		/// Share this artist with others.
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
			
			request("artist.Share", p);
		}
		
		/// <summary>
		/// Search for artists on Last.fm.
		/// </summary>
		/// <param name="artistName">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="ArtistSearch"/>
		/// </returns>
		public static ArtistSearch Search(string artistName, Session session)
		{
			return new ArtistSearch(artistName, session);
		}
		
		/// <summary>
		/// Add one or more tags to this artist.
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
				
				request("artist.addTags", p);
			}
		}
		
		/// <summary>
		/// Add one or more tags to this artist.
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
		/// Add one or more tags to this artist.
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
		/// Returns the tags set by the authenticated user to this artist.
		/// </summary>
		/// <returns>
		/// A <see cref="Tag"/>
		/// </returns>
		public Tag[] GetTags()
		{
			//This method requires authentication
			requireAuthentication();
			
			XmlDocument doc = request("artist.getTags");
			
			TagCollection collection = new TagCollection(Session);
			
			foreach(string name in this.extractAll(doc, "name"))
				collection.Add(name);
			
			return collection.ToArray();
		}
		
		/// <summary>
		/// Returns the top tags of this artist on Last.fm.
		/// </summary>
		/// <returns>
		/// A <see cref="TopTag"/>
		/// </returns>
		public TopTag[] GetTopTags()
		{
			XmlDocument doc = request("artist.getTopTags");
			
			List<TopTag> list = new List<TopTag>();
			foreach(XmlNode n in doc.GetElementsByTagName("tag"))
				list.Add(new TopTag(new Tag(extract(n, "name"), Session), Int32.Parse(extract(n, "count"))));
			
			return list.ToArray();
		}
		
		/// <summary>
		/// Returns the top tags of this artist on Last.fm.
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
		/// Removes from the tags that the authenticated user has set to this artist.
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
				
				request("artist.removeTag", p);
			}
		}
		
		/// <summary>
		/// Removes from the tags that the authenticated user has set to this artist.
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
		/// Removes from the tags that the authenticated user has set to this artist.
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
		/// Sets the tags applied by the authenticated user to this artist to
		/// only those tags. Removing and adding tags as neccessary.
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
		/// Sets the tags applied by the authenticated user to this artist to
		/// only those tags. Removing and adding tags as neccessary.
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
		/// Sets the tags applied by the authenticated user to this artist to
		/// only those tags. Removing and adding tags as neccessary.
		/// </summary>
		/// <param name="tags">
		/// A <see cref="TagCollection"/>
		/// </param>
		public void SetTags(TagCollection tags)
		{
			SetTags(tags.ToArray());
		}

		/// <summary>
		/// Clears the tags that the authenticated user has set to this artist.
		/// </summary>
		public void ClearTags()
		{
			foreach(Tag tag in GetTags())
				RemoveTags(tag);
		}
		
		/// <summary>
		/// Returns an artist by their MusicBrainz artist id.
		/// </summary>
		/// <param name="mbid">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		/// <returns>
		/// A <see cref="Artist"/>
		/// </returns>
		public static Artist GetByMBID(string mbid, Session session)
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["mbid"] = mbid;
			
			XmlDocument doc = (new Request("artist.getInfo", session, p)).execute();
			
			string name = doc.GetElementsByTagName("name")[0].InnerText;
			
			return new Artist(name, session);
		}

		/// <summary>
		/// Returns the artist's MusicBrainz id.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetMBID()
		{
			XmlDocument doc = request("artist.getInfo");
			
			return extract(doc, "mbid");
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
			
			return "http://" + domain + "/music/" + urlSafe(Name);
		}

		/// <summary>
		/// The object's Last.fm page url.
		/// </summary>
		public string URL
		{ get { return GetURL(SiteLanguage.English); } }
		
		/// <summary>
		/// Returns true if the artist's music is available for streaming.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsStreamable()
		{
			XmlDocument doc = request("artist.getInfo");
			
			if (extract(doc, "streamable") == "1")
				return true;
			else
				return false;
		}
	}
}
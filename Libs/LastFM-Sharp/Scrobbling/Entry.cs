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

namespace Lastfm.Scrobbling
{
	
	
	public class Entry
	{
		public string Artist {get; set;}
		public string Title {get; set;}
		public string Album {get; set;}
		public TimeSpan Duration {get; set;}
		public int Number {get; set;}
		public string MusicBrainzID {get; set;}
		public DateTime TimeStarted {get; set;}
		public string RecommendationKey {get; set;}
		public PlaybackSource Source {get; set;}
		public ScrobbleMode Mode {get; set;}
		public string MBID {get; set;}
		
		public Entry(string artist, string title, DateTime timeStarted, PlaybackSource source, TimeSpan duration,
		                   ScrobbleMode mode)
		{
			Artist = artist;
			Title = title;
			TimeStarted = timeStarted;
			Source = source;
			Duration = duration;
			Mode = mode;
		}
		
		public Entry(string artist, string title, DateTime timeStarted, PlaybackSource source, string recommendationKey,
		                   TimeSpan duration, ScrobbleMode mode)
		{
			Artist = artist;
			Title = title;
			TimeStarted = timeStarted;
			Source = source;
			Duration = duration;
			Mode = mode;
			RecommendationKey = recommendationKey;
		}
		
		public Entry(string artist, string title, DateTime timeStarted, PlaybackSource source, TimeSpan duration,
		                   ScrobbleMode mode, string album, int trackNumber, string mbid)
		{
			Artist = artist;
			Title = title;			
			TimeStarted = timeStarted;
			Source = source;
			Duration = duration;
			Mode = mode;
			Album = album;
			Number = trackNumber;
			MBID = mbid;
		}		

		public Entry(string artist, string title, DateTime timeStarted, PlaybackSource source, string recommendationKey,
		                   TimeSpan duration, ScrobbleMode mode, string album, int trackNumber, string mbid)
		{
			Artist = artist;
			Title = title;
			TimeStarted = timeStarted;
			Source = source;
			Duration = duration;
			Mode = mode;
			Album = album;
			Number = trackNumber;
			MBID = mbid;
			RecommendationKey = recommendationKey;
		}
		
		internal RequestParameters getParameters()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			
			// Artist
			p["a"] = Artist;
			
			// Title
			p["t"] = Title;
			
			// Time started
			p["i"] = Utilities.DateTimeToUTCTimestamp(TimeStarted).ToString();
			
			// Playback source
			if (Source == PlaybackSource.User) {
				p["o"] = "P";
			}
			else if (Source == PlaybackSource.NonPersonalizedBroadcast){
				p["o"] = "R";
			}
			else if (Source == PlaybackSource.PersonalizedBroadcast){
				p["o"] = "E";
			}
			else if (Source == PlaybackSource.Lastfm) {
				if (RecommendationKey == "")
					throw new Exception("A recommendation key must be provided if the source for playing this track is Last.fm");
				
				p["o"] = "L";
			}
			else if (Source == PlaybackSource.Unknown) {
				p["o"] = "U";
			}
			
			// Rating
			if (Mode == ScrobbleMode.Banned)
			{
				if (Source != PlaybackSource.Lastfm)
					throw new Exception("Banning is only allowed if source is Last.fm.");
				
				p["r"] = "B";
			}
			else if (Mode == ScrobbleMode.Played)
			{
				p["r"] = "L";
			}
			else if (Mode == ScrobbleMode.Skipped)
			{
				if (Source != PlaybackSource.Lastfm)
					throw new Exception("Banning is only allowed if source is Last.fm.");
				
				p["r"] = "S";
			}
			
			// Duration
			p["l"] = Duration.TotalSeconds.ToString();
			
			// Album
			p["b"] = Album;
			
			// Track number
			if (Number > 0)
				p["n"] = Number.ToString();
			else
				p["n"] = "";
			
			// MBID
			p["m"] = MBID;
			
			
			return p;
		}
		
		public override string ToString ()
		{
			return Artist + " - " + Title + " (" + TimeStarted + ")";
		}
		
		public Lastfm.Services.Track GetInfo(Session session)
		{
			return new Lastfm.Services.Track(this.Artist, this.Title, session);
		}
	}
}

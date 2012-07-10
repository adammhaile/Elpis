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
using System.IO;
using System.Threading;
using System.Text;

namespace Lastfm.Scrobbling
{
	/// <summary>
	/// A classs to be used by a media player,
	/// it caches the tracks to a file first then scrobbles
	/// them asynchronously on another thread.
	/// </summary>
	public class ScrobbleManager
	{
		public Connection Connection {get; set;}
		
		public string CacheDir {get; set;}
		private string cacheFileName {get; set;}
		
		public ScrobbleManager(Connection connection)
		{
			Connection = connection;
			
			// Set the default value for the CacheDir and cacheFileName
			CacheDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + 
			                 "/.lastfm-sharp/" + Connection.ClientID + "/";
			
			init();
		}
		
		public ScrobbleManager(Connection connection, string cacheDir)
		{
			Connection = connection;
			
			CacheDir = cacheDir;
			
			init();
		}
		
		private void init()
		{
			cacheFileName = "scrobbler.cache";
			
			// Create the directory if not exists
			if (!Directory.Exists(CacheDir))
				Directory.CreateDirectory(CacheDir);
		}
		
		/// <summary>
		/// Queues a <see cref="PlayedTrack"/> for scrobbling, then
		/// scrobbles it on another thread. Or at least tries to.
		/// The <see cref="PlayedTrack"/> info is cached locally firt, so
		/// there's no fear of losing precious scrobbles.
		/// </summary>
		/// <param name="track">
		/// A <see cref="PlayedTrack"/>
		/// </param>
		public void Queue(Entry track)
		{
			// Append the scrobble line to the file
			StreamWriter writer = new StreamWriter(CacheDir + cacheFileName, true, Encoding.Unicode);
			writer.WriteLine(track.getParameters().serialize());
			writer.Flush();
			writer.Close();
			
			// Try and submit the whole file now, in another thread.
			Submit();
		}
		
		/// <summary>
		/// Submits the cached scrobbles if any. You shouldn't really concern
		/// yourself with calling. It gets called upon each new queued track.
		/// </summary>
		public void Submit()
		{
			Thread submittingThread = new Thread(new ThreadStart(this.threadedSubmit));
			submittingThread.Start();
		}
		
		/// <summary>
		/// Called internally by the scrobbling thread.
		/// </summary>
		private void threadedSubmit()
		{
			StreamReader reader = new StreamReader(CacheDir + cacheFileName, Encoding.Unicode);			
			string[] lines = reader.ReadToEnd().Trim().Split('\n');
			reader.Close();
			
			foreach (string line in lines)
			{
				Lastfm.RequestParameters p = new Lastfm.RequestParameters(line);

				// Append the "[0]" to the key names
				Lastfm.RequestParameters np = new Lastfm.RequestParameters();
				foreach (string key in p.Keys)
					np[key + "[0]"] = p[key];
				
				Connection.Scrobble(np);
			}
			
			// It won't get to this point unless
			// all the scrobbling was a success.
			File.Delete(CacheDir + cacheFileName);
		}
		
		/// <summary>
		/// Relays a Now Playing report to the underlying
		/// connection. For luxury purposes only.
		/// </summary>
		/// <param name="track">
		/// A <see cref="NowplayingTrack"/>
		/// </param>
		public void ReportNowplaying(NowplayingTrack track)
		{
			Connection.ReportNowplaying(track);
		}
	}
}

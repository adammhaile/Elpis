#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using Un4seen.Bass;
using Un4seen.Bass.Misc;

namespace BassPlayer
{
    internal class StreamCopy : BaseDSP
    {
        private int _stream;
        private BASSBuffer _streamBuffer;
        private BASSFlag _streamFlags;

        public StreamCopy()
        {
        }

        public StreamCopy(int channel, int priority)
            : base(channel, priority, IntPtr.Zero)
        {
        }

        public int Stream
        {
            get { return _stream; }
        }

        public BASSFlag StreamFlags
        {
            get { return _streamFlags; }
            set { _streamFlags = value; }
        }

        public override void OnChannelChanged()
        {
            OnStopped();
            if (base.IsAssigned)
            {
                OnStarted();
            }
        }

        public override void OnStarted()
        {
            int channelBitwidth = base.ChannelBitwidth;
            switch (channelBitwidth)
            {
                case 0x20:
                    _streamFlags &= ~BASSFlag.BASS_SAMPLE_8BITS;
                    _streamFlags |= BASSFlag.BASS_SAMPLE_FLOAT;
                    channelBitwidth = 4;
                    break;

                case 8:
                    _streamFlags &= ~BASSFlag.BASS_SAMPLE_FLOAT;
                    _streamFlags |= BASSFlag.BASS_SAMPLE_8BITS;
                    channelBitwidth = 1;
                    break;

                default:
                    _streamFlags &= ~BASSFlag.BASS_SAMPLE_FLOAT;
                    _streamFlags &= ~BASSFlag.BASS_SAMPLE_8BITS;
                    channelBitwidth = 2;
                    break;
            }
            _streamBuffer = new BASSBuffer(2f, base.ChannelSampleRate, base.ChannelNumChans, channelBitwidth);
            _stream = Bass.BASS_StreamCreate(base.ChannelSampleRate, base.ChannelNumChans, _streamFlags,
                                             null, IntPtr.Zero);
            Bass.BASS_ChannelSetLink(base.ChannelHandle, _stream);
            if (Bass.BASS_ChannelIsActive(base.ChannelHandle) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Bass.BASS_ChannelPlay(_stream, false);
            }
        }

        public override void OnStopped()
        {
            Bass.BASS_ChannelRemoveLink(base.ChannelHandle, _stream);
            Bass.BASS_StreamFree(_stream);
            _stream = 0;
            ClearBuffer();
        }

        public void ClearBuffer()
        {
            if (_streamBuffer != null)
            {
                _streamBuffer.Clear();
            }
        }

        public override void DSPCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            try
            {
                _streamBuffer.Write(buffer, length);
            }
            catch (Exception ex)
            {
                Log.Error("Caught Exception in DSPCallBack. {0}", ex.Message);
            }
        }

        public override string ToString()
        {
            return "StreamCopy";
        }

        // Properties
    }
}
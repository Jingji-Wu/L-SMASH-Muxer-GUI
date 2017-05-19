using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_SMASH___MP4_Muxer.Job
{
    class MuxerJob
    {
        private VideoInfo _videoInfo;
        private List<AudioInfo> _audioInfoList;
        private ChapterInfo _chapterInfo;
        private string _output;
        private double _progressValue;

        public MuxerJob(VideoInfo videoInfo, List<AudioInfo> audioInfoList, ChapterInfo chapterInfo, string output)
        {
            _videoInfo = videoInfo;
            _audioInfoList = audioInfoList;
            _chapterInfo = chapterInfo;
            _output = output;
            _progressValue = 0;
        }

        public double ProgressValue
        {
            get { return _progressValue; }
            set { if (value > -1 || value < 101) _progressValue = value; }
        }

        public string GenerateMuxerArgs()
        {
            string arg_muxer = "";

            arg_muxer += " --file-format mp4";

            int n_track = 0;
            // set video track
            if (!string.IsNullOrEmpty(_videoInfo.Path))
            {
                int video_args = 0;
                arg_muxer += " -i \"" + _videoInfo.Path + "\"";
                if (!string.IsNullOrEmpty(_videoInfo.FPS))
                {
                    arg_muxer += "?fps=" + _videoInfo.FPS;
                    ++video_args;
                }
                if (!string.IsNullOrEmpty(_videoInfo.Name))
                {
                    if (video_args == 0)
                    {
                        arg_muxer += "?handler=" + _videoInfo.Name;
                    }
                    else
                    {
                        arg_muxer += ",handler=" + _videoInfo.Name;
                    }
                    ++video_args;
                }
                if (!string.IsNullOrEmpty(_videoInfo.PAR))
                {
                    if (video_args == 0)
                    {
                        arg_muxer += "?par=" + _videoInfo.PAR;
                    }
                    else
                    {
                        arg_muxer += ",par=" + _videoInfo.PAR;
                    }
                    ++video_args;
                }
                ++n_track;
            }
            // set audio track
            int n_atracks = _audioInfoList.Count;
            for (int i = 0; i < n_atracks; ++i)
            {
                AudioInfo ai = _audioInfoList[i];
                if (!string.IsNullOrEmpty(ai.Path))
                {
                    int audio_args = 0;
                    arg_muxer += " -i \"" + ai.Path + "\"";
                    if (!string.IsNullOrEmpty(ai.Language))
                    {
                        arg_muxer += "?language=" + ai.Language;
                        ++audio_args;
                    }
                    if (!string.IsNullOrEmpty(ai.Name))
                    {
                        if (audio_args == 0)
                        {
                            arg_muxer += "?handler=" + ai.Name;
                        }
                        else
                        {
                            arg_muxer += ",handler=" + ai.Name;
                        }
                        ++audio_args;
                    }
                    if (ai.Delay != 0)
                    {
                        if (audio_args == 0)
                        {
                            arg_muxer += "?encoder-delay=" + ai.Delay;
                        }
                        else
                        {
                            arg_muxer += ",encoder-delay=" + ai.Delay;
                        }
                        ++audio_args;
                    }
                    ++n_track;
                }
            }
            // set chapter track
            if (!string.IsNullOrEmpty(_chapterInfo.Path))
            {
                arg_muxer += " --chapter \"" + _chapterInfo.Path + "\"";
                ++n_track;
            }
            // set output
            if (n_track != 0 && !string.IsNullOrEmpty(_output))
            {
                arg_muxer += " -o \"" + _output + "\"";
            }
            else
            {
                return null;
            }

            return arg_muxer;
        }

        public long TotalSize
        {
            get
            {
                long totalSize = 0;

                if (!string.IsNullOrEmpty(_videoInfo.Path))
                {
                    FileInfo videoFileInfo = new FileInfo(_videoInfo.Path);
                    if (videoFileInfo.Exists)
                    {
                        totalSize += videoFileInfo.Length;
                    }
                }
                foreach (AudioInfo audioInfo in _audioInfoList)
                {
                    if (string.IsNullOrEmpty(audioInfo.Path))
                    {
                        continue;
                    }
                    FileInfo audioFileInfo = new FileInfo(audioInfo.Path);
                    if (audioFileInfo.Exists)
                    {
                        totalSize += audioFileInfo.Length;
                    }
                }
                if (!string.IsNullOrEmpty(_chapterInfo.Path))
                {
                    FileInfo chapterFileInfo = new FileInfo(_chapterInfo.Path);
                    if (chapterFileInfo.Exists)
                    {
                        totalSize += chapterFileInfo.Length;
                    }
                }

                return totalSize;
            }
        }
    }
}

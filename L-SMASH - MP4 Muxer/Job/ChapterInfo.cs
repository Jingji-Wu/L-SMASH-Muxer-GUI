using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_SMASH___MP4_Muxer.Job
{
    class ChapterInfo
    {
        private string _path;

        public ChapterInfo(string path = null)
        {
            _path = path;
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
    }
}

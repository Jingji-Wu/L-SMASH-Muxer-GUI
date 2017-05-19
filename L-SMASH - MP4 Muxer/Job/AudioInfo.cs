using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_SMASH___MP4_Muxer.Job
{
    class AudioInfo
    {
        private string _path;
        private string _language;
        private string _name;
        private int _delay;

        public AudioInfo(string path = null, string language = null, string name = null, int delay = 0)
        {
            _path = path;
            _language = language;
            _name = name;
            _delay = delay;
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string Language
        {
            get { return _language; }
            set { _language = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_SMASH___MP4_Muxer.Job
{
    class VideoInfo
    {
        private string _path;
        private string _fps;
        private string _name;
        private int _PAR_numberator;
        private int _PAR_denominator;

        public VideoInfo(string path = null, string fps = null, string name = null, int PAR_n = 0, int PAR_d = 0)
        {
            _path = path;
            _fps = fps;
            _name = name;
            _PAR_numberator = PAR_n;
            _PAR_denominator = PAR_d;
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string FPS
        {
            get { return _fps; }
            set { _fps = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string PAR
        {
            get
            {
                if (_PAR_numberator == 0 || _PAR_denominator == 0)
                {
                    return null;
                }
                return _PAR_numberator.ToString() + ":" + _PAR_denominator.ToString();
            }
        }
    }
}

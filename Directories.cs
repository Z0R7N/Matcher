using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matcher
{
    class Directories
    {
        private string nameDir;
        private int cnt;

        public Directories(string nameDir, int cnt)
        {
            this.nameDir = nameDir;
            this.cnt = cnt;
        }
        public string NameDir
        {
            get
            {
                return nameDir;
            }
            set
            {
                nameDir = value;
            }
        }
        public int Cnt
        {
            get
            {
                return cnt;
            }
            set
            {
                cnt = value;
            }
        }
    }
}

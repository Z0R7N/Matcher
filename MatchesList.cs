using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matcher
{
    class MatchesList
    {
        private string name;
        private int id;
        private long size;
        private bool check;
        private string fullName;
        private string dir;

        public MatchesList(string name, string fullName, string dir, int id, long size, bool check = false)
        {
            this.fullName = fullName;
            this.name = name;
            this.id = id;
            this.size = size;
            this.dir = dir;
            this.check = check;
        }

        public string Dir
        {
            get
            {
                return dir;
            }
            set
            {
                dir = value;
            }
        }
        public bool Check
        {
            get
            {
                return check;
            }
            set
            {
                check = value;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public long Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        public string FullName
        {
            get
            {
                return fullName;
            }
            set
            {
                fullName = value;
            }
        }
    }
}

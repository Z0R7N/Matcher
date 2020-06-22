using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matcher
{
    class ListRefer
    {
        private string name;
        private int id;
        private long size;
        private bool check;

        public ListRefer()
        {
        }

        public ListRefer (string name, int id, long size, bool check)
        {
            this.name = name;
            this.size = size;
            this.id = id;
            this.check = check;
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
        public string Name { 
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
    }
}

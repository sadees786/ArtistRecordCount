using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtistRecordCount.Model
{
    public class ResultWordCountModel
    {
        public int WordCount { get; set; }
        public List<string> Words { get; set; } = new List<string>();
        
    }
}

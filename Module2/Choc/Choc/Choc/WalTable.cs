using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Choc
{
    public class WalTable
    {
        [JsonProperty(PropertyName = "ID")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "DarkChocProb")]
        public float DarkChocProb { get; set; }

        [JsonProperty(PropertyName = "MilkChocProb")]
        public float MilkChocProb { get; set; }
    }
}

using System.Collections.Generic;
using System.Xml.Serialization;

namespace CCRPGarage.Models
{
    public class Blacklist
    {
        [XmlAttribute]
        public string BypassPermission;
        [XmlArrayItem("Id")]
        public List<ushort> IdList;

        public Blacklist()
        {
            
        }
    }

    public class CategoryBlacklist
    {
        [XmlAttribute]
        public string BypassPermission;
        [XmlArrayItem("Category")]
        public List<string> CategoryList;

        public CategoryBlacklist()
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace KeyFinder.Helpers
{
    class UtilityFunctions
    {
        public static XmlDocument ListToXml<T>(T list, string rootElementName = "root")
        {   
            XmlDocument xmlDoc = new XmlDocument();
            XPathNavigator nav = xmlDoc.CreateNavigator();
            using (XmlWriter writer = nav.AppendChild())
            {
                XmlSerializer ser = new XmlSerializer(typeof(T), new XmlRootAttribute(rootElementName));
                ser.Serialize(writer, list); 
            }
            return xmlDoc;
        }
    }
}

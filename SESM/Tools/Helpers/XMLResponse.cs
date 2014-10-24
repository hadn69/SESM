using System;
using System.Xml.Linq;

namespace SESM.Tools.Helpers
{
    public class XmlResponse
    {
        public XmlResponseType Type;
        public object Content;
        public XDocument xDoc;

        public XmlResponse(XmlResponseType type = XmlResponseType.Error, object content = "Unknow")
        {
            Type = type;
            Content = content;
        }

        public XmlResponse Build()
        {
            xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            xDoc.Add(new XElement("Type", Type.ToString()));
            xDoc.Add(new XElement("Timestamp", DateTime.Now.ToString("O")));
            xDoc.Add(new XElement("Content", Content));
            return this;
        }

        public override string ToString()
        {
            Build();
            return xDoc.Declaration + "\r\n" + xDoc.ToString();
        }
    }

    public enum XmlResponseType
    {
        Success,
        Error
    }
}
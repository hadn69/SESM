using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SESM.Tools.Helpers
{
    public class XmlResponse
    {
        public XmlResponseType Type;
        public object Content;
        public XDocument xDoc;

        public XmlResponse()
        {
            Type = XmlResponseType.Success;
            Content = new List<XElement>();
        }

        public XmlResponse(XmlResponseType type, object content)
        {
            Type = type;
            Content = content;
        }

        public void AddToContent(XElement item)
        {
            ((List<XElement>)Content).Add(item);
        }

        public XmlResponse Build()
        {
            xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            XElement resp = new XElement("Response");
            xDoc.Add(resp);
            resp.Add(new XElement("Type", Type.ToString()));
            resp.Add(new XElement("Timestamp", DateTime.Now.ToString("O")));
            resp.Add(new XElement("Content", Content));
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
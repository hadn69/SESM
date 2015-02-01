using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SESM.Tools.API
{
    public class XMLMessage
    {
        public XmlResponseType Type;
        public object Content;
        public XDocument xDoc;
        public string Code;

        public XMLMessage(string code = "UNKN")
        {
            Type = XmlResponseType.Success;
            Content = new List<XNode>();
            Code = code;
        }

        public XMLMessage(XmlResponseType type, string code, object content)
        {
            Type = type;
            Code = code;
            Content = content;
            
        }

        public void AddToContent(XNode item)
        {
            ((List<XNode>)Content).Add(item);
        }

        public XMLMessage Build()
        {
            xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            XElement resp = new XElement("Response");
            xDoc.Add(resp);
            resp.Add(new XElement("Type", Type.ToString()));
            resp.Add(new XElement("Timestamp", DateTime.Now.ToString("o")));
            resp.Add(new XElement("ReturnCode", Code));
            resp.Add(new XElement("Content", Content));
            return this;
        }

        public override string ToString()
        {
            Build();
            return xDoc.Declaration + xDoc.ToString(SaveOptions.DisableFormatting);
        }

        public static XMLMessage Error(string code, string message)
        {
            return new XMLMessage(
                XmlResponseType.Error,
                code,
                message);
        }
        public static XMLMessage Warning(string code, string message)
        {
            return new XMLMessage(
                XmlResponseType.Warning,
                code,
                message);
        }
        public static XMLMessage Success(string code, string message)
        {
            return new XMLMessage(
                XmlResponseType.Success,
                code,
                message);
        }
    }

    public enum XmlResponseType
    {
        Success,
        Warning,
        Error
    }
}
/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of PandoraSharp.
 * PandoraSharp is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * PandoraSharp is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with PandoraSharp. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using PandoraSharp.Exceptions;

namespace PandoraSharp
{
    public class XmlRPC
    {
        public static XmlReader GetReader(string data)
        {
            var reader = new XmlTextReader(new StringReader(data)) { WhitespaceHandling = WhitespaceHandling.Significant };
            return reader;
        }

        public static string GetFaultString(PDict dict)
        {
            if (dict.ContainsKey("faultString"))
            {
                var fault = ((string) dict["faultString"]);
                string[] split = fault.Split('|');
                if (split.Length >= 3)
                    return ((string) dict["faultString"]).Split('|')[2];
                else
                    return fault;
            }
            else
                return string.Empty;
        }

        public static string ResponseFaultCheck(string data)
        {
            try
            {
                object results = ParseXML(data);
                if (results.GetType() == typeof (PDict))
                    return GetFaultString((PDict) results);

                return string.Empty;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static object ParseXML(string data)
        {
            try
            {
                object results = null;
                using (XmlReader reader = GetReader(data))
                {
                    if (reader.ReadToDescendant("value"))
                    {
                        results = parseValue(reader);
                        reader.ReadEndElement();
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                throw new XmlRPCException("Error Parsing XML", e);
            }
        }

        private static object parseValue(XmlReader reader)
        {
            object result = null;
            if (!reader.Read())
                return false;

            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "struct":
                        result = parseStruct(reader);
                        break;
                    case "int":
                        result = reader.ReadElementContentAsInt();
                        break;
                    case "boolean":
                        result = reader.ReadElementContentAsBoolean();
                        break;
                    case "array":
                        result = parseArray(reader);
                        break;
                    default:
                        return null;
                }
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                result = reader.ReadString().XmlDecode();
            }
            else
            {
                result = string.Empty;
            }

            reader.ReadEndElement();

            return result;
        }

        private static object[] parseArray(XmlReader reader)
        {
            var array = new List<object>();
            if (reader.ReadToDescendant("value"))
            {
                do
                {
                    array.Add(parseValue(reader));
                } while (reader.NodeType == XmlNodeType.Element || reader.ReadToNextSibling("value"));
            }

            reader.ReadEndElement();
            return array.ToArray();
        }

        private static PDict parseStruct(XmlReader reader)
        {
            var results = new PDict();

            reader.ReadToDescendant("member");
            do
            {
                reader.ReadToDescendant("name");
                string name = reader.ReadElementContentAsString();

                results.Add(name, parseValue(reader));
            } while (reader.ReadToNextSibling("member"));

            reader.ReadEndElement();

            return results;
        }

        public static string GenerateValue(object v)
        {
            Type t = v.GetType();
            if (t == typeof (string))
                return string.Format("<value><string>{0}</string></value>",
                                     ((string) v).XmlEncode());
            else if (t == typeof (bool))
                if (((bool) v))
                    return "<value><boolean>1</boolean></value>";
                else
                    return "<value><boolean>0</boolean></value>";
            else if (t == typeof (int))
                return string.Format("<value><int>{0}</int></value>",
                                     ((int) v).ToString());
            else if (t == typeof (object[]))
            {
                string result = string.Empty;
                foreach (object obj in ((object[]) v))
                {
                    result += string.Format("<value><array><data>{0}</data></array></value>",
                                            GenerateValue(obj));
                }
                return result;
            }
            else
                throw new ArgumentException(
                    string.Format("Can't encode {0} of type {1} tp XMLRPC",
                                  v, t));
        }

        public static string GenerateRPCXml(string method, object[] args)
        {
            string argString = string.Empty;
            foreach (object arg in args)
                argString += string.Format("<param>{0}</param>", GenerateValue(arg));

            return
                string.Format(
                    "<?xml version=\"1.0\"?><methodCall><methodName>{0}</methodName><params>{1}</params></methodCall>",
                    method, argString);
        }
    }
}
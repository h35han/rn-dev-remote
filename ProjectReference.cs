using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace RN_Dev_Assistant
{
    public class ProjectReference
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string UpdatedDate { get; set; }

        public static string GetName(string path)
        {
            string manifestPath = path + @"\android\app\src\main\res\values\strings.xml";
            if (File.Exists(manifestPath))
            {
                XElement manifest = XElement.Load(manifestPath);
                IEnumerable<XElement> address =
                    from el in manifest.Elements("string")
                    where (string)el.Attribute("name") == "app_name"
                    select el;

                foreach (XElement el in address)
                    return el.Value;
            }
            return null;
        }

        public static string GetUpdatedDate(string path)
        {
            if (Directory.Exists(path))
                return Directory.GetLastWriteTime(path).ToString("MM/dd/yyyy HH:mm:ss");
            return null;
        }
    }
}

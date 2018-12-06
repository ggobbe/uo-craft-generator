using System;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace item_generator
{
    class Program
    {
        private const string TagPrefix = "{{";
        private const string TagSuffix = "}}";
        
        private const string SwitchPrefix = "[";
        private const string SwitchSuffix = "]";

        static void Main(string[] args)
        {
            GenerateCode("items.csv", "template-item.txt", "output-items.txt");
            GenerateCode("items.csv", "template-def.txt", "output-def.txt");
        }

        private static void GenerateCode(string csvFile, string templateFile, string outputFile)
        {
            using (var srItems = new StreamReader(csvFile))
            using (var template = new StreamReader(templateFile))
            using (var csv = new CsvReader(srItems, new Configuration {Delimiter = ","}))
            using (var output = new StreamWriter(outputFile))
            {
                var itemTemplateCode = template.ReadToEnd();

                csv.Read();
                csv.ReadHeader();
                var headerRow = csv.Context.HeaderRecord;

                while (csv.Read())
                {
                    var itemCode = new StringBuilder(itemTemplateCode);

                    foreach (var key in headerRow)
                    {
                        var tag = string.Format("{0}{1}{2}", TagPrefix, key, TagSuffix);
                        var value = csv[key];

                        if (key.StartsWith("if:"))
                        {
                            if (!bool.Parse(value))
                            {
                                continue;
                            }

                            value = string.Empty;
                        }

                        if (key.StartsWith("switch:"))
                        {
                            tag = string.Format("{0}{1}{2}{3}{4}{5}", TagPrefix, key, SwitchPrefix, value, SwitchSuffix, TagSuffix);
                            value = string.Empty;
                        }

                        itemCode = itemCode.Replace(tag, value);
                    }

                    output.Write(RemoveComments(itemCode.ToString()));
                }
            }
        }

        private static string RemoveComments(string input)
        {
            var ifPrefix = string.Format("{0}if:", TagPrefix);
            var switchPrefix = string.Format("{0}switch:", TagPrefix);
            
            var lines = input.Split(Environment.NewLine).Where(l => !l.TrimStart().StartsWith(ifPrefix) && !l.TrimStart().StartsWith(switchPrefix));
            return string.Join(Environment.NewLine, lines);
        }
    }
}
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace item_generator
{
    class Program
    {
        private const string TagPrefix = "{{";
        private const string TagSuffix = "}}";

        static void Main(string[] args)
        {
            using (var srItems = new StreamReader("items.csv"))
            using (var srTemplate = new StreamReader("template.txt"))
            using (var csv = new CsvReader(srItems, new Configuration {Delimiter = ","}))
            using (var swOutput = new StreamWriter("output.txt"))
            {
                var templateText = srTemplate.ReadToEnd();

                csv.Read();
                csv.ReadHeader();
                var headerRow = csv.Context.HeaderRecord;

                while (csv.Read())
                {
                    var output = new StringBuilder(templateText);
                    foreach (var key in headerRow)
                    {
                        var tag = new StringBuilder().Append(TagPrefix).Append(key).Append(TagSuffix);
                        var value = csv[key];
                        output = output.Replace(tag.ToString(), value);
                    }

                    output.Append("\n\n");
                    swOutput.Write(output);
                }
            }
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testna_JsonToHtml
{
    class Program
    {
        public static void JsonToHtml(string json)
        {
            #region spremenljivke
            List<string> metaItems = new List<string>(); //List, da lažje ustvarimo meta značko
            metaItems.Add("description");
            metaItems.Add("keywords");
            metaItems.Add("author");
            metaItems.Add("viewport");
            JObject html = null; // Osnovni JSON object
            IList<JToken> body = null; // Body ter vsi gnezdeni elementi
            IList<JToken> meta = null; // Meta ter vsi gnezdeni atributi
            string output = "output.html";
            string charset = null;
            string title = null;
            string doctype = null;
            string language = null;
            StringBuilder sb = new StringBuilder(); //Končen string - HTML
            #endregion

            #region Try-Catch-Bloki
            try
            {
                html = JObject.Parse(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("NAPAKA: Vaša JSON datoteka je v nepravilnem formatu. Točen error: \n" + e.Message);
                Console.ReadLine();
                Environment.Exit(0);
            }
             
            try
            {
                body = html["body"].Children().ToList();
            }
            catch
            {
                Console.WriteLine("WARNING: Vaš HTML ne vsebuje elementa \"body\"");
            }

            try
            {
                meta = html["head"]["meta"].Children().ToList();
                charset = (string)html["head"]["meta"]["charset"];
                title = (string)html["head"]["title"];
                doctype = html.Property("doctype").Value.ToString();
                language = html.Property("language").Value.ToString();
            }
            catch
            {

            }
            #endregion

          
            // Pre head
            if (!String.IsNullOrEmpty(doctype))
                {
                    sb.Append("<!doctype " + doctype + ">");
                    sb.Append("\n");
                }

            if (String.IsNullOrEmpty(language))
            {
                sb.Append("<html>\n");
            }
            else
            {
                sb.Append("<html lang=\"" + language + "\">");
                sb.Append("\n");
            }
            // End pre head
            #region head
            //Head

            sb.Append("<head>\n");

            //Meta

            if (charset != null)
            {
                sb.Append("\t<meta ");
                sb.Append("charset=\"" + charset + "\"");
                sb.Append(">\n");
            }


            if(meta != null)
            {
                foreach (JProperty metaItem in meta) 
                {
                    if (metaItems.Contains(metaItem.Name)) //Če prepoznamo meta značko
                    {
                        IList<JToken> content = html["head"]["meta"][metaItem.Name].Children().ToList();
                        sb.Append("\t<meta name=\"" + metaItem.Name + "\"" + " content=\"");
                        int counter = 1;
                        foreach (JProperty item in content)
                        {
                            if (metaItem.Name == "viewport") // Viewport ima drugačen content
                            {
                                sb.Append(item.Name + "=" + item.Value);
                                if (counter != content.Count) //Če counter ni zadnji element, dodamo vejico
                                {
                                    sb.Append(",");
                                }
                                counter++;
                            }
                            else
                            {
                                sb.Append(item.Value);
                            }
                        }
                        sb.Append("\"");
                        sb.Append(">\n");
                    }
                }
            }
         
            //End meta

            if (title != null)
            {
                sb.Append("\t<title>" + title + "</title>\n");
            }

            sb.Append("</head>\n");

            //End head
            #endregion

            #region body
            //Body
            sb.Append("<body>\n");
            if (body != null)
            {
                foreach (JProperty item in body)
                {
                    if (html["body"][item.Name].Children().ToList().Count() == 0) //Če element nima atributov
                    {
                        sb.Append("\t<" + item.Name + ">" + item.Value + "</" + item.Name + ">\n");
                    }
                    else
                    {
                        IList<JToken> element = html["body"][item.Name].Children().ToList();
                        string value = ""; //"Value" je vrednost elementa, in ne atribut.
                        sb.Append("\t<" + item.Name + " ");
                        foreach (JProperty attribute in element)
                        {

                            if (attribute.Name != "Value")
                            {
                                sb.Append(attribute.Name + "=\"" + attribute.Value + "\"");
                            }
                            else
                            {
                                value = attribute.Value.ToString(); //Value shranimo v spremenljivko, ter ga dodamo na koncu
                            }
                        }
                        sb.Append(">" + value + "</" + item.Name + ">\n");
                    }
                }
            }

            sb.Append("</body>\n");

            //End body
            #endregion

            sb.Append("</html>\n");

            try
            {
                using (FileStream fs = new FileStream(output, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sb.ToString());
                }
                Console.WriteLine("Pretvarjanje iz input.json v " + output + " uspešno!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            catch
            {
                Console.WriteLine("Napaka pri pisanju datoteke v datoteko "+ output + "!");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        static void Main(string[] args)
        {
            string input = "input.json";
            string json = "";
            try 
            {
                StreamReader r = new StreamReader(input);
                json = r.ReadToEnd();
            }
            catch
            {
                Console.WriteLine("Datoteke "+ input + " ni bilo mogoče najti. Nahajati se more v isti mapi kot je .EXE datoteka.");
                Console.ReadLine();
                Environment.Exit(0);
            }
            JsonToHtml(json);
        }
    }
}

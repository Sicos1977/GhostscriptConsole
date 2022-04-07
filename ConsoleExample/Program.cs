using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ConsoleExample
{
    internal class Program
    {
        private static GSNET gsNet = new GSNET();

        #region GeneratePostScriptFileName
        private static string GeneratePostScriptFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            var temp = fileName.Replace("\\", "\\\\");
            temp = temp.Replace("(", "\\(");
            temp = temp.Replace(")", "\\)");
            temp = temp.Replace("{", "\\{");
            temp = temp.Replace("}", "\\}");
            temp = temp.Replace("[", "\\[");
            temp = temp.Replace("]", "\\]");
            temp = temp.Replace("<", "\\<");
            temp = temp.Replace(">", "\\>");
            temp = temp.Replace("%", "\\%");
            temp = temp.Replace("/", "\\/");

            var result = string.Empty;

            foreach (var chr in temp)
            {
                if (chr < 32 || chr > 126)
                {
                    var bytes = Encoding.UTF8.GetBytes(chr.ToString());
                    foreach (var b in bytes)
                    {
                        var octal = string.Empty;

                        var quotient = (int)b;
                        while (quotient != 0)
                        {
                            octal += quotient % 8;
                            quotient /= 8;
                        }

                        var array = octal.ToCharArray();
                        Array.Reverse(array);
                        result += "\\" + new string(array);
                    }
                }
                else
                    result += chr;
            }

            return result;
        }
        #endregion

        #region GetPdfInfo
        public static void GetPdfInfo(string inputFile)
        {
            var sb = new StringBuilder();

            var args = new List<string>
            {
                "gs",
                "-q",
                "-dQUIET",
                "-dNEWPDF=false",
                $"--permit-file-read={inputFile}"
            };

            sb.AppendLine("/putchar");
            sb.AppendLine("[");
            sb.AppendLine("    (%stdout) (w) file");
            sb.AppendLine("    /exch cvx /write cvx");
            sb.AppendLine("] cvx bind def");
            sb.AppendLine();
            sb.AppendLine("/put-ucode");
            sb.AppendLine("{");
            sb.AppendLine("    dup 16#80 ge");
            sb.AppendLine("    {");
            sb.AppendLine("        dup 16#800 ge");
            sb.AppendLine("        {");
            sb.AppendLine("            dup 16#10000 ge"); 
            sb.AppendLine("            {");
            sb.AppendLine("                dup -18 bitshift 16#f0 or putchar");
            sb.AppendLine("                dup -12 bitshift 16#3f and 16#80 or putchar");
            sb.AppendLine("            }");
            sb.AppendLine("            {");
            sb.AppendLine("                dup -12 bitshift 16#e0 or putchar");
            sb.AppendLine("            } ifelse");
            sb.AppendLine("            dup -6  bitshift 16#3f and 16#80 or putchar");
            sb.AppendLine("        }");
            sb.AppendLine("        {");
            sb.AppendLine("            dup -6 bitshift 16#C0 or putchar");
            sb.AppendLine("        } ifelse");
            sb.AppendLine("        16#3f and 16#80 or");
            sb.AppendLine("    } if");
            sb.AppendLine("    putchar");
            sb.AppendLine("} bind def");
            sb.AppendLine();
            sb.AppendLine("/doc-to-ucode");
            sb.AppendLine("[");
            sb.AppendLine("    0 1 23 {} for");
            sb.AppendLine("    16#2d8 16#2c7 16#2c6 16#2d9 16#2dd 16#2db 16#2da 16#2dc");
            sb.AppendLine("    32 1 127 {} for");
            sb.AppendLine("    16#2022 16#2020 16#2021 16#2026 16#2014 16#2013 16#192");
            sb.AppendLine("    16#2044 16#2039 16#203a 16#2212 16#2030 16#201e 16#201c");
            sb.AppendLine("    16#201d 16#2018 16#2019 16#201a 16#2122 16#fb01 16#fb02");
            sb.AppendLine("    16#141 16#152 16#160 16#178 16#17d 16#131 16#142 16#153");
            sb.AppendLine("    16#161 16#17e 0 16#20ac");
            sb.AppendLine("    161 1 255 {} for");
            sb.AppendLine("] readonly def");
            sb.AppendLine();
            sb.AppendLine("/write-doc-string"); 
            sb.AppendLine("{");
            sb.AppendLine("    1024 string cvs <feff> anchorsearch");
            sb.AppendLine("    {");
            sb.AppendLine("        pop");
            sb.AppendLine("        0 exch");
            sb.AppendLine("        0 2 2 index length 2 sub");
            sb.AppendLine("        {");
            sb.AppendLine("            2 copy 2 copy");
            sb.AppendLine("            get 256 mul 3 1 roll");
            sb.AppendLine("            1 add get add");
            sb.AppendLine("            dup 16#fc00 and dup");
            sb.AppendLine("            16#d800 eq");
            sb.AppendLine("            {");
            sb.AppendLine("                pop");
            sb.AppendLine("                16#3ff and");
            sb.AppendLine("                10 bitshift");
            sb.AppendLine("                16#10000 add");
            sb.AppendLine("                4 1 roll");
            sb.AppendLine("                pop exch pop");
            sb.AppendLine("            }");
            sb.AppendLine("            {");
            sb.AppendLine("                16#dc00 eq");
            sb.AppendLine("                {");
            sb.AppendLine("                    16#3ff and");
            sb.AppendLine("                    4 -1 roll add");
            sb.AppendLine("                    put-ucode");
            sb.AppendLine("                    pop 0 exch");
            sb.AppendLine("                }");
            sb.AppendLine("                {");
            sb.AppendLine("                    put-ucode");
            sb.AppendLine("                    pop");
            sb.AppendLine("                } ifelse");
            sb.AppendLine("            } ifelse");
            sb.AppendLine("        } for");
            sb.AppendLine("        pop pop");
            sb.AppendLine("    }");
            sb.AppendLine("    {");
            sb.AppendLine("        {");
            sb.AppendLine("            //doc-to-ucode exch get put-ucode"); 
            sb.AppendLine("        } forall");
            sb.AppendLine("    } ifelse");
            sb.AppendLine("} bind def");
            sb.AppendLine();
            sb.AppendLine($"({GeneratePostScriptFileName(inputFile)}) (r) file "); 
            sb.AppendLine("{");
            sb.AppendLine("    runpdfbegin Trailer /Info knownoget ");
            sb.AppendLine("    { ");
            sb.AppendLine("        dup /Title knownoget { (Title: ) print write-doc-string () = flush } if");
            sb.AppendLine("        dup /Author knownoget { (Author: ) print write-doc-string () = flush } if");       
            sb.AppendLine("        dup /Subject knownoget { (Subject: ) print write-doc-string () = flush } if");       
            sb.AppendLine("        dup /Keywords knownoget { (Keywords: ) print write-doc-string () = flush } if");       
            sb.AppendLine("        dup /Creator knownoget { (Creator: ) print write-doc-string () = flush } if");       
            sb.AppendLine("        dup /Producer knownoget { (Producer: ) print write-doc-string () = flush } if");       
            sb.AppendLine("        dup /CreationDate knownoget { (CreationDate: ) print write-doc-string () = flush } if");       
            sb.AppendLine("        dup /ModDate knownoget { (ModDate: ) print write-doc-string () = flush } if");    
            sb.AppendLine("    } if"); 
            sb.AppendLine("} stopped"); 
            sb.AppendLine("{");    
            sb.AppendLine("    (Error: Trailer corrupt\\n) print"); 
            sb.AppendLine("} if"); 
            sb.AppendLine();
            sb.AppendLine("(Pages: ) print pdfpagecount = flush"); 
            sb.AppendLine();
            sb.AppendLine("1 1 pdfpagecount");  
            sb.AppendLine("{");     
            sb.AppendLine("    dup ( - page: ) print =print ( - ) print");   
            sb.AppendLine("    pdfgetpage dup");     
            sb.AppendLine("    /MediaBox pget");
            sb.AppendLine("    {");
            sb.AppendLine("        /MediaBox exch def ");
            sb.AppendLine("        (left ) print MediaBox 0 get =print");
            sb.AppendLine("        (, top ) print MediaBox 1 get =print");
            sb.AppendLine("        (, width ) print MediaBox 2 get =print");
            sb.AppendLine("        (, height ) print MediaBox 3 get =print");
            sb.AppendLine("    } ");
            sb.AppendLine("    { ");
            sb.AppendLine("        (left 0, top 0, width 0, height 0) print");     
            sb.AppendLine("    } ifelse");
            sb.AppendLine();   
            sb.AppendLine("    dup /Rotate pget");     
            sb.AppendLine("    { ");        
            sb.AppendLine("        (, rotate ) print =print");     
            sb.AppendLine("    }");    
            sb.AppendLine("    {");        
            sb.AppendLine("        (, rotate 0 ) print");     
            sb.AppendLine("    } ifelse");     
            sb.AppendLine("    (, annotations ) print");
            sb.AppendLine("    dup /Annots pget");     
            sb.AppendLine("    {");        
            sb.AppendLine("        (yes) print");     
            sb.AppendLine("    }");     
            sb.AppendLine("    {");        
            sb.AppendLine("        (no) print");     
            sb.AppendLine("    } ifelse");     
            sb.AppendLine("    (\\n) print");
            sb.AppendLine("} for flush");

            // ReSharper disable once AssignNullToNotNullAttribute
            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile), $"{Path.GetFileNameWithoutExtension(inputFile)}_pdfinfo.ps");
            File.WriteAllText(outputFile, sb.ToString());

            args.Add(outputFile);

            var input = new gsParamState_t();
            input.args = args;

            gsNet.StdIOCallBack += delegate(string mess, int len) { Console.WriteLine(mess); };

            var result = gsNet.gsFileSync(input);
        }
        #endregion

        

        static void Main(string[] args)
        {
            for (var i = 0; i < 100; i++)
            {
                Debug.Print(i.ToString());
                GetPdfInfo(@"d:\test\test.pdf");
            }
        }
    }
}

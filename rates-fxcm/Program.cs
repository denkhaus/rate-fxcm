using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mono.Options;


namespace ratesfxcm
{
	class MainClass
	{
        private static ExpandoObject ParserOptions(IEnumerable<string> args)
        {
            dynamic expando = new ExpandoObject();
            
            expando.Verbosity = 0;
            expando.Filter = String.Empty;

            var p = new OptionSet{
                { "f|filter=", "filter the Symbol by regex.", v => expando.Filter = v },
                { "v", "increase debug message verbosity", v => { if (v != null) ++expando.Verbosity; } },
                { "h|help",  "get command help", v => expando.ShowHelp = v != null }
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("rates-fxcm: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `rates-fxcm --help' for more information.");
            }

            return expando;
        }

	    private static void ParseRates(dynamic opts)
	    {
            try
            {
                if (opts.Verbosity > 0)
                {
                    Console.WriteLine("Fetch Rates");
                }

                var xdoc = XDocument.Load("http://rates.fxcm.com/RatesXML");

                var data = xdoc.Descendants("Rate").Select(r => new
                {
                    Symbol = r.Attribute("Symbol").Value,
                    Bid = r.Element("Bid").Value,
                    Ask = r.Element("Ask").Value,
                    DateTime = r.Element("Last").Value
                }).ToArray();

                if (opts.Verbosity > 0)
                {
                    Console.WriteLine("{0} DataSets received.", data.Count());
                }

                if (!string.IsNullOrEmpty(opts.Filter))
                {
                    if (opts.Verbosity > 0)
                    {
                        Console.WriteLine("Apply Filter: {0}", opts.Filter);
                    }

                    var regex = new Regex(opts.Filter,RegexOptions.IgnoreCase|RegexOptions.Singleline);
                    data = data.Where(r => regex.Match(r.Symbol).Success).ToArray();
                }

                var result = new StringBuilder();

                if (opts.Verbosity > 0)
                {
                    Console.WriteLine("{0} Results:", data.Count());
                }

                foreach (var rate in data)
                {
                    result.AppendLine(string.Format("{0}|{1}|{2}|{3}", rate.Symbol, rate.Bid, rate.Ask, rate.DateTime));
                }

                Console.Write(result.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error::{0} StackTrace::{1}", ex.Message, ex.StackTrace);
            }
	    }



	    public static void Main (string[] args)
	    {
	        var opts = ParserOptions(args);
	        ParseRates(opts);
	    }

	    
	}
}

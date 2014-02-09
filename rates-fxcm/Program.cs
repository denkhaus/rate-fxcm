using System;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NDesk.Options;
using NDesk.Options.Extensions;


namespace ratesfxcm
{
	class MainClass
	{
		private static ExpandoObject ParserOptions(string[] args)
        {
			var CONSOLE_NAME = "rates-fxcm";
            dynamic expando = new ExpandoObject();
            
			expando.Parsed = false;
			expando.Verbose = false;
            expando.Filter = String.Empty;

			var os = new RequiredValuesOptionSet(); 

            try
            {
				var filter = os.AddVariable<string> ("f|filter", "filter the symbol by regex.");
				var verbose = os.AddSwitch("v|verbose", "get additional infos");

				var manager = new ConsoleManager(CONSOLE_NAME, os, "h|help",  "get command help");

				if(manager.TryParseOrShowHelp(Console.Out, args))
				{
					expando.Filter = filter.Value;
					expando.Verbose = verbose.Enabled;
					expando.Parsed =   true;
				}
			} 
            catch (OptionException e)
            {
				Console.Write(CONSOLE_NAME + ": ");
                Console.WriteLine(e.Message);
				Console.WriteLine("Try `" + CONSOLE_NAME + " --help' for more information.");
            }

            return expando;
        }

	    private static void ParseRates(dynamic opts)
	    {
            try
            {
				if (opts.Verbose)
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

				if (opts.Verbose)
                {
                    Console.WriteLine("{0} DataSets received.", data.Count());
                }

                if (!string.IsNullOrEmpty(opts.Filter))
                {
					if (opts.Verbose)
                    {
                        Console.WriteLine("Apply Filter: {0}", opts.Filter);
                    }

                    var regex = new Regex(opts.Filter,RegexOptions.IgnoreCase|RegexOptions.Singleline);
                    data = data.Where(r => regex.Match(r.Symbol).Success).ToArray();
                }

                var result = new StringBuilder();

				if (opts.Verbose)
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
			dynamic opts = ParserOptions(args);

			if(opts.Parsed)
	        	ParseRates(opts);
	    }

	    
	}
}

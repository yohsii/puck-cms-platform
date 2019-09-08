using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using puck.core.Base;
using puck.core.Helpers;
using puck.core.Models;

namespace puck.core.Extensions
{
    public static class ExtensionsMisc
    {
        public static string Highlight(this string text,string term)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            var bq = new BooleanQuery();
            term.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList()
                .ForEach(x => bq.Add(new TermQuery(new Term("field", x)), Occur.SHOULD));
            var fragmentLength = 100;
            var highlightStartTag = @"<span class='search_highlight'>";
            var highlightEndTag = @"</span>";
            QueryScorer scorer = new QueryScorer(bq);
            var formatter = new SimpleHTMLFormatter(highlightStartTag, highlightEndTag);
            Highlighter highlighter = new Highlighter(formatter, scorer);
            highlighter.TextFragmenter = new SimpleFragmenter(fragmentLength);
            TokenStream stream = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48).GetTokenStream("field", new StringReader(text));
            return highlighter.GetBestFragments(stream, text, 100, "...");
        }
    }
}
